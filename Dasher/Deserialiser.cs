using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

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
    }

    public sealed class Deserialiser
    {
        private readonly Func<Unpacker, object> _func;

        public Deserialiser(Type type, UnexpectedFieldBehaviour unexpectedFieldBehaviour = UnexpectedFieldBehaviour.Throw)
        {
            _func = BuildUnpacker(type, unexpectedFieldBehaviour);
        }

        public object Deserialise(byte[] bytes)
        {
            return Deserialise(new Unpacker(new MemoryStream(bytes)));
        }

        public object Deserialise(Unpacker unpacker)
        {
            return _func(unpacker);
        }

        public object Deserialise(Stream stream)
        {
            return Deserialise(new Unpacker(stream));
        }

        private static Func<Unpacker, object> BuildUnpacker(Type type, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
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
                new[] {typeof(Unpacker) });

            var ilg = method.GetILGenerator();

            #endregion

            #region Initialise locals

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
                LoadType(ilg, type);
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
                ilg.Emit(OpCodes.Ldarg_0); // unpacker
                ilg.Emit(OpCodes.Ldloca, mapSize);
                ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadMapLength)));

                // If false was returned, the data stream ended
                var ifLabel = ilg.DefineLabel();
                ilg.Emit(OpCodes.Brtrue, ifLabel);
                {
                    ilg.Emit(OpCodes.Ldarg_0); // unpacker
                    ilg.Emit(OpCodes.Call, typeof(Unpacker).GetProperty(nameof(Unpacker.HasStreamEnded)).GetMethod);
                    var lblNotEmpty = ilg.DefineLabel();
                    ilg.Emit(OpCodes.Brfalse, lblNotEmpty);
                    ilg.Emit(OpCodes.Ldstr, "Data stream empty");
                    throwException();
                    ilg.MarkLabel(lblNotEmpty);
                    ilg.Emit(OpCodes.Ldstr, "Message must be encoded as a MsgPack map");
                    throwException();
                }
                ilg.MarkLabel(ifLabel);
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
                    ilg.Emit(OpCodes.Ldarg_0); // unpacker
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

                    ReadPropertyValue(ilg, valueLocals[parameterIndex], parameters[parameterIndex].Name, type, unexpectedFieldBehaviour);

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
            return (Func<Unpacker, object>)method.CreateDelegate(typeof(Func<Unpacker, object>));
        }

        #region Read value to local

        private static readonly Dictionary<Type, MethodInfo> _unpackerTryReadMethodByType = new Dictionary<Type, MethodInfo>
        {
            {typeof(sbyte),  typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadSByte))},
            {typeof(byte),   typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadByte))},
            {typeof(short),  typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadInt16))},
            {typeof(ushort), typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadUInt16))},
            {typeof(int),    typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadInt32))},
            {typeof(uint),   typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadUInt32))},
            {typeof(long),   typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadInt64))},
            {typeof(ulong),  typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadUInt64))},
            {typeof(float),  typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadSingle))},
            {typeof(double), typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadDouble))},
            {typeof(bool),   typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadBoolean))},
            {typeof(string), typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadString), new[] {typeof(string).MakeByRefType()})},
            {typeof(byte[]), typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadBinary))}
        };

        private static void ReadPropertyValue(ILGenerator ilg, LocalBuilder local, string name, Type targetType, UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            // TODO DateTime, TimeSpan

            var type = local.LocalType;

            MethodInfo unpackerMethod;
            if (_unpackerTryReadMethodByType.TryGetValue(type, out unpackerMethod))
            {
                ilg.Emit(OpCodes.Ldarg_0); // unpacker
                ilg.Emit(OpCodes.Ldloca, local);
                ilg.Emit(OpCodes.Call, unpackerMethod);

                // If the unpacker method failed (returned false), throw
                var typeGetterSuccess = ilg.DefineLabel();
                ilg.Emit(OpCodes.Brtrue, typeGetterSuccess);
                {
                    var format = ilg.DeclareLocal(typeof(Format));
                    ilg.Emit(OpCodes.Ldarg_0);
                    ilg.Emit(OpCodes.Ldloca, format);
                    ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryPeekFormat)));
                    ilg.Emit(OpCodes.Pop);

                    ilg.Emit(OpCodes.Ldstr, "Unexpected type for \"{0}\". Expected {1}, got {2}.");
                    ilg.Emit(OpCodes.Ldstr, name);
                    ilg.Emit(OpCodes.Ldstr, type.Name);
                    ilg.Emit(OpCodes.Ldloc, format);
                    ilg.Emit(OpCodes.Box, typeof(Format));
                    ilg.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Format), new[] { typeof(string), typeof(object), typeof(object), typeof(object) }));
                    LoadType(ilg, targetType);
                    ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] {typeof(string), typeof(Type)}));
                    ilg.Emit(OpCodes.Throw);
                }
                ilg.MarkLabel(typeGetterSuccess);
                return;
            }

            if (type == typeof(decimal))
            {
                // Read value as a string
                var s = ilg.DeclareLocal(typeof(string));

                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ldloca, s);
                ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadString), new[] { typeof(string).MakeByRefType() }));

                ilg.Emit(OpCodes.Ldloc, s); // unpacker
                ilg.Emit(OpCodes.Ldloca, local);
                ilg.Emit(OpCodes.Call, typeof(decimal).GetMethod(nameof(decimal.TryParse), new[] { typeof(string), typeof(decimal).MakeByRefType() }));

                ilg.Emit(OpCodes.And);

                // If the unpacker method failed (returned false), throw
                var typeGetterSuccess = ilg.DefineLabel();
                ilg.Emit(OpCodes.Brtrue, typeGetterSuccess);
                {
                    // TODO throw better exception
                    ilg.Emit(OpCodes.Ldstr, "TEST THIS CASE 4b");
                    ilg.Emit(OpCodes.Newobj, typeof(Exception).GetConstructor(new[] { typeof(string) }));
                    ilg.Emit(OpCodes.Throw);
                }
                ilg.MarkLabel(typeGetterSuccess);
                return;
            }

            if (type.IsEnum)
            {
                // Read value as a string
                var s = ilg.DeclareLocal(typeof(string));

                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ldloca, s);
                ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadString), new[] { typeof(string).MakeByRefType() }));

                var lbl1 = ilg.DefineLabel();
                ilg.Emit(OpCodes.Brtrue, lbl1);
                {
                    ilg.Emit(OpCodes.Ldstr, "Unable to read string value for enum property {0} of type {1}");
                    ilg.Emit(OpCodes.Ldstr, name);
                    LoadType(ilg, type);
                    ilg.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Format), new[] { typeof(string), typeof(object), typeof(object) }));
                    LoadType(ilg, targetType);
                    ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] { typeof(string), typeof(Type) }));
                    ilg.Emit(OpCodes.Throw);
                }
                ilg.MarkLabel(lbl1);

                ilg.Emit(OpCodes.Ldloc, s);
                ilg.Emit(OpCodes.Ldc_I4_1);
                ilg.Emit(OpCodes.Ldloca, local);
                ilg.Emit(OpCodes.Call, typeof(Enum).GetMethods(BindingFlags.Static | BindingFlags.Public).Single(m => m.Name == "TryParse" && m.GetParameters().Length == 3).MakeGenericMethod(type));

                var lbl2 = ilg.DefineLabel();
                ilg.Emit(OpCodes.Brtrue, lbl2);
                {
                    ilg.Emit(OpCodes.Ldstr, "Unable to parse value \"{0}\" as a member of enum type {1}");
                    ilg.Emit(OpCodes.Ldloc, s);
                    LoadType(ilg, type);
                    ilg.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Format), new[] { typeof(string), typeof(object), typeof(object) }));
                    LoadType(ilg, targetType);
                    ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] { typeof(string), typeof(Type) }));
                    ilg.Emit(OpCodes.Throw);
                }
                ilg.MarkLabel(lbl2);
                return;
            }

            var listType = type.GetInterfaces().SingleOrDefault(i => i.Name == "IReadOnlyCollection`1" && i.Namespace == "System.Collections.Generic");
            if (listType != null)
            {
                var elementType = listType.GetGenericArguments().Single();

                // read list length
                var count = ilg.DeclareLocal(typeof(int));
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ldloca, count);
                ilg.Emit(OpCodes.Call, typeof(Unpacker).GetMethod(nameof(Unpacker.TryReadArrayLength)));

                // verify read correctly
                var lbl1 = ilg.DefineLabel();
                ilg.Emit(OpCodes.Brtrue, lbl1);
                {
                    ilg.Emit(OpCodes.Ldstr, "Expecting collection data to be encoded as array");
                    LoadType(ilg, targetType);
                    ilg.Emit(OpCodes.Newobj, typeof(DeserialisationException).GetConstructor(new[] { typeof(string), typeof(Type) }));
                    ilg.Emit(OpCodes.Throw);
                }
                ilg.MarkLabel(lbl1);

                // create an array to store values
                ilg.Emit(OpCodes.Ldloc, count);
                ilg.Emit(OpCodes.Newarr, elementType);

                var array = ilg.DeclareLocal(elementType.MakeArrayType());
                ilg.Emit(OpCodes.Stloc, array);

                // begin loop
                var loopStart = ilg.DefineLabel();
                var loopTest = ilg.DefineLabel();
                var loopEnd = ilg.DefineLabel();

                var i = ilg.DeclareLocal(typeof(int));
                ilg.Emit(OpCodes.Ldc_I4_0);
                ilg.Emit(OpCodes.Stloc, i);

                ilg.Emit(OpCodes.Br, loopTest);
                ilg.MarkLabel(loopStart);

                // loop body
                var element = ilg.DeclareLocal(elementType);
                ReadPropertyValue(ilg, element, name, targetType, unexpectedFieldBehaviour);

                ilg.Emit(OpCodes.Ldloc, array);
                ilg.Emit(OpCodes.Ldloc, i);
                ilg.Emit(OpCodes.Ldloc, element);
                ilg.Emit(OpCodes.Stelem, elementType);

                // loop counter increment
                ilg.Emit(OpCodes.Ldloc, i);
                ilg.Emit(OpCodes.Ldc_I4_1);
                ilg.Emit(OpCodes.Add);
                ilg.Emit(OpCodes.Stloc, i);

                // loop test
                ilg.MarkLabel(loopTest);
                ilg.Emit(OpCodes.Ldloc, i);
                ilg.Emit(OpCodes.Ldloc, count);
                ilg.Emit(OpCodes.Clt);
                ilg.Emit(OpCodes.Brtrue, loopStart);

                // after loop
                ilg.MarkLabel(loopEnd);

                ilg.Emit(OpCodes.Ldloc, array);
                ilg.Emit(OpCodes.Stloc, local);
                return;
            }

            if (type.IsClass && type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length == 1)
            {
                // TODO should support complex structs too
                // TODO cache subtype deserialiser instances in fields of generated class (requires moving away from DynamicMethod)
                LoadType(ilg, type);
                ilg.Emit(OpCodes.Ldc_I4, (int)unexpectedFieldBehaviour);
                ilg.Emit(OpCodes.Newobj, typeof(Deserialiser).GetConstructor(new[] {typeof(Type), typeof(UnexpectedFieldBehaviour)}));
                ilg.Emit(OpCodes.Ldarg_0); // unpacker
                ilg.Emit(OpCodes.Call, typeof(Deserialiser).GetMethod(nameof(Deserialiser.Deserialise), new[] {typeof(Unpacker)}));
                ilg.Emit(OpCodes.Castclass, type);
                ilg.Emit(OpCodes.Stloc, local);
                return;
            }

            throw new NotImplementedException($"No support yet exists for reading values of type {type} from MsgPack data");
        }

        private static void LoadType(ILGenerator ilg, Type type)
        {
            ilg.Emit(OpCodes.Ldtoken, type);
            ilg.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)));
        }

        #endregion

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