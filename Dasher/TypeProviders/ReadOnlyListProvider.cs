#region License
//
// Dasher
//
// Copyright 2015-2016 Drew Noakes
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
// More information about this project is available at:
//
//    https://github.com/drewnoakes/dasher
//
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class ReadOnlyListProvider : ITypeProvider
    {
        public bool CanProvide(Type type) => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>);

        public bool TryEmitSerialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            var type = value.LocalType;
            var elementType = type.GetGenericArguments().Single();

            // read list length
            var count = ilg.DeclareLocal(typeof(int));
            ilg.Emit(OpCodes.Ldloc, value);
            ilg.Emit(OpCodes.Callvirt, typeof(IReadOnlyCollection<>).MakeGenericType(elementType).GetProperty(nameof(IReadOnlyList<int>.Count)).GetMethod);
            ilg.Emit(OpCodes.Stloc, count);

            // write array header
            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldloc, count);
            ilg.Emit(OpCodes.Call, Methods.Packer_PackArrayHeader);

            // begin loop
            var loopStart = ilg.DefineLabel();
            var loopTest = ilg.DefineLabel();

            var i = ilg.DeclareLocal(typeof(int));
            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Stloc, i);

            ilg.Emit(OpCodes.Br, loopTest);
            ilg.MarkLabel(loopStart);

            // loop body
            ilg.Emit(OpCodes.Ldloc, value);
            ilg.Emit(OpCodes.Ldloc, i);
            ilg.Emit(OpCodes.Callvirt, type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Single(p => p.Name == "Item" && p.GetIndexParameters().Length == 1).GetMethod);
            var elementValue = ilg.DeclareLocal(elementType);
            ilg.Emit(OpCodes.Stloc, elementValue);

            if (!SerialiserEmitter.TryEmitSerialiseCode(ilg, throwBlocks, errors, elementValue, packer, context, contextLocal))
            {
                errors.Add($"Cannot serialise IReadOnlyList<> element type {elementType}.");
                return false;
            }

            // loop counter increment
            ilg.Emit(OpCodes.Ldloc, i);
            ilg.Emit(OpCodes.Ldc_I4_1);
            ilg.Emit(OpCodes.Add);
            ilg.Emit(OpCodes.Stloc, i);

            // loop test
            ilg.MarkLabel(loopTest);
            ilg.Emit(OpCodes.Ldloc, i);
            ilg.Emit(OpCodes.Ldloc, count);
            ilg.Emit(OpCodes.Clt);
            ilg.Emit(OpCodes.Brtrue, loopStart);

            return true;
        }

        public bool TryEmitDeserialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            var elementType = value.LocalType.GetGenericArguments().Single();

            // read list length
            var count = ilg.DeclareLocal(typeof(int));
            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, count);
            ilg.Emit(OpCodes.Call, Methods.Unpacker_TryReadArrayLength);

            // verify read correctly
            throwBlocks.ThrowIfFalse(() =>
            {
                ilg.Emit(OpCodes.Ldstr, "Expecting collection data to be encoded as an array");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, Methods.DeserialisationException_Ctor_String_Type);
                ilg.Emit(OpCodes.Throw);
            });

            // create an array to store values
            ilg.Emit(OpCodes.Ldloc, count);
            ilg.Emit(OpCodes.Newarr, elementType);

            var array = ilg.DeclareLocal(elementType.MakeArrayType());
            ilg.Emit(OpCodes.Stloc, array);

            // begin loop
            var loopStart = ilg.DefineLabel();
            var loopTest = ilg.DefineLabel();
            var loopEnd = ilg.DefineLabel();

            var i = ilg.DeclareLocal(typeof(int));
            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Stloc, i);

            ilg.Emit(OpCodes.Br, loopTest);
            ilg.MarkLabel(loopStart);

            // loop body
            var element = ilg.DeclareLocal(elementType);

            if (!DeserialiserEmitter.TryEmitDeserialiseCode(ilg, throwBlocks, errors, name, targetType, element, unpacker, context, contextLocal, unexpectedFieldBehaviour))
            {
                errors.Add($"Unable to deserialise IReadOnlyList<> element type {elementType} from MsgPack data.");
                return false;
            }

            ilg.Emit(OpCodes.Ldloc, array);
            ilg.Emit(OpCodes.Ldloc, i);
            ilg.Emit(OpCodes.Ldloc, element);
            ilg.Emit(OpCodes.Stelem, elementType);

            // loop counter increment
            ilg.Emit(OpCodes.Ldloc, i);
            ilg.Emit(OpCodes.Ldc_I4_1);
            ilg.Emit(OpCodes.Add);
            ilg.Emit(OpCodes.Stloc, i);

            // loop test
            ilg.MarkLabel(loopTest);
            ilg.Emit(OpCodes.Ldloc, i);
            ilg.Emit(OpCodes.Ldloc, count);
            ilg.Emit(OpCodes.Clt);
            ilg.Emit(OpCodes.Brtrue, loopStart);

            // after loop
            ilg.MarkLabel(loopEnd);

            ilg.Emit(OpCodes.Ldloc, array);
            ilg.Emit(OpCodes.Stloc, value);

            return true;
        }
    }
}