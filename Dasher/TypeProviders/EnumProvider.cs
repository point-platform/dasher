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
using System.Reflection;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class EnumProvider : ITypeProvider
    {
        public bool CanProvide(Type type) => type.IsEnum;

        public void Serialise(ILGenerator ilg, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            // write the string form of the value
            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Constrained, value.LocalType);
            ilg.Emit(OpCodes.Callvirt, typeof(object).GetMethod(nameof(ToString), new Type[0]));
            ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.Pack), new[] { typeof(string) }));
        }

        public void Deserialise(ILGenerator ilg, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            // Read value as a string
            var s = ilg.DeclareLocal(typeof(string));

            ilg.Emit(OpCodes.Ldloc, unpacker);
            ilg.Emit(OpCodes.Ldloca, s);
            ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadString), new[] { typeof(string).MakeByRefType() }));

            var lbl1 = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, lbl1);
            {
                ilg.Emit(OpCodes.Ldstr, "Unable to read string value for enum property {0} of type {1}");
                ilg.Emit(OpCodes.Ldstr, name);
                ilg.LoadType(value.LocalType);
                ilg.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Format), new[] { typeof(string), typeof(object), typeof(object) }));
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] { typeof(string), typeof(Type) }));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lbl1);

            ilg.Emit(OpCodes.Ldloc, s);
            ilg.Emit(OpCodes.Ldc_I4_1);
            ilg.Emit(OpCodes.Ldloca, value);
            ilg.Emit(OpCodes.Call, typeof(Enum).GetMethods(BindingFlags.Static | BindingFlags.Public).Single(m => m.Name == "TryParse" && m.GetParameters().Length == 3).MakeGenericMethod(value.LocalType));

            var lbl2 = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, lbl2);
            {
                ilg.Emit(OpCodes.Ldstr, "Unable to parse value \"{0}\" as a member of enum type {1}");
                ilg.Emit(OpCodes.Ldloc, s);
                ilg.LoadType(value.LocalType);
                ilg.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Format), new[] { typeof(string), typeof(object), typeof(object) }));
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] { typeof(string), typeof(Type) }));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lbl2);
        }
    }
}