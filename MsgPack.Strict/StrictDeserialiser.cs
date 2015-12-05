using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace MsgPack.Strict
{
    public sealed class StrictDeserialiser<T>
    {
        private readonly StrictDeserialiser _inner;

        internal StrictDeserialiser(StrictDeserialiser inner)
        {
            _inner = inner;
        }

        public T Deserialise(byte[] bytes) => (T)_inner.Deserialise(bytes);
    }

    public sealed class StrictDeserialiser
    {
        #region Instance accessors

        private static readonly ConcurrentDictionary<Type, StrictDeserialiser> _deserialiserByType = new ConcurrentDictionary<Type, StrictDeserialiser>();

        public static StrictDeserialiser<T> Get<T>()
        {
            return new StrictDeserialiser<T>(Get(typeof(T)));
        }

        public static StrictDeserialiser Get(Type type)
        {
            StrictDeserialiser deserialiser;
            if (_deserialiserByType.TryGetValue(type, out deserialiser))
                return deserialiser;

            _deserialiserByType.TryAdd(type, new StrictDeserialiser(type));
            var present = _deserialiserByType.TryGetValue(type, out deserialiser);
            Debug.Assert(present);
            return deserialiser;
        }

        #endregion

        private readonly Func<Unpacker, object> _func;

        private StrictDeserialiser(Type type)
        {
            _func = BuildUnpacker(type);
        }

        public object Deserialise(byte[] bytes)
        {
            var unpacker = Unpacker.Create(new MemoryStream(bytes));
            return _func(unpacker);
        }

        private static Func<Unpacker, object> BuildUnpacker(Type type)
        {
            #region Verify and prepare for target type

            if (type.IsPrimitive)
                throw new Exception("TEST THIS CASE");

            var ctor = type.GetConstructors().Single();
            var parameters = ctor.GetParameters();

            #endregion

            #region Initialise code gen

            var method = new DynamicMethod(
                $"Deserialiser{type.Name}",
                typeof(object),
                new[] {typeof(Unpacker)});

            var ilg = method.GetILGenerator();

            #endregion

            Action<string> debug = msg =>
            {
//                ilg.Emit(OpCodes.Ldstr, msg);
//                ilg.Emit(OpCodes.Call, typeof (Debug).GetMethod("WriteLine", new[] {typeof (string)}));
            };

            #region Initialise locals

            var valueLocals = new LocalBuilder[parameters.Length];
            var valueSetLocals = new LocalBuilder[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                valueLocals[i] = ilg.DeclareLocal(parameter.ParameterType);
                valueSetLocals[i] = ilg.DeclareLocal(typeof(bool));

                if (parameter.HasDefaultValue)
                {
                    // set default values on params
                    StoreValue(ilg, parameter.DefaultValue);
                    ilg.Emit(OpCodes.Stloc, valueLocals[i]);
                    // set 'valueSet' to true
                    ilg.Emit(OpCodes.Ldc_I4_1);
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

            var mapSize = ilg.DeclareLocal(typeof(long));
            {
                // read its length
                ilg.Emit(OpCodes.Ldarg_0); // unpacker
                ilg.Emit(OpCodes.Ldloca, mapSize);
                ilg.Emit(OpCodes.Callvirt, typeof(Unpacker).GetMethod("ReadMapLength"));
                // return value should be true
                var ifLabel = ilg.DefineLabel();
                ilg.Emit(OpCodes.Brtrue, ifLabel);
                {
                    ilg.Emit(OpCodes.Ldtoken, type);
                    ilg.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                    ilg.Emit(OpCodes.Ldstr, "Data stream ended.");
                    ilg.Emit(OpCodes.Newobj, typeof(StrictDeserialisationException).GetConstructor(new[] {typeof(Type), typeof(string)}));
                    ilg.Emit(OpCodes.Throw);
                }
                ilg.MarkLabel(ifLabel);
            }

            #endregion

            // for each item in map
            {
                debug("Starting loop");

                // var loopIndex;
                var loopIndex = ilg.DeclareLocal(typeof(long));
                // loopIndex = 0;
                ilg.Emit(OpCodes.Ldc_I4_0);
                ilg.Emit(OpCodes.Conv_I8);
                ilg.Emit(OpCodes.Stloc, loopIndex);

                var lblLoopTest = ilg.DefineLabel();
                var lblLoopExit = ilg.DefineLabel();
                var lblLoopStart = ilg.DefineLabel();

                ilg.Emit(OpCodes.Br, lblLoopTest);

                ilg.MarkLabel(lblLoopStart);

                debug("=============================");
                debug("<loop start>");

                debug("Reading key");

                // read the key
                var key = ilg.DeclareLocal(typeof(string));
                {
                    ilg.Emit(OpCodes.Ldarg_0); // unpacker
                    ilg.Emit(OpCodes.Ldloca, key);
                    ilg.Emit(OpCodes.Callvirt, typeof(Unpacker).GetMethod("ReadString"));
                    // return value should be true
                    var ifLabel = ilg.DefineLabel();
                    ilg.Emit(OpCodes.Brtrue, ifLabel);
                    {
                        // TODO throw better exception
                        ilg.Emit(OpCodes.Ldstr, "TEST THIS CASE");
                        ilg.Emit(OpCodes.Newobj, typeof(Exception).GetConstructor(new[] {typeof(string)}));
                        ilg.Emit(OpCodes.Throw);
                    }
                    ilg.MarkLabel(ifLabel);
                }

                // DEBUG CODE
                ilg.Emit(OpCodes.Ldloc, key);
                ilg.Emit(OpCodes.Call, typeof(Debug).GetMethod("WriteLine", new[] {typeof(string)}));

                Label? nextLabel = null;
                for (var parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
                {
                    // wire up labels between consecutive cascading if/elif/elif... blocks
                    if (nextLabel != null)
                        ilg.MarkLabel(nextLabel.Value);
                    nextLabel = ilg.DefineLabel();

                    debug("-----------------");
                    debug($"Testing parameter #{parameterIndex}: key == {parameters[parameterIndex].Name}");

                    // compare map's key with this parameter's name in a case insensitive way
                    ilg.Emit(OpCodes.Ldloc, key);
                    ilg.Emit(OpCodes.Ldstr, parameters[parameterIndex].Name);
                    ilg.Emit(OpCodes.Ldc_I4_5);
                    ilg.Emit(OpCodes.Callvirt, typeof(string).GetMethod("Equals", new[] {typeof(string), typeof(StringComparison)}));

                    // if the key doesn't match this property, go to the next block
                    ilg.Emit(OpCodes.Brfalse, nextLabel.Value);

                    debug("Found parameter");

                    // read value
                    MethodInfo methodInfo;
                    if (!_typeGetters.TryGetValue(parameters[parameterIndex].ParameterType, out methodInfo))
                        throw new NotImplementedException();

                    debug("Found parameter type getter");

                    // verify we haven't already seen a value for this parameter
                    {
                        ilg.Emit(OpCodes.Ldloc, valueSetLocals[parameterIndex]);
                        var notSeenLabel = ilg.DefineLabel();
                        ilg.Emit(OpCodes.Brfalse, notSeenLabel);
                        {
                            // TODO throw better exception
                            ilg.Emit(OpCodes.Ldstr, "TEST THIS CASE");
                            ilg.Emit(OpCodes.Newobj, typeof(Exception).GetConstructor(new[] {typeof(string)}));
                            ilg.Emit(OpCodes.Throw);
                        }

                        ilg.MarkLabel(notSeenLabel);

                        // set 'seen' to true
                        ilg.Emit(OpCodes.Ldc_I4_1);
                        ilg.Emit(OpCodes.Stloc, valueSetLocals[parameterIndex]);
                    }

                    // the 'type getter' expects the unpacker on the stack
                    ilg.Emit(OpCodes.Ldarg_0); // unpacker
                    ilg.Emit(OpCodes.Ldloca, valueLocals[parameterIndex]);
                    ilg.Emit(OpCodes.Call, methodInfo);

                    // the 'type getter' pushes true or false

                    // TODO if fail, throw
                    {
                        var typeGetterSuccess = ilg.DefineLabel();
                        ilg.Emit(OpCodes.Brtrue, typeGetterSuccess);
                        {
                            // TODO throw better exception
                            ilg.Emit(OpCodes.Ldstr, "TEST THIS CASE");
                            ilg.Emit(OpCodes.Newobj, typeof(Exception).GetConstructor(new[] {typeof(string)}));
                            ilg.Emit(OpCodes.Throw);
                        }
                        ilg.MarkLabel(typeGetterSuccess);
                    }
                }

                if (nextLabel != null)
                    ilg.MarkLabel(nextLabel.Value);

                // increment the loop index
                debug("<loop increment>");
                ilg.Emit(OpCodes.Ldloc, loopIndex);
                ilg.Emit(OpCodes.Ldc_I4_1, loopIndex);
                ilg.Emit(OpCodes.Conv_I8);
                ilg.Emit(OpCodes.Add);
                ilg.Emit(OpCodes.Stloc, loopIndex);

                // test loop condition
                ilg.MarkLabel(lblLoopTest);
                debug("<loop test>");
                ilg.Emit(OpCodes.Ldloc, loopIndex);
                ilg.Emit(OpCodes.Ldloc, mapSize);
                ilg.Emit(OpCodes.Beq, lblLoopExit);

                ilg.Emit(OpCodes.Br, lblLoopStart);
                ilg.MarkLabel(lblLoopExit);
                debug("<loop exited>");
            }

            #region Verify all required values specified

            ilg.Emit(OpCodes.Ldc_I4_1);
            foreach (var valueSetLocal in valueSetLocals)
            {
                ilg.Emit(OpCodes.Ldloc, valueSetLocal);
                ilg.Emit(OpCodes.And, valueSetLocal);
            }
            var lblValuesOk = ilg.DefineLabel();
            ilg.Emit(OpCodes.Brtrue, lblValuesOk);
            {
                // TODO throw better exception
                ilg.Emit(OpCodes.Ldstr, "TEST THIS CASE");
                ilg.Emit(OpCodes.Newobj, typeof(Exception).GetConstructor(new[] {typeof(string)}));
                ilg.Emit(OpCodes.Throw);
            }
            ilg.MarkLabel(lblValuesOk);

            #endregion

            foreach (var valueLocal in valueLocals)
                ilg.Emit(OpCodes.Ldloc, valueLocal);
            ilg.Emit(OpCodes.Newobj, ctor);

            ilg.Emit(OpCodes.Ret);

            return (Func<Unpacker, object>)method.CreateDelegate(typeof(Func<Unpacker, object>));
        }

        private static void StoreValue(ILGenerator ilg, object value)
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
            else
            {
                throw new NotImplementedException($"No support for default values of type {value?.GetType().Name} (yet).");
            }
        }

        #region Primitive type getters

        /*
        mTypeHash[typeof(sbyte)]=OpCodes.Ldind_I1;
        mTypeHash[typeof(byte)]=OpCodes.Ldind_U1;
        mTypeHash[typeof(char)]=OpCodes.Ldind_U2;
        mTypeHash[typeof(short)]=OpCodes.Ldind_I2;
        mTypeHash[typeof(ushort)]=OpCodes.Ldind_U2;
        mTypeHash[typeof(int)]=OpCodes.Ldind_I4;
        mTypeHash[typeof(uint)]=OpCodes.Ldind_U4;
        mTypeHash[typeof(long)]=OpCodes.Ldind_I8;
        mTypeHash[typeof(ulong)]=OpCodes.Ldind_I8;
        mTypeHash[typeof(bool)]=OpCodes.Ldind_I1;
        mTypeHash[typeof(double)]=OpCodes.Ldind_R8;
        mTypeHash[typeof(float)]=OpCodes.Ldind_R4;
        */

        private static readonly Dictionary<Type, MethodInfo> _typeGetters = new Dictionary<Type, MethodInfo>
        {
            // TODO DateTime, TimeSpan
            // TODO IReadOnlyList<T>
            // TODO complex types
            {typeof(sbyte),   typeof(StrictDeserialiser).GetMethod(nameof(TryReadSByte),   BindingFlags.Static | BindingFlags.Public)},
            {typeof(byte),    typeof(StrictDeserialiser).GetMethod(nameof(TryReadByte),    BindingFlags.Static | BindingFlags.Public)},
            {typeof(short),   typeof(StrictDeserialiser).GetMethod(nameof(TryReadShort),   BindingFlags.Static | BindingFlags.Public)},
            {typeof(ushort),  typeof(StrictDeserialiser).GetMethod(nameof(TryReadUShort),  BindingFlags.Static | BindingFlags.Public)},
            {typeof(int),     typeof(StrictDeserialiser).GetMethod(nameof(TryReadInt),     BindingFlags.Static | BindingFlags.Public)},
            {typeof(uint),    typeof(StrictDeserialiser).GetMethod(nameof(TryReadUInt),    BindingFlags.Static | BindingFlags.Public)},
            {typeof(long),    typeof(StrictDeserialiser).GetMethod(nameof(TryReadLong),    BindingFlags.Static | BindingFlags.Public)},
            {typeof(ulong),   typeof(StrictDeserialiser).GetMethod(nameof(TryReadULong),   BindingFlags.Static | BindingFlags.Public)},
            {typeof(float),   typeof(StrictDeserialiser).GetMethod(nameof(TryReadFloat),   BindingFlags.Static | BindingFlags.Public)},
            {typeof(double),  typeof(StrictDeserialiser).GetMethod(nameof(TryReadDouble),  BindingFlags.Static | BindingFlags.Public)},
            {typeof(bool),    typeof(StrictDeserialiser).GetMethod(nameof(TryReadBool),    BindingFlags.Static | BindingFlags.Public)},
            {typeof(string),  typeof(StrictDeserialiser).GetMethod(nameof(TryReadString),  BindingFlags.Static | BindingFlags.Public)},
            {typeof(decimal), typeof(StrictDeserialiser).GetMethod(nameof(TryReadDecimal), BindingFlags.Static | BindingFlags.Public)}
        };

        public static bool TryReadSByte  (Unpacker unpacker, out sbyte  value) => unpacker.ReadSByte  (out value);
        public static bool TryReadByte   (Unpacker unpacker, out byte   value) => unpacker.ReadByte   (out value);
        public static bool TryReadShort  (Unpacker unpacker, out short  value) => unpacker.ReadInt16  (out value);
        public static bool TryReadUShort (Unpacker unpacker, out ushort value) => unpacker.ReadUInt16 (out value);
        public static bool TryReadInt    (Unpacker unpacker, out int    value) => unpacker.ReadInt32  (out value);
        public static bool TryReadUInt   (Unpacker unpacker, out uint   value) => unpacker.ReadUInt32 (out value);
        public static bool TryReadLong   (Unpacker unpacker, out long   value) => unpacker.ReadInt64  (out value);
        public static bool TryReadULong  (Unpacker unpacker, out ulong  value) => unpacker.ReadUInt64 (out value);
        public static bool TryReadBool   (Unpacker unpacker, out bool   value) => unpacker.ReadBoolean(out value);
        public static bool TryReadFloat  (Unpacker unpacker, out float  value) => unpacker.ReadSingle (out value);
        public static bool TryReadDouble (Unpacker unpacker, out double value) => unpacker.ReadDouble (out value);
        public static bool TryReadString (Unpacker unpacker, out string value) => unpacker.ReadString (out value);

        public static bool TryReadDecimal(Unpacker unpacker, out decimal value)
        {
            string s;
            if (!unpacker.ReadString(out s))
            {
                value = default(decimal);
                return false;
            }
            return decimal.TryParse(s, out value);
        }

        #endregion
    }
}