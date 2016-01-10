#region License
//
// Dasher
//
// Copyright 2015 Drew Noakes
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
        public bool CanProvide(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>);

        public void Serialise(ILGenerator ilg, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            var type = value.LocalType;
            var elementType = type.GetGenericArguments().Single();

            var endLabel = ilg.DefineLabel();

            // check for null
            var nonNullLabel = ilg.DefineLabel();
            ilg.Emit(OpCodes.Ldloc, value);
            ilg.Emit(OpCodes.Brtrue, nonNullLabel);
            {
                // write null
                ilg.Emit(OpCodes.Ldloc, packer);
                ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.PackNull)));
                ilg.Emit(OpCodes.Br, endLabel);
            }
            ilg.MarkLabel(nonNullLabel);

            // read list length
            var count = ilg.DeclareLocal(typeof(int));
            ilg.Emit(OpCodes.Ldloc, value);
            ilg.Emit(OpCodes.Callvirt, typeof(IReadOnlyCollection<>).MakeGenericType(elementType).GetProperty(nameof(IReadOnlyList<int>.Count)).GetMethod);
            ilg.Emit(OpCodes.Stloc, count);

            // write array header
            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldloc, count);
            ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.PackArrayHeader)));

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

            if (!context.TrySerialise(ilg, elementValue, packer, contextLocal))
                throw new Exception($"Cannot serialise IReadOnlyList<> element type {value.LocalType}.");

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

            // end
            ilg.MarkLabel(endLabel);
        }

        public void Deserialise(ILGenerator ilg, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            var elementType = value.LocalType.GetGenericArguments().Single();

            var endLabel = ilg.DefineLabel();

            // check for null
            var nonNullLabel = ilg.DefineLabel();
            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadNull)));
            ilg.Emit(OpCodes.Brfalse, nonNullLabel);
            {
                ilg.Emit(OpCodes.Ldnull);
                ilg.Emit(OpCodes.Stloc, value);
                ilg.Emit(OpCodes.Br, endLabel);
            }
            ilg.MarkLabel(nonNullLabel);

            // read list length
            var count = ilg.DeclareLocal(typeof(int));
            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, count);
            ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadArrayLength)));

            // verify read correctly
            var lbl1 = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, lbl1);
            {
                ilg.Emit(OpCodes.Ldstr, "Expecting collection data to be encoded as array");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] { typeof(string), typeof(Type) }));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lbl1);

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

            if (!context.TryDeserialise(ilg, name, targetType, element, unpacker, contextLocal, unexpectedFieldBehaviour))
                throw new Exception($"Unable to deserialise values of type {elementType} from MsgPack data.");

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

            // end
            ilg.MarkLabel(endLabel);
        }
    }
}