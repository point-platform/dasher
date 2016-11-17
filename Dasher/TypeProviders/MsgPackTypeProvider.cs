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
using System.Reflection;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class MsgPackTypeProvider : ITypeProvider
    {
        private static readonly Dictionary<Type, MethodInfo> _unpackerTryReadMethodByType = new Dictionary<Type, MethodInfo>
        {
            {typeof(sbyte),  Methods.Unpacker_TryReadSByte},
            {typeof(byte),   Methods.Unpacker_TryReadByte},
            {typeof(short),  Methods.Unpacker_TryReadInt16},
            {typeof(ushort), Methods.Unpacker_TryReadUInt16},
            {typeof(int),    Methods.Unpacker_TryReadInt32},
            {typeof(uint),   Methods.Unpacker_TryReadUInt32},
            {typeof(long),   Methods.Unpacker_TryReadInt64},
            {typeof(ulong),  Methods.Unpacker_TryReadUInt64},
            {typeof(float),  Methods.Unpacker_TryReadSingle},
            {typeof(double), Methods.Unpacker_TryReadDouble},
            {typeof(bool),   Methods.Unpacker_TryReadBoolean},
            {typeof(string), Methods.Unpacker_TryReadString},
            {typeof(byte[]), Methods.Unpacker_TryReadBinary}
        };

        public bool CanProvide(Type type) => _unpackerTryReadMethodByType.ContainsKey(type);

        public bool UseDefaultNullHandling(Type valueType) => !valueType.GetTypeInfo().IsValueType;

        public bool TryEmitSerialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            var packerMethod = typeof(Packer).GetMethod(nameof(Packer.Pack), new[] {value.LocalType});

            if (packerMethod == null)
                throw new InvalidOperationException($"Type not supported. Call {nameof(CanProvide)} first.");

            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldloc, value);
            ilg.Emit(OpCodes.Call, packerMethod);

            return true;
        }

        public bool TryEmitDeserialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            MethodInfo unpackerMethod;
            if (!_unpackerTryReadMethodByType.TryGetValue(value.LocalType, out unpackerMethod))
            {
                errors.Add($"Type {targetType} does not map to a native MsgPack type");
                return false;
            }

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, unpackerMethod);

            // If the unpacker method failed (returned false), throw
            throwBlocks.ThrowIfFalse(() =>
            {
                var format = ilg.DeclareLocal(typeof(Format));
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ldloca, format);
                ilg.Emit(OpCodes.Call, Methods.Unpacker_TryPeekFormat);
                ilg.Emit(OpCodes.Pop);

                ilg.Emit(OpCodes.Ldstr, "Unexpected MsgPack format for \"{0}\". Expected {1}, got {2}.");
                ilg.Emit(OpCodes.Ldstr, name);
                ilg.Emit(OpCodes.Ldstr, value.LocalType.Name);
                ilg.Emit(OpCodes.Ldloc, format);
                ilg.Emit(OpCodes.Box, typeof(Format));
                ilg.Emit(OpCodes.Call, Methods.String_Format_String_Object_Object_Object);
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, Methods.DeserialisationException_Ctor_String_Type);
                ilg.Emit(OpCodes.Throw);
            });

            return true;
        }
    }
}