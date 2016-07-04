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
using System.Reflection;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class DateTimeProvider : ITypeProvider
    {
        public bool CanProvide(Type type) => type == typeof(DateTime);

        public void EmitSerialiseCode(ILGenerator ilg, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            // Write the binary form of the value as long
            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, typeof(DateTime).GetMethod(nameof(DateTime.ToBinary)));
            ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.Pack), new[] {typeof(long)}));
        }

        public void EmitDeserialiseCode(ILGenerator ilg, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            // Read value as a long
            var binary = ilg.DeclareLocal(typeof(long));

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, binary);
            ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadInt64)));

            // If the unpacker method failed (returned false), throw
            var lbl = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, lbl);
            {
                ilg.Emit(OpCodes.Ldstr, $"Expecting Int64 value for DateTime property {name}");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] {typeof(string), typeof(Type)}));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lbl);

            ilg.Emit(OpCodes.Ldloc, binary);
            ilg.Emit(OpCodes.Call, typeof(DateTime).GetMethod(nameof(DateTime.FromBinary), BindingFlags.Static | BindingFlags.Public));
            ilg.Emit(OpCodes.Stloc, value);
        }
    }
}