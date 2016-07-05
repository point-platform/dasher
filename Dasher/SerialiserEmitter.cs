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
using Dasher.TypeProviders;

namespace Dasher
{
    internal static class SerialiserEmitter
    {
        public static Action<UnsafePacker, DasherContext, object> Build(Type type, DasherContext context)
        {
            if (type.IsPrimitive)
                throw new SerialisationException("Cannot serialise primitive types. The root type must contain properties and values to support future versioning.", type);

            var method = new DynamicMethod(
                $"Serialiser{type.Name}",
                returnType: null,
                parameterTypes: new[] {typeof(UnsafePacker), typeof(DasherContext), typeof(object)},
                restrictedSkipVisibility: true);

            var ilg = method.GetILGenerator();

            // store packer in a local so we can pass it easily
            var packer = ilg.DeclareLocal(typeof(UnsafePacker));
            ilg.Emit(OpCodes.Ldarg_0); // packer
            ilg.Emit(OpCodes.Stloc, packer);

            // store context in a local so we can pass it easily
            var contextLocal = ilg.DeclareLocal(typeof(DasherContext));
            ilg.Emit(OpCodes.Ldarg_1); // context
            ilg.Emit(OpCodes.Stloc, contextLocal);

            // cast value to a local of required type
            var value = ilg.DeclareLocal(type);
            ilg.Emit(OpCodes.Ldarg_2); // value
            ilg.Emit(type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, type);
            ilg.Emit(OpCodes.Stloc, value);

            if (!TryEmitSerialiseCode(ilg, value, packer, context, contextLocal, isRoot: true))
                throw new Exception($"Cannot serialise type {value.LocalType}.");

            ilg.Emit(OpCodes.Ret);

            // Return a delegate that performs the above operations
            return (Action<UnsafePacker, DasherContext, object>)method.CreateDelegate(typeof(Action<UnsafePacker, DasherContext, object>));
        }

        public static bool TryEmitSerialiseCode(ILGenerator ilg, LocalBuilder value, LocalBuilder packer, DasherContext context, LocalBuilder contextLocal, bool isRoot = false)
        {
            ITypeProvider provider;
            if (!context.TryGetTypeProvider(value.LocalType, out provider))
                return false;

            if (!isRoot && provider is ComplexTypeProvider)
            {
                // prevent endless code generation for recursive types by delegating to a method call
                ilg.Emit(OpCodes.Ldloc, contextLocal);
                ilg.LoadType(value.LocalType);
                ilg.Emit(OpCodes.Call, typeof(DasherContext).GetMethod(nameof(context.GetOrCreateSerialiseAction), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {typeof(Type)}, null));

                ilg.Emit(OpCodes.Ldloc, packer);
                ilg.Emit(OpCodes.Ldloc, contextLocal);
                ilg.Emit(OpCodes.Ldloc, value);
                if (value.LocalType.IsValueType)
                    ilg.Emit(OpCodes.Box, value.LocalType);
                ilg.Emit(OpCodes.Call, typeof(Action<UnsafePacker, DasherContext, object>).GetMethod(nameof(Func<UnsafePacker, DasherContext, object>.Invoke), new[] {typeof(UnsafePacker), typeof(DasherContext), typeof(object)}));
            }
            else
            {
                var end = ilg.DefineLabel();

                if (!value.LocalType.IsValueType)
                {
                    var nonNull = ilg.DefineLabel();
                    ilg.Emit(OpCodes.Ldloc, value);
                    ilg.Emit(OpCodes.Brtrue, nonNull);
                    ilg.Emit(OpCodes.Ldloc, packer);
                    ilg.Emit(OpCodes.Call, typeof(UnsafePacker).GetMethod(nameof(UnsafePacker.PackNull)));
                    ilg.Emit(OpCodes.Br, end);
                    ilg.MarkLabel(nonNull);
                }

                provider.EmitSerialiseCode(ilg, value, packer, contextLocal, context);

                ilg.MarkLabel(end);
            }
            return true;
        }
    }
}