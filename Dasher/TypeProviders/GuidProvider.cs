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
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class GuidProvider : ITypeProvider
    {
        public bool CanProvide(Type type) => type == typeof(Guid);

        public void EmitSerialiseCode(ILGenerator ilg, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            // write the string form of the value
            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, typeof(Guid).GetMethod(nameof(Guid.ToByteArray), new Type[0]));
            ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.Pack), new[] {typeof(byte[])}));
        }

        public void EmitDeserialiseCode(ILGenerator ilg, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            // Read value as a string
            var bytes = ilg.DeclareLocal(typeof(byte[]));

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, bytes);
            ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadBinary), new[] {typeof(byte[]).MakeByRefType()}));

            // If the unpacker method failed (returned false), throw
            var lbl = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, lbl);
            {
                ilg.Emit(OpCodes.Ldstr, "Unable to deserialise GUID value");
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] { typeof(string), typeof(Type) }));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lbl);

            ilg.Emit(OpCodes.Ldloc, bytes);
            ilg.Emit(OpCodes.Newobj, typeof(Guid).GetConstructor(new[] {typeof(byte[])}));
            ilg.Emit(OpCodes.Stloc, value);
        }
    }
}