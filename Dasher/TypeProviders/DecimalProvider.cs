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
            // Read value as a string
            var s = ilg.DeclareLocal(typeof(string));

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, s);
            ilg.Emit(OpCodes.Call, Methods.Unpacker_TryReadString);

            // If the unpacker method failed, throw
            throwBlocks.ThrowIfFalse(() =>
            {
                var format = ilg.DeclareLocal(typeof(Format));
                ilg.Emit(OpCodes.Ldloc, unpacker);
                ilg.Emit(OpCodes.Ldloca, format);
                ilg.Emit(OpCodes.Call, Methods.Unpacker_TryPeekFormat);
                ilg.Emit(OpCodes.Pop);

                ilg.Emit(OpCodes.Ldstr, "Unable to deserialise decimal value from MsgPack format {0}.");
                ilg.Emit(OpCodes.Ldloc, format);
                ilg.Emit(OpCodes.Box, typeof(Format));
                ilg.Emit(OpCodes.Call, Methods.String_Format_String_Object);
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, Methods.DeserialisationException_Ctor_String_Type);
                ilg.Emit(OpCodes.Throw);
            });

            ilg.Emit(OpCodes.Ldloc, s);
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, Methods.Decimal_TryParse);

            // If parsing failed, throw
            throwBlocks.ThrowIfFalse(() =>
            {
                ilg.Emit(OpCodes.Ldstr, "Unable to deserialise string \"{0}\" as a decimal value.");
                ilg.Emit(OpCodes.Ldloc, s);
                ilg.Emit(OpCodes.Call, Methods.String_Format_String_Object);
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, Methods.DeserialisationException_Ctor_String_Type);
                ilg.Emit(OpCodes.Throw);
            });

            return true;
        }
    }
}