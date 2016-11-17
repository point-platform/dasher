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
    internal sealed class DateTimeOffsetProvider : ITypeProvider
    {
        private const int TicksPerMinute = 600000000;

        public bool CanProvide(Type type) => type == typeof(DateTimeOffset);

        public bool UseDefaultNullHandling(Type valueType) => false;

        public bool TryEmitSerialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            // We need to write both the date and the offset
            // - dto.DateTime always has unspecified kind (so we can just use Ticks rather than ToBinary and ignore internal flags)
            // - dto.Offset is a timespan but always has integral minutes (minutes will be a smaller number than ticks so uses fewer bytes on the wire)

            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Dup);
            ilg.Emit(OpCodes.Dup);

            // Write the array header
            ilg.Emit(OpCodes.Ldc_I4_2);
            ilg.Emit(OpCodes.Call, Methods.Packer_PackArrayHeader);

            // Write ticks
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, Methods.DateTimeOffset_Ticks_Get);
            ilg.Emit(OpCodes.Call, Methods.Packer_Pack_Int64);

            // Write offset minutes
            var offset = ilg.DeclareLocal(typeof(TimeSpan));
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, Methods.DateTimeOffset_Offset_Get);
            ilg.Emit(OpCodes.Stloc, offset);
            ilg.Emit(OpCodes.Ldloca, offset);
            ilg.Emit(OpCodes.Call, Methods.TimeSpan_Ticks_Get);
            ilg.Emit(OpCodes.Ldc_I4, TicksPerMinute);
            ilg.Emit(OpCodes.Conv_I8);
            ilg.Emit(OpCodes.Div);
            ilg.Emit(OpCodes.Conv_I2);
            ilg.Emit(OpCodes.Call, Methods.Packer_Pack_Int16);

            return true;
        }

        public bool TryEmitDeserialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            // Ensure we have an array of two values
            var arrayLength = ilg.DeclareLocal(typeof(int));

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, arrayLength);
            ilg.Emit(OpCodes.Call, Methods.Unpacker_TryReadArrayLength);

            // If the unpacker method failed (returned false), throw
            throwBlocks.ThrowIfFalse(() =>
            {
                ilg.Emit(OpCodes.Ldstr, $"Expecting array header for DateTimeOffset property {name}");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, Methods.DeserialisationException_Ctor_String_Type);
                ilg.Emit(OpCodes.Throw);
            });

            ilg.Emit(OpCodes.Ldloc, arrayLength);
            ilg.Emit(OpCodes.Ldc_I4_2);

            throwBlocks.ThrowIfNotEqual(() =>
            {
                ilg.Emit(OpCodes.Ldstr, $"Expecting array to contain two items for DateTimeOffset property {name}");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, Methods.DeserialisationException_Ctor_String_Type);
                ilg.Emit(OpCodes.Throw);
            });

            // Read ticks
            var ticks = ilg.DeclareLocal(typeof(long));

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, ticks);
            ilg.Emit(OpCodes.Call, Methods.Unpacker_TryReadInt64);

            // If the unpacker method failed (returned false), throw
            throwBlocks.ThrowIfFalse(() =>
            {
                ilg.Emit(OpCodes.Ldstr, $"Expecting Int64 value for ticks component of DateTimeOffset property {name}");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, Methods.DeserialisationException_Ctor_String_Type);
                ilg.Emit(OpCodes.Throw);
            });

            // Read offset
            var minutes = ilg.DeclareLocal(typeof(short));

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, minutes);
            ilg.Emit(OpCodes.Call, Methods.Unpacker_TryReadInt16);

            // If the unpacker method failed (returned false), throw
            throwBlocks.ThrowIfFalse(() =>
            {
                ilg.Emit(OpCodes.Ldstr, $"Expecting Int16 value for offset component of DateTimeOffset property {name}");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, Methods.DeserialisationException_Ctor_String_Type);
                ilg.Emit(OpCodes.Throw);
            });

            // Compose the final DateTimeOffset
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Ldloc, ticks);
            ilg.Emit(OpCodes.Ldloc, minutes);
            ilg.Emit(OpCodes.Conv_I8);
            ilg.Emit(OpCodes.Ldc_I4, TicksPerMinute);
            ilg.Emit(OpCodes.Conv_I8);
            ilg.Emit(OpCodes.Mul);
            ilg.Emit(OpCodes.Conv_I8);
            ilg.Emit(OpCodes.Call, Methods.TimeSpan_FromTicks);
            ilg.Emit(OpCodes.Call, Methods.DateTimeOffset_Ctor_Long_TimeSpan);

            return true;
        }
    }
}