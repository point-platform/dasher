using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class ValueTupleProvider : ITypeProvider
    {
        public bool CanProvide(Type type) => type.IsConstructedGenericType && type.FullName.StartsWith("System.ValueTuple`");

        public bool UseDefaultNullHandling(Type valueType) => false;

        public bool TryEmitSerialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            var tupleType = value.LocalType;
            var tupleSize = tupleType.GenericTypeArguments.Length;

            Debug.Assert(tupleSize > 1);

            // write the array header
            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldc_I4, tupleSize);
            ilg.Emit(OpCodes.Call, Methods.Packer_PackArrayHeader);

            var success = true;

            // write each item's value
            var i = 1;
            foreach (var itemType in tupleType.GenericTypeArguments)
            {
                ilg.Emit(OpCodes.Ldloc, value);
                ilg.Emit(OpCodes.Ldfld, tupleType.GetField($"Item{i}"));
                var local = ilg.DeclareLocal(itemType);
                ilg.Emit(OpCodes.Stloc, local);
                if (!SerialiserEmitter.TryEmitSerialiseCode(ilg, throwBlocks, errors, local, packer, context, contextLocal))
                {
                    errors.Add($"Unable to serialise tuple item of type {itemType}");
                    success = false;
                }
                i++;
            }

            return success;
        }

        public bool TryEmitDeserialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            var tupleType = value.LocalType;
            var tupleSize = tupleType.GenericTypeArguments.Length;

            Debug.Assert(tupleSize > 1);

            // read array length
            var count = ilg.DeclareLocal(typeof(int));
            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, count);
            ilg.Emit(OpCodes.Call, Methods.Unpacker_TryReadArrayLength);

            // verify read correctly
            throwBlocks.ThrowIfFalse(() =>
            {
                ilg.Emit(OpCodes.Ldstr, "Expecting tuple data to be encoded as array");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, Methods.DeserialisationException_Ctor_String_Type);
                ilg.Emit(OpCodes.Throw);
            });

            // Ensure the array has the expected number of items
            ilg.Emit(OpCodes.Ldloc, count);
            ilg.Emit(OpCodes.Ldc_I4, tupleSize);

            throwBlocks.ThrowIfNotEqual(() =>
            {
                ilg.Emit(OpCodes.Ldstr, $"Received array must have length {tupleSize} for type {tupleType.FullName}");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, Methods.DeserialisationException_Ctor_String_Type);
                ilg.Emit(OpCodes.Throw);
            });

            var success = true;

            var locals = new List<LocalBuilder>();
            var i = 1;
            foreach (var type in tupleType.GenericTypeArguments)
            {
                var local = ilg.DeclareLocal(type);
                locals.Add(local);
                if (!DeserialiserEmitter.TryEmitDeserialiseCode(ilg, throwBlocks, errors, $"Item{i}", targetType, local, unpacker, context, contextLocal, unexpectedFieldBehaviour))
                {
                    errors.Add($"Unable to create deserialiser for tuple item type {type}");
                    success = false;
                }
                i++;
            }

            ilg.Emit(OpCodes.Ldloca, value);

            foreach (var local in locals)
                ilg.Emit(OpCodes.Ldloc, local);

            ilg.Emit(OpCodes.Call, tupleType.GetConstructor(tupleType.GenericTypeArguments));

            return success;
        }
    }
}