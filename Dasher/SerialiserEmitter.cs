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
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using Dasher.TypeProviders;
using System.Reflection;

namespace Dasher
{
    internal static class SerialiserEmitter
    {
        public static Action<Packer, DasherContext, object> Build(Type type, DasherContext context)
        {
            // Create a collection for errors, initially with zero capacity (minimal allocation)
            var errors = new List<string>(0);

            // Validate that the type is suitable as a top-level type
            context.ValidateTopLevelType(type, errors);

            // Throw if there were any errors
            if (errors.Any())
                throw new SerialiserGenerationException(errors, type);

            // Initialise code gen
            var method = new DynamicMethod(
                $"Generated{type.Name}Serialiser",
                returnType: null,
                parameterTypes: new[] {typeof(Packer), typeof(DasherContext), typeof(object)},
                restrictedSkipVisibility: true);

            var ilg = method.GetILGenerator();

            // Convert args to locals, so we can pass them around
            var packer = ilg.DeclareLocal(typeof(Packer));
            ilg.Emit(OpCodes.Ldarg_0); // packer
            ilg.Emit(OpCodes.Stloc, packer);

            var contextLocal = ilg.DeclareLocal(typeof(DasherContext));
            ilg.Emit(OpCodes.Ldarg_1); // context
            ilg.Emit(OpCodes.Stloc, contextLocal);

            var value = ilg.DeclareLocal(type);
            ilg.Emit(OpCodes.Ldarg_2); // value
            ilg.Emit(type.GetTypeInfo().IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, type);
            ilg.Emit(OpCodes.Stloc, value);

            var throwBlocks = new ThrowBlockGatherer(ilg);

            if (!TryEmitSerialiseCode(ilg, throwBlocks, errors, value, packer, context, contextLocal, isRoot: true))
            {
                Debug.Assert(errors.Any());
                throw new SerialiserGenerationException(errors, type);
            }

            // Return the newly constructed object!
            ilg.Emit(OpCodes.Ret);

            // Write all the exception handling blocks out of line
            throwBlocks.Flush();

            // Return a delegate that performs the above operations
            return (Action<Packer, DasherContext, object>)method.CreateDelegate(typeof(Action<Packer, DasherContext, object>));
        }

        public static bool TryEmitSerialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, LocalBuilder value, LocalBuilder packer, DasherContext context, LocalBuilder contextLocal, bool isRoot = false)
        {
            ITypeProvider provider;
            if (!context.TryGetTypeProvider(value.LocalType, errors, out provider))
                return false;

            if (!isRoot && provider is ComplexTypeProvider)
            {
                // prevent endless code generation for recursive types by delegating to a method call
                ilg.Emit(OpCodes.Ldloc, contextLocal);
                ilg.LoadType(value.LocalType);
                ilg.Emit(OpCodes.Call, Methods.DasherContext_GetOrCreateSerialiseAction);

                ilg.Emit(OpCodes.Ldloc, packer);
                ilg.Emit(OpCodes.Ldloc, contextLocal);
                ilg.Emit(OpCodes.Ldloc, value);
                if (value.LocalType.GetTypeInfo().IsValueType)
                    ilg.Emit(OpCodes.Box, value.LocalType);
                ilg.Emit(OpCodes.Call, Methods.DasherSerialiseAction_Invoke);
            }
            else
            {
                var end = ilg.DefineLabel();

                if (provider.UseDefaultNullHandling(value.LocalType))
                {
                    var nonNull = ilg.DefineLabel();
                    ilg.Emit(OpCodes.Ldloc, value);
                    ilg.Emit(OpCodes.Brtrue_S, nonNull);
                    ilg.Emit(OpCodes.Ldloc, packer);
                    ilg.Emit(OpCodes.Call, Methods.Packer_PackNull);
                    ilg.Emit(OpCodes.Br, end);
                    ilg.MarkLabel(nonNull);
                }

                if (!provider.TryEmitSerialiseCode(ilg, throwBlocks, errors, value, packer, contextLocal, context))
                    return false;

                ilg.MarkLabel(end);
            }
            return true;
        }
    }
}