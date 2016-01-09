#region License
//
// Dasher
//
// Copyright 2015 Drew Noakes
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
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Dasher.TypeProviders;

namespace Dasher
{
    public enum UnexpectedFieldBehaviour
    {
        Throw,
        Ignore
    }

    public sealed class Deserialiser<T>
    {
        private readonly Deserialiser _inner;

        public Deserialiser(UnexpectedFieldBehaviour unexpectedFieldBehaviour = UnexpectedFieldBehaviour.Throw)
        {
            _inner = new Deserialiser(typeof(T), unexpectedFieldBehaviour);
        }

        public T Deserialise(byte[] bytes) => (T)_inner.Deserialise(bytes);

        public T Deserialise(Stream stream) => (T)_inner.Deserialise(stream);

        public T Deserialise(Unpacker unpacker) => (T)_inner.Deserialise(unpacker);
    }

    public sealed class Deserialiser
    {
        private readonly Func<Unpacker, DasherContext, object> _func;
        private readonly DasherContext _context;

        public Deserialiser(Type type, UnexpectedFieldBehaviour unexpectedFieldBehaviour = UnexpectedFieldBehaviour.Throw, DasherContext context = null)
        {
            _context = context ?? new DasherContext();
            _func = BuildUnpacker(type, unexpectedFieldBehaviour, _context);
        }

        public object Deserialise(byte[] bytes) => Deserialise(new Unpacker(new MemoryStream(bytes)));

        public object Deserialise(Stream stream) => Deserialise(new Unpacker(stream));

        public object Deserialise(Unpacker unpacker) => _func(unpacker, _context);

        private static Func<Unpacker, DasherContext, object> BuildUnpacker(Type type, UnexpectedFieldBehaviour unexpectedFieldBehaviour, DasherContext context)
        {
            #region Verify and prepare for target type

            if (type.IsPrimitive)
                throw new Exception("TEST THIS CASE 1");

            var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            if (ctors.Length != 1)
                throw new DeserialisationException("Type must have a single public constructor.", type);
            var ctor = ctors[0];

            var parameters = ctor.GetParameters();

            #endregion

            #region Initialise code gen

            var method = new DynamicMethod(
                $"Deserialiser{type.Name}",
                typeof(object),
                new[] {typeof(Unpacker), typeof(DasherContext) });

            var ilg = method.GetILGenerator();

            #endregion

            #region Initialise locals

            // Store the Unpacker arg as a local so we can pass it around
            var unpacker = ilg.DeclareLocal(typeof(Unpacker));
            ilg.Emit(OpCodes.Ldarg_0);
            ilg.Emit(OpCodes.Stloc, unpacker);

            // Store the context as a local so we can pass it around
            var contextLocal = ilg.DeclareLocal(typeof(DasherContext));
            ilg.Emit(OpCodes.Ldarg_1);
            ilg.Emit(OpCodes.Stloc, contextLocal);

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
                    LoadConstant(ilg, parameter.DefaultValue);
                    ilg.Emit(OpCodes.Stloc, valueLocals[i]);
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

            Action throwException = () =>
            {
                ilg.LoadType(type);
                ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] {typeof(string), typeof(Type)}));
                ilg.Emit(OpCodes.Throw);
            };

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
                ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadMapLength)));

                // If false was returned, the data stream ended
                var lblHaveMapSize = ilg.DefineLabel();
                ilg.Emit(OpCodes.Brtrue, lblHaveMapSize);
                {
                    ilg.Emit(OpCodes.Ldloc, unpacker);
                    ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadNull)));
                    var lblNotNull = ilg.DefineLabel();
                    ilg.Emit(OpCodes.Brfalse, lblNotNull);
                    {
                        // value is null
                        ilg.Emit(OpCodes.Ldnull);
                        ilg.Emit(OpCodes.Ret);
                    }
                    ilg.MarkLabel(lblNotNull);
                    ilg.Emit(OpCodes.Ldloc, unpacker);
                    ilg.Emit(OpCodes.Call, typeof(Unpacker).GetProperty(nameof(Unpacker.HasStreamEnded)).GetMethod);
                    var lblNotEmpty = ilg.DefineLabel();
                    ilg.Emit(OpCodes.Brfalse, lblNotEmpty);
                    ilg.Emit(OpCodes.Ldstr, "Data stream empty");
                    throwException();
                    ilg.MarkLabel(lblNotEmpty);
                    ilg.Emit(OpCodes.Ldstr, "Message must be encoded as a MsgPack map");
                    throwException();
                }
                ilg.MarkLabel(lblHaveMapSize);
            }

            #endregion

            // For each key/value pair in the map...
            {
                // Create a loop counter, initialised to zero
                var loopIndex = ilg.DeclareLocal(typeof(long));
                ilg.Emit(OpCodes.Ldc_I4_0);
                ilg.Emit(OpCodes.Conv_I8);
                ilg.Emit(OpCodes.Stloc, loopIndex);

                // Create labels to jump to within the loop
                var lblLoopTest = ilg.DefineLabel();   // Comparing counter to map size
                var lblLoopExit = ilg.DefineLabel();   // The first instruction after the loop
                var lblLoopStart = ilg.DefineLabel();  // The first instruction within the loop

                // Run the test first
                ilg.Emit(OpCodes.Br, lblLoopTest);

                // Mark the first instruction within the loop
                ilg.MarkLabel(lblLoopStart);

                // Although MsgPack allows map keys to be of any arbitrary type, our convention
                // is to require keys to be strings. We read the key here.
                var key = ilg.DeclareLocal(typeof(string));
                {
                    ilg.Emit(OpCodes.Ldloc, unpacker);
                    ilg.Emit(OpCodes.Ldloca, key);
                    ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadString), new[] {typeof(string).MakeByRefType()}));

                    // If false was returned, the data stream ended
                    var ifLabel = ilg.DefineLabel();
                    ilg.Emit(OpCodes.Brtrue, ifLabel);
                    {
                        ilg.Emit(OpCodes.Ldstr, "Data stream ended.");
                        throwException();
                    }
                    ilg.MarkLabel(ifLabel);
                }

                // Build a chain of if/elseif/elseif... blocks for each of the expected fields.
                // It could be slightly more efficient here to generate a O(log(N)) tree-based lookup,
                // but that would take quite some engineering. Let's see if there's a significant perf
                // hit here or not first.
                var lblEndIfChain = ilg.DefineLabel();
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
                    ilg.Emit(OpCodes.Ldc_I4_5);
                    ilg.Emit(OpCodes.Callvirt, typeof(string).GetMethod("Equals", new[] {typeof(string), typeof(StringComparison)}));

                    // If the key doesn't match this property, go to the next block
                    ilg.Emit(OpCodes.Brfalse, nextLabel.Value);

                    // Verify we haven't already seen a value for this parameter
                    {
                        // Mask out the LSb and see if it is set. If so, we've seen this property
                        // already in this message, which is invalid.
                        ilg.Emit(OpCodes.Ldloc, valueSetLocals[parameterIndex]);
                        ilg.Emit(OpCodes.Ldc_I4_1);
                        ilg.Emit(OpCodes.And);
                        var notSeenLabel = ilg.DefineLabel();
                        ilg.Emit(OpCodes.Brfalse, notSeenLabel);
                        {
                            ilg.Emit(OpCodes.Ldstr, "Encountered duplicate field \"{0}\".");
                            ilg.Emit(OpCodes.Ldloc, key);
                            ilg.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Format), new[] { typeof(string), typeof(object) }));
                            throwException();
                        }

                        ilg.MarkLabel(notSeenLabel);

                        // Record the fact that we've seen this property
                        ilg.Emit(OpCodes.Ldloc, valueSetLocals[parameterIndex]);
                        ilg.Emit(OpCodes.Ldc_I4_1);
                        ilg.Emit(OpCodes.Or);
                        ilg.Emit(OpCodes.Stloc, valueSetLocals[parameterIndex]);
                    }

                    ITypeProvider provider;
                    if (!context.TryGetTypeProvider(valueLocals[parameterIndex].LocalType, out provider))
                        throw new Exception($"Unable to deserialise values of type {valueLocals[parameterIndex].LocalType} from MsgPack data.");

                    provider.Deserialise(ilg, parameters[parameterIndex].Name, type, valueLocals[parameterIndex], unpacker, contextLocal, context, unexpectedFieldBehaviour);

                    ilg.Emit(OpCodes.Br, lblEndIfChain);
                }

                if (nextLabel != null)
                    ilg.MarkLabel(nextLabel.Value);

                // If we got here then the property was not recognised. Either throw or ignore, depending upon configuration.
                if (unexpectedFieldBehaviour == UnexpectedFieldBehaviour.Throw)
                {
                    ilg.Emit(OpCodes.Ldstr, "Encountered unexpected field \"{0}\".");
                    ilg.Emit(OpCodes.Ldloc, key);
                    ilg.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Format), new[] {typeof(string), typeof(object)}));
                    throwException();
                }
                else
                {
                    // skip unexpected value
                    ilg.Emit(OpCodes.Ldloc, unpacker);
                    ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.SkipValue)));
                }

                ilg.MarkLabel(lblEndIfChain);

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
                ilg.Emit(OpCodes.Beq, lblLoopExit);

                // Jump back to the start of the loop
                ilg.Emit(OpCodes.Br, lblLoopStart);

                // Mark the end of the loop
                ilg.MarkLabel(lblLoopExit);
            }

            #region Verify all required values either specified or have a default value

            var lblValuesMissing = ilg.DefineLabel();
            var lblValuesOk = ilg.DefineLabel();

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
                ilg.Emit(OpCodes.Beq, lblValuesMissing);
            }

            // If we got here, all value exist and we're good to continue.
            // Jump to the next section.
            ilg.Emit(OpCodes.Br, lblValuesOk);
            {
                // If we got here then one or more values is missing.
                ilg.MarkLabel(lblValuesMissing);
                ilg.Emit(OpCodes.Ldstr, "Missing required field \"{0}\".");
                ilg.Emit(OpCodes.Ldloc, paramName);
                ilg.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Format), new[] { typeof(string), typeof(object) }));
                throwException();
            }
            ilg.MarkLabel(lblValuesOk);

            #endregion

            // Push all values onto the execution stack
            foreach (var valueLocal in valueLocals)
                ilg.Emit(OpCodes.Ldloc, valueLocal);

            // Call the target type's constructor
            ilg.Emit(OpCodes.Newobj, ctor);

            if (type.IsValueType)
                ilg.Emit(OpCodes.Box, type);

            // Return the newly constructed object!
            ilg.Emit(OpCodes.Ret);

            // Return a delegate that performs the above operations
            return (Func<Unpacker, DasherContext, object>)method.CreateDelegate(typeof(Func<Unpacker, DasherContext, object>));
        }

        private static void LoadConstant(ILGenerator ilg, object value)
        {
            if (value == null)
                ilg.Emit(OpCodes.Ldnull);
            else if (value is int)
                ilg.Emit(OpCodes.Ldc_I4, (int)value);
            else if (value is uint)
                ilg.Emit(OpCodes.Ldc_I4, (int)(uint)value);
            else if (value is byte)
                ilg.Emit(OpCodes.Ldc_I4, (int)(byte)value);
            else if (value is sbyte)
                ilg.Emit(OpCodes.Ldc_I4, (int)(sbyte)value);
            else if (value is short)
                ilg.Emit(OpCodes.Ldc_I4, (int)(short)value);
            else if (value is ushort)
                ilg.Emit(OpCodes.Ldc_I4, (int)(ushort)value);
            else if (value is long)
                ilg.Emit(OpCodes.Ldc_I8, (long)value);
            else if (value is ulong)
                ilg.Emit(OpCodes.Ldc_I8, (long)(ulong)value);
            else if (value is string)
                ilg.Emit(OpCodes.Ldstr, (string)value);
            else if (value is bool)
                ilg.Emit(OpCodes.Ldc_I4, (bool)value ? 1 : 0);
            else if (value is float)
                ilg.Emit(OpCodes.Ldc_R4, (float)value);
            else if (value is double)
                ilg.Emit(OpCodes.Ldc_R8, (double)value);
            else if (value is decimal)
            {
                var bits = decimal.GetBits((decimal)value);
                ilg.Emit(OpCodes.Ldc_I4_4);
                ilg.Emit(OpCodes.Newarr, typeof(int));
                for (var i = 0; i < 4; i++)
                {
                    ilg.Emit(OpCodes.Dup);
                    ilg.Emit(OpCodes.Ldc_I4, i); // index
                    ilg.Emit(OpCodes.Ldc_I4, bits[i]); // value
                    ilg.Emit(OpCodes.Stelem_I4);
                }
                ilg.Emit(OpCodes.Newobj, typeof(decimal).GetConstructor(new[] {typeof(int[])}));
            }
            else if (value.GetType().IsEnum)
            {
                // TODO test and cater for non-4-byte enums too
                ilg.Emit(OpCodes.Ldc_I4, (int)value);
            }
            else
            {
                throw new NotImplementedException($"No support for default values of type {value?.GetType().Name} (yet).");
            }
        }
    }
}