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
    internal sealed class TimeSpanProvider : ITypeProvider
    {
        public bool CanProvide(Type type) => type == typeof(TimeSpan);

        public bool TryEmitSerialiseCode(ILGenerator ilg, ICollection<string> errors, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            // write the ticks form of the value as int64
            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, typeof(TimeSpan).GetProperty(nameof(TimeSpan.Ticks)).GetMethod);
            ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.Pack), new[] {typeof(long)}));

            return true;
        }

        public bool TryEmitDeserialiseCode(ILGenerator ilg, ICollection<string> errors, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            // Read value as a long
            var ticks = ilg.DeclareLocal(typeof(long));

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, ticks);
            ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadInt64)));

            // If the unpacker method failed (returned false), throw
            var lbl = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, lbl);
            {
                ilg.Emit(OpCodes.Ldstr, $"Expecting Int64 value for TimeSpan property {name}");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] {typeof(string), typeof(Type)}));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lbl);

            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Ldloc, ticks);
            ilg.Emit(OpCodes.Call, typeof(TimeSpan).GetConstructor(new[] {typeof(long)}));

            return true;
        }
    }
}