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
using System.Reflection;
using System.Reflection.Emit;

namespace Dasher.TypeProviders
{
    internal sealed class ComplexTypeProvider : ITypeProvider
    {
        public static bool TryValidateComplexType(Type type, ICollection<string> errors)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);

            switch (constructors.Length)
            {
                case 0:
                    errors.Add("Complex type provider requires a public constructor.");
                    return false;
                case 1:
                    break;
                default:
                    errors.Add($"Complex type provider requires a 1 public constructor, not {constructors.Length}.");
                    return false;
            }

            if (constructors[0].GetParameters().Length == 0)
            {
                errors.Add("Complex type provider requires constructor to have at least one argument.");
                return false;
            }

            return true;
        }

        public bool CanProvide(Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            return constructors.Length == 1 && constructors[0].GetParameters().Length != 0;
        }

        public bool UseDefaultNullHandling(Type valueType) => !valueType.GetTypeInfo().IsValueType;

        public bool TryEmitSerialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, LocalBuilder value, LocalBuilder packer, LocalBuilder contextLocal, DasherContext context)
        {
            // treat as complex object and recur
            var props = value.LocalType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .ToList();

            if (props.Count == 0)
                throw new SerialisationException($"Unable to serialise type \"{value.LocalType}\". It has no dedicated type provider, and has no properties for serialisation as a complex type.", value.LocalType);

            // write map header
            ilg.Emit(OpCodes.Ldloc, packer);
            ilg.Emit(OpCodes.Ldc_I4, props.Count);
            ilg.Emit(OpCodes.Call, Methods.Packer_PackMapHeader);

            var success = true;

            // write each property's value
            foreach (var prop in props)
            {
                var propValue = ilg.DeclareLocal(prop.PropertyType);

                // write property name
                ilg.Emit(OpCodes.Ldloc, packer);
                ilg.Emit(OpCodes.Ldstr, prop.Name);
                ilg.Emit(OpCodes.Call, Methods.Packer_Pack_String);

                // get property value
                ilg.Emit(value.LocalType.GetTypeInfo().IsValueType ? OpCodes.Ldloca : OpCodes.Ldloc, value);
                ilg.Emit(OpCodes.Call, prop.GetMethod);
                ilg.Emit(OpCodes.Stloc, propValue);

                success &= SerialiserEmitter.TryEmitSerialiseCode(ilg, throwBlocks, errors, propValue, packer, context, contextLocal);
            }

            return success;
        }

        public bool TryEmitDeserialiseCode(ILGenerator ilg, ThrowBlockGatherer throwBlocks, ICollection<string> errors, string name, Type targetType, LocalBuilder value, LocalBuilder unpacker, LocalBuilder contextLocal, DasherContext context, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            if (!TryValidateComplexType(targetType, errors))
                return false;

            var constructors = targetType.GetConstructors(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            Debug.Assert(constructors.Length == 1, "Should have one constructor (validation should be performed before call)");
            var constructor = constructors[0];
            var parameters = constructor.GetParameters();

            Action throwException = () =>
            {
                ilg.LoadType(targetType);
                ilg.Emit(OpCodes.Newobj, Methods.DeserialisationException_Ctor_String_Type);
                ilg.Emit(OpCodes.Throw);
            };

            Action loadPeekedFormatName = () =>
            {
                var format = ilg.DeclareLocal(typeof(Format));
                ilg.Emit(OpCodes.Ldloc, unpacker);
                ilg.Emit(OpCodes.Ldloca, format);
                ilg.Emit(OpCodes.Call, Methods.Unpacker_TryPeekFormat);
                // Drop the return value: if false, 'format' will be 'Unknown' which is fine.
                ilg.Emit(OpCodes.Pop);
                ilg.Emit(OpCodes.Ldloc, format);
                ilg.Emit(OpCodes.Box, typeof(Format));
                ilg.Emit(OpCodes.Call, Methods.Format_ToString);
            };

            #region Initialise locals for constructor args

            var valueLocals = new LocalBuilder[parameters.Length];
            var valueSetLocals = new LocalBuilder[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                valueLocals[i] = ilg.DeclareLocal(parameter.ParameterType);
                valueSetLocals[i] = ilg.DeclareLocal(typeof(int));

                if (parameter.HasDefaultValue)
                {
                    // set default values on params
                    if (NullableValueProvider.IsNullableValueType(parameter.ParameterType))
                    {
                        ilg.Emit(OpCodes.Ldloca, valueLocals[i]);
                        if (parameter.DefaultValue == null)
                        {
                            ilg.Emit(OpCodes.Initobj, parameter.ParameterType);
                        }
                        else
                        {
                            ilg.LoadConstant(parameter.DefaultValue);
                            ilg.Emit(OpCodes.Call, parameter.ParameterType.GetConstructor(new[] { parameter.ParameterType.GetGenericArguments().Single() }));
                        }
                    }
                    else
                    {
                        // TODO if DefaultValue == default(T), do nothing?
                        ilg.LoadConstant(parameter.DefaultValue);
                        ilg.Emit(OpCodes.Stloc, valueLocals[i]);
                    }
                    // set 'valueSet' to true
                    // note we use the second LSb to indicate a default value
                    ilg.Emit(OpCodes.Ldc_I4_2);
                    ilg.Emit(OpCodes.Stloc, valueSetLocals[i]);
                }
                else
                {
                    // set 'valueSet' to false
                    ilg.Emit(OpCodes.Ldc_I4_0);
                    ilg.Emit(OpCodes.Stloc, valueSetLocals[i]);
                }
            }

            #endregion

            #region Read map length

            var mapSize = ilg.DeclareLocal(typeof(int));
            {
                // MsgPack messages may be single values, arrays, maps, or any arbitrary
                // combination of these types. Our convention is to require messages to
                // be encoded as maps where the key is the property name.
                //
                // MsgPack maps begin with a header indicating the number of pairs
                // within the map. We read this here.
                ilg.Emit(OpCodes.Ldloc, unpacker);
                ilg.Emit(OpCodes.Ldloca, mapSize);
                ilg.Emit(OpCodes.Call, Methods.Unpacker_TryReadMapLength);

                // If false was returned, then the next MsgPack value is not a map
                throwBlocks.ThrowIfFalse(() =>
                {
                    // Check if it's a null
                    ilg.Emit(OpCodes.Ldloc, unpacker);
                    ilg.Emit(OpCodes.Call, Methods.Unpacker_TryReadNull);
                    var lblNotNull = ilg.DefineLabel();
                    ilg.Emit(OpCodes.Brfalse_S, lblNotNull);
                    {
                        // value is null
                        ilg.Emit(OpCodes.Ldnull);
                        ilg.Emit(OpCodes.Ret);
                    }
                    ilg.MarkLabel(lblNotNull);
                    ilg.Emit(OpCodes.Ldloc, unpacker);
                    ilg.Emit(OpCodes.Call, Methods.Unpacker_HasStreamEnded_Get);
                    var lblNotEmpty = ilg.DefineLabel();
                    ilg.Emit(OpCodes.Brfalse_S, lblNotEmpty);
                    ilg.Emit(OpCodes.Ldstr, "Data stream empty");
                    throwException();
                    ilg.MarkLabel(lblNotEmpty);
                    ilg.Emit(OpCodes.Ldstr, "Message must be encoded as a MsgPack map, not \"{0}\".");
                    loadPeekedFormatName();
                    ilg.Emit(OpCodes.Call, Methods.String_Format_String_Object);
                    throwException();
                });
            }

            #endregion

            #region Loop through each key/value pair in the map

            {
                #region Initialise loop

                // Create a loop counter, initialised to zero
                var loopIndex = ilg.DeclareLocal(typeof(long));
                ilg.Emit(OpCodes.Ldc_I4_0);
                ilg.Emit(OpCodes.Conv_I8);
                ilg.Emit(OpCodes.Stloc, loopIndex);

                // Create labels to jump to within the loop
                var lblLoopTest = ilg.DefineLabel(); // Comparing counter to map size
                var lblLoopExit = ilg.DefineLabel(); // The first instruction after the loop
                var lblLoopStart = ilg.DefineLabel(); // The first instruction within the loop

                // Run the test first
                ilg.Emit(OpCodes.Br, lblLoopTest);

                // Mark the first instruction within the loop
                ilg.MarkLabel(lblLoopStart);

                #endregion

                #region Read field name

                // Although MsgPack allows map keys to be of any arbitrary type, our convention
                // is to require keys to be strings. We read the key here.
                var key = ilg.DeclareLocal(typeof(string));
                {
                    ilg.Emit(OpCodes.Ldloc, unpacker);
                    ilg.Emit(OpCodes.Ldloca, key);
                    ilg.Emit(OpCodes.Call, Methods.Unpacker_TryReadString);

                    // If false was returned, the data stream ended
                    throwBlocks.ThrowIfFalse(() =>
                    {
                        ilg.Emit(OpCodes.Ldstr, "Data stream ended.");
                        throwException();
                    });
                }

                #endregion

                #region Match name to expected field, then attempt to deserialise by expected type

                // Build a chain of if/elseif/elseif... blocks for each of the expected fields.
                // It could be slightly more efficient here to generate a O(log(N)) tree-based lookup,
                // but that would take quite some engineering. Let's see if there's a significant perf
                // hit here or not first.

                var lblEndIfChain = ilg.DefineLabel();

                // TODO how much perf diff if properties sorted lexicographically, so no searching?
                Label? nextLabel = null;
                for (var parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
                {
                    // Mark the beginning of the next block, as used if the previous block's condition failed
                    if (nextLabel != null)
                        ilg.MarkLabel(nextLabel.Value);
                    nextLabel = ilg.DefineLabel();

                    // Compare map's key with this parameter's name in a case insensitive way
                    ilg.Emit(OpCodes.Ldloc, key);
                    ilg.Emit(OpCodes.Ldstr, parameters[parameterIndex].Name);
                    ilg.Emit(OpCodes.Ldc_I4_5); // StringComparison.OrdinalIgnoreCase
                    ilg.Emit(OpCodes.Callvirt, Methods.String_Equals_String_StringComparison);

                    // If the key doesn't match this property, go to the next block
                    ilg.Emit(OpCodes.Brfalse, nextLabel.Value);

                    #region Test for duplicate fields

                    // Verify we haven't already seen a value for this parameter
                    {
                        // Mask out the LSb and see if it is set. If so, we've seen this property
                        // already in this message, which is invalid.
                        ilg.Emit(OpCodes.Ldloc, valueSetLocals[parameterIndex]);
                        ilg.Emit(OpCodes.Ldc_I4_1);
                        ilg.Emit(OpCodes.And);
                        throwBlocks.ThrowIfTrue(() =>
                        {
                            ilg.Emit(OpCodes.Ldstr, "Encountered duplicate field \"{0}\" for type \"{1}\".");
                            ilg.Emit(OpCodes.Ldloc, key);
                            ilg.Emit(OpCodes.Ldstr, targetType.Name);
                            ilg.Emit(OpCodes.Call, Methods.String_Format_String_Object_Object);
                            throwException();
                        });

                        // Record the fact that we've seen this property
                        ilg.Emit(OpCodes.Ldloc, valueSetLocals[parameterIndex]);
                        ilg.Emit(OpCodes.Ldc_I4_1);
                        ilg.Emit(OpCodes.Or);
                        ilg.Emit(OpCodes.Stloc, valueSetLocals[parameterIndex]);
                    }

                    #endregion

                    if (!DeserialiserEmitter.TryEmitDeserialiseCode(ilg, throwBlocks, errors, parameters[parameterIndex].Name, targetType, valueLocals[parameterIndex], unpacker, context, contextLocal, unexpectedFieldBehaviour))
                        throw new Exception($"Unable to deserialise values of type {valueLocals[parameterIndex].LocalType} from MsgPack data.");

                    ilg.Emit(OpCodes.Br, lblEndIfChain);
                }

                if (nextLabel != null)
                    ilg.MarkLabel(nextLabel.Value);

                #region Process unexpected field

                // If we got here then the property was not recognised. Either throw or ignore, depending upon configuration.
                if (unexpectedFieldBehaviour == UnexpectedFieldBehaviour.Throw)
                {
                    throwBlocks.Throw(() =>
                    {
                        ilg.Emit(OpCodes.Ldstr, "Encountered unexpected field \"{0}\" of MsgPack format \"{1}\" for CLR type \"{2}\".");
                        ilg.Emit(OpCodes.Ldloc, key);
                        loadPeekedFormatName();
                        ilg.Emit(OpCodes.Ldstr, targetType.Name);
                        ilg.Emit(OpCodes.Call, Methods.String_Format_String_Object_Object_Object);
                        throwException();
                    });
                }
                else
                {
                    // skip unexpected value
                    ilg.Emit(OpCodes.Ldloc, unpacker);
                    ilg.Emit(OpCodes.Call, Methods.Unpacker_SkipValue);
                }

                #endregion

                ilg.MarkLabel(lblEndIfChain);

                #endregion

                #region Evaluate loop and branch to start unless finished

                // Increment the loop index
                ilg.Emit(OpCodes.Ldloc, loopIndex);
                ilg.Emit(OpCodes.Ldc_I4_1);
                ilg.Emit(OpCodes.Conv_I8);
                ilg.Emit(OpCodes.Add);
                ilg.Emit(OpCodes.Stloc, loopIndex);

                // Loop condition
                ilg.MarkLabel(lblLoopTest);
                ilg.Emit(OpCodes.Ldloc, loopIndex);
                ilg.Emit(OpCodes.Ldloc, mapSize);
                ilg.Emit(OpCodes.Conv_I8, mapSize);
                // If the loop is done, jump to the first instruction after the loop
                ilg.Emit(OpCodes.Beq_S, lblLoopExit);

                // Jump back to the start of the loop
                ilg.Emit(OpCodes.Br, lblLoopStart);

                // Mark the end of the loop
                ilg.MarkLabel(lblLoopExit);

                #endregion
            }

            #endregion

            #region Verify all required values either specified or have a default value

            var paramName = ilg.DeclareLocal(typeof(string));
            for (var i = 0; i < valueSetLocals.Length; i++)
            {
                // Store the name of the parameter we are inspecting
                // for use in the exception later.
                ilg.Emit(OpCodes.Ldstr, parameters[i].Name);
                ilg.Emit(OpCodes.Stloc, paramName);

                // If any value is zero then neither a default nor specified value
                // exists for that parameter, and we cannot continue.
                ilg.Emit(OpCodes.Ldloc, valueSetLocals[i]);
                ilg.Emit(OpCodes.Ldc_I4_0);
                throwBlocks.ThrowIfEqual(() =>
                {
                    ilg.Emit(OpCodes.Ldstr, "Missing required field \"{0}\" for type \"{1}\".");
                    ilg.Emit(OpCodes.Ldloc, paramName);
                    ilg.Emit(OpCodes.Ldstr, targetType.Name);
                    ilg.Emit(OpCodes.Call, Methods.String_Format_String_Object_Object);
                    throwException();
                });
            }

            // If we got here, all value exist and we're good to continue.

            #endregion

            #region Construct final complex object via its constructor

            // Push all values onto the execution stack
            foreach (var valueLocal in valueLocals)
                ilg.Emit(OpCodes.Ldloc, valueLocal);

            // Call the target type's constructor
            ilg.Emit(OpCodes.Newobj, constructor);

            ilg.Emit(OpCodes.Stloc, value);

            #endregion

            return true;
        }
    }
}