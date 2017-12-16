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
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class ByteArraySegmentProvider : ITypeProvider
    {
        public bool CanProvide(Type type) => type == typeof(ArraySegment<byte>);

        public bool UseDefaultNullHandling(Type type) => false;

        public bool TryEmitSerialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldloc, value);
            ilg.Emit(OpCodes.Call, Methods.Packer_Pack_ByteArraySegment);

            return true;
        }

        public bool TryEmitDeserialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            var done = ilg.DefineLabel();

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, Methods.Unpacker_TryReadByteArraySegment);

            throwBlocks.ThrowIfFalse(() =>
            {
                // not binary and not null
                ilg.Emit(OpCodes.Ldstr, "Unexpected MsgPack format for \"{0}\". Expected {1}, got {2}.");
                ilg.Emit(OpCodes.Ldstr, name);
                ilg.Emit(OpCodes.Ldstr, value.LocalType.Name);
                ilg.PeekFormatString(unpacker);
                ilg.Emit(OpCodes.Call, Methods.String_Format_String_Object_Object_Object);
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, Methods.DeserialisationException_Ctor_String_Type);
                ilg.Emit(OpCodes.Throw);
            });

            return true;
        }
    }
}
