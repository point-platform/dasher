#region License
//
// Dasher
//
// Copyright 2015-2017 Drew Noakes
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
    internal sealed class DecimalProvider : ITypeProvider
    {
        public bool CanProvide(Type type) => type == typeof(decimal);

        public bool UseDefaultNullHandling(Type valueType) => false;

        public bool TryEmitSerialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            // write the string form of the value
            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, Methods.Decimal_ToString);
            ilg.Emit(OpCodes.Call, Methods.Packer_Pack_String);

            return true;
        }

        public bool TryEmitDeserialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.LoadType(targetType);
            ilg.Emit(OpCodes.Ldstr, name);
            ilg.Emit(OpCodes.Call, Methods.DecimalProvider_Parse);
            ilg.Emit(OpCodes.Stloc, value);

            return true;
        }

        public static decimal Parse(Unpacker unpacker, Type targetType, string name)
        {
            if (!unpacker.TryPeekFormat(out Format format))
                throw new DeserialisationException($"Unable to determine MsgPack format for \"{name}\".", targetType);

            switch (format)
            {
                case Format.Str8:
                case Format.Str16:
                case Format.Str32:
                case Format.FixStr:
                {
                        if (!unpacker.TryReadString(out string str))
                            throw new DeserialisationException($"Unable to read MsgPack string for decimal value \"{name}\".", targetType);

                        if (!decimal.TryParse(str, out decimal value))
                            throw new DeserialisationException($"Unable to parse string \"{str}\" as a decimal for \"{name}\".", targetType);

                        return value;
                }
                case Format.NegativeFixInt:
                case Format.Int8:
                case Format.Int16:
                case Format.Int32:
                case Format.Int64:
                {
                        if (!unpacker.TryReadInt64(out long value))
                            throw new DeserialisationException($"Unable to read MsgPack integer for decimal value \"{name}\".", targetType);
                        return value;
                }
                case Format.PositiveFixInt:
                case Format.UInt8:
                case Format.UInt16:
                case Format.UInt32:
                case Format.UInt64:
                {
                        if (!unpacker.TryReadUInt64(out ulong value))
                            throw new DeserialisationException($"Unable to read MsgPack unsigned integer for decimal value \"{name}\".", targetType);
                        return value;
                }
                default:
                    throw new DeserialisationException($"Unable to deserialise decimal value from MsgPack format {format}.", targetType);
            }
        }
    }
}