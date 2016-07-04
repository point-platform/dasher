using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class TupleProvider : ITypeProvider
    {
        public bool CanProvide(Type type) => type.IsConstructedGenericType && type.FullName.StartsWith("System.Tuple`");

        public void EmitSerialiseCode(ILGenerator ilg, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            var tupleType = value.LocalType;
            var tupleSize = tupleType.GenericTypeArguments.Length;

            Debug.Assert(tupleSize != 0);

            // write the array header
            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldc_I4, tupleSize);
            ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.PackArrayHeader), new[] {typeof(uint)}));

            // write each item's value
            var i = 1;
            foreach (var type in tupleType.GenericTypeArguments)
            {
                ilg.Emit(OpCodes.Ldloc, value);
                ilg.Emit(OpCodes.Call, tupleType.GetProperty($"Item{i}").GetMethod);
                var local = ilg.DeclareLocal(type);
                ilg.Emit(OpCodes.Stloc, local);
                if (!context.TryEmitSerialiseCode(ilg, local, packer, contextLocal))
                    throw new Exception($"Unable to serialise tuple item of type {type}");
                i++;
            }
        }

        public void EmitDeserialiseCode(ILGenerator ilg, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            var tupleType = value.LocalType;
            var tupleSize = tupleType.GenericTypeArguments.Length;

            Debug.Assert(tupleSize != 0);

            // read array length
            var count = ilg.DeclareLocal(typeof(int));
            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, count);
            ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadArrayLength)));

            // verify read correctly
            var lblReadArrayOk = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, lblReadArrayOk);
            {
                ilg.Emit(OpCodes.Ldstr, "Expecting tuple data to be encoded as array");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] { typeof(string), typeof(Type) }));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lblReadArrayOk);

            // Ensure the array has the expected number of items
            ilg.Emit(OpCodes.Ldloc, count);
            ilg.Emit(OpCodes.Ldc_I4, tupleSize);
            var lblSizeOk = ilg.DefineLabel();
            ilg.Emit(OpCodes.Beq, lblSizeOk);
            {
                ilg.Emit(OpCodes.Ldstr, $"Received array must have length {tupleSize} for type {tupleType.FullName}");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] {typeof(string), typeof(Type)}));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lblSizeOk);

            var locals = new List<LocalBuilder>();
            var i = 1;
            foreach (var type in tupleType.GenericTypeArguments)
            {
                var local = ilg.DeclareLocal(type);
                locals.Add(local);
                if (!context.TryEmitDeserialiseCode(ilg, $"Item{i}", targetType, local, unpacker, contextLocal, unexpectedFieldBehaviour))
                    throw new DeserialisationException($"Unable to create deserialiser for tuple item of type {type}", targetType);
                i++;
            }

            foreach (var local in locals)
                ilg.Emit(OpCodes.Ldloc, local);

            ilg.Emit(OpCodes.Newobj, tupleType.GetConstructor(tupleType.GenericTypeArguments));

            ilg.Emit(OpCodes.Stloc, value);
        }
    }
}