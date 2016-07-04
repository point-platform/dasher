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
using System.Linq;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class NullableValueProvider : ITypeProvider
    {
        bool ITypeProvider.CanProvide(Type type) => IsNullableValueType(type);

        public static bool IsNullableValueType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

        public void EmitSerialiseCode(ILGenerator ilg, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            var type = value.LocalType;
            var valueType = type.GetGenericArguments().Single();

            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, type.GetProperty(nameof(Nullable<int>.HasValue)).GetMethod);

            var lblNull = ilg.DefineLabel();
            var lblExit = ilg.DefineLabel();

            ilg.Emit(OpCodes.Brfalse, lblNull);

            // has a value to serialise
            var nonNullValue = ilg.DeclareLocal(valueType);
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, type.GetProperty(nameof(Nullable<int>.Value)).GetMethod);
            ilg.Emit(OpCodes.Stloc, nonNullValue);

            if (!context.TrySerialise(ilg, nonNullValue, packer, contextLocal))
                throw new Exception($"Cannot serialise underlying type of Nullable<{valueType}>");

            ilg.Emit(OpCodes.Br, lblExit);

            ilg.MarkLabel(lblNull);

            // value is null
            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.PackNull)));

            ilg.MarkLabel(lblExit);
        }

        public void EmitDeserialiseCode(ILGenerator ilg, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            var nullableType = value.LocalType;
            var valueType = nullableType.GetGenericArguments().Single();

            var lblNull = ilg.DefineLabel();
            var lblExit = ilg.DefineLabel();

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadNull)));

            ilg.Emit(OpCodes.Brtrue, lblNull);

            // non-null
            var nonNullValue = ilg.DeclareLocal(valueType);

            if (!context.TryDeserialise(ilg, name, targetType, nonNullValue, unpacker, contextLocal, unexpectedFieldBehaviour))
                throw new Exception($"Unable to deserialise values of type Nullable<{valueType}> from MsgPack data.");

            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Ldloc, nonNullValue);
            ilg.Emit(OpCodes.Call, nullableType.GetConstructor(new[] {valueType}));

            ilg.Emit(OpCodes.Br, lblExit);
            ilg.MarkLabel(lblNull);

            // null
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Initobj, nullableType);

            ilg.MarkLabel(lblExit);
        }
    }
}