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
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using Dasher.TypeProviders;

namespace Dasher
{
    internal static class DeserialiserEmitter
    {
        public static Func<Unpacker, DasherContext, object> Build(Type type, UnexpectedFieldBehaviour unexpectedFieldBehaviour, DasherContext context)
        {
            // Create a collection for errors, initially with zero capacity (minimal allocation)
            var errors = new List<string>(0);

            // Validate that the type is suitable as a top-level type
            context.ValidateTopLevelType(type, errors);

            // Throw if there were any errors
            if (errors.Any())
                throw new DeserialisationException(errors, type);

            // Initialise code gen
            var method = new DynamicMethod(
                $"Generated{type.Name}Deserialiser",
                returnType: typeof(object),
                parameterTypes: new[] {typeof(Unpacker), typeof(DasherContext)},
                restrictedSkipVisibility: true);

            var ilg = method.GetILGenerator();

            // Convert args to locals, so we can pass them around
            var unpacker = ilg.DeclareLocal(typeof(Unpacker));
            ilg.Emit(OpCodes.Ldarg_0);
            ilg.Emit(OpCodes.Stloc, unpacker);

            var contextLocal = ilg.DeclareLocal(typeof(DasherContext));
            ilg.Emit(OpCodes.Ldarg_1);
            ilg.Emit(OpCodes.Stloc, contextLocal);

            var valueLocal = ilg.DeclareLocal(type);

            var throwBlocks = new ThrowBlockGatherer(ilg);

            if (!TryEmitDeserialiseCode(ilg, throwBlocks, errors, "<root>", type, valueLocal, unpacker, context, contextLocal, unexpectedFieldBehaviour, isRoot: true))
            {
                Debug.Assert(errors.Any());
                throw new DeserialisationException(errors, type);
            }

            ilg.Emit(OpCodes.Ldloc, valueLocal);

            if (type.IsValueType)
                ilg.Emit(OpCodes.Box, type);

            // Return the newly constructed object!
            ilg.Emit(OpCodes.Ret);

            // Write all the exception handling blocks out of line
            throwBlocks.Flush();

            // Return a delegate that performs the above operations
            return (Func<Unpacker, DasherContext, object>)method.CreateDelegate(typeof(Func<Unpacker, DasherContext, object>));
        }

        public static bool TryEmitDeserialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, DasherContext context, LocalBuilder contextLocal, UnexpectedFieldBehaviour unexpectedFieldBehaviour, bool isRoot = false)
        {
            ITypeProvider provider;
            if (!context.TryGetTypeProvider(value.LocalType, errors, out provider))
                return false;

            if (!isRoot && provider is ComplexTypeProvider)
            {
                ilg.Emit(OpCodes.Ldloc, contextLocal);
                ilg.LoadType(value.LocalType);
                ilg.Emit(OpCodes.Ldc_I4, (int)unexpectedFieldBehaviour);
                ilg.Emit(OpCodes.Call, Methods.DasherContext_GetOrCreateDeserialiseFunc);
                ilg.Emit(OpCodes.Ldloc, unpacker);
                ilg.Emit(OpCodes.Ldloc, contextLocal);
                ilg.Emit(OpCodes.Call, Methods.DasherDeserialiseFunc_Invoke);
                ilg.Emit(value.LocalType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, value.LocalType);
                ilg.Emit(OpCodes.Stloc, value);
            }
            else
            {
                var end = ilg.DefineLabel();

                if (!value.LocalType.IsValueType)
                {
                    // check for null
                    var nonNullLabel = ilg.DefineLabel();
                    ilg.Emit(OpCodes.Ldloc, unpacker);
                    ilg.Emit(OpCodes.Call, Methods.Unpacker_TryReadNull);
                    ilg.Emit(OpCodes.Brfalse_S, nonNullLabel);
                    {
                        ilg.Emit(OpCodes.Ldnull);
                        ilg.Emit(OpCodes.Stloc, value);
                        ilg.Emit(OpCodes.Br, end);
                    }
                    ilg.MarkLabel(nonNullLabel);
                }

                if (!provider.TryEmitDeserialiseCode(ilg, throwBlocks, errors, name, targetType, value, unpacker, contextLocal, context, unexpectedFieldBehaviour))
                    return false;

                ilg.MarkLabel(end);
            }
            return true;
        }
    }
}