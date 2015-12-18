using System;
using System.Collections.Generic;
using System.Reflection;

namespace MsgPack.Strict
{
    public static class ValueUnpacker
    {
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
        public static string TryReadComplexName = nameof(TryReadComplex);
        private static readonly Dictionary<Type, MethodInfo> _typeGetters = new Dictionary<Type, MethodInfo>
        {
            // TODO DateTime, TimeSpan
            // TODO IReadOnlyList<T>
            // TODO complex types
            {typeof(sbyte),   typeof(ValueUnpacker).GetMethod(nameof(TryReadSByte),   BindingFlags.Static | BindingFlags.Public)},
            {typeof(byte),    typeof(ValueUnpacker).GetMethod(nameof(TryReadByte),    BindingFlags.Static | BindingFlags.Public)},
            {typeof(short),   typeof(ValueUnpacker).GetMethod(nameof(TryReadShort),   BindingFlags.Static | BindingFlags.Public)},
            {typeof(ushort),  typeof(ValueUnpacker).GetMethod(nameof(TryReadUShort),  BindingFlags.Static | BindingFlags.Public)},
            {typeof(int),     typeof(ValueUnpacker).GetMethod(nameof(TryReadInt),     BindingFlags.Static | BindingFlags.Public)},
            {typeof(uint),    typeof(ValueUnpacker).GetMethod(nameof(TryReadUInt),    BindingFlags.Static | BindingFlags.Public)},
            {typeof(long),    typeof(ValueUnpacker).GetMethod(nameof(TryReadLong),    BindingFlags.Static | BindingFlags.Public)},
            {typeof(ulong),   typeof(ValueUnpacker).GetMethod(nameof(TryReadULong),   BindingFlags.Static | BindingFlags.Public)},
            /* TODO
            {typeof(float),   typeof(ValueUnpacker).GetMethod(nameof(TryReadFloat),   BindingFlags.Static | BindingFlags.Public)},
            {typeof(double),  typeof(ValueUnpacker).GetMethod(nameof(TryReadDouble),  BindingFlags.Static | BindingFlags.Public)},
            */
            {typeof(bool),    typeof(ValueUnpacker).GetMethod(nameof(TryReadBool),    BindingFlags.Static | BindingFlags.Public)},
            {typeof(string),  typeof(ValueUnpacker).GetMethod(nameof(TryReadString),  BindingFlags.Static | BindingFlags.Public)},
            {typeof(decimal), typeof(ValueUnpacker).GetMethod(nameof(TryReadDecimal), BindingFlags.Static | BindingFlags.Public)}
        };

        public static MethodInfo GetUnpackerMethodForType(Type type)
        {
            MethodInfo methodInfo;
            if (_typeGetters.TryGetValue(type, out methodInfo))
                return methodInfo;

            return typeof (ValueUnpacker).GetMethod(nameof(TryReadComplex), BindingFlags.Static | BindingFlags.Public);
        }

        public static bool TryReadSByte(MsgPackUnpacker unpacker, out sbyte value) => unpacker.TryReadSByte(out value);
        public static bool TryReadByte(MsgPackUnpacker unpacker, out byte value) => unpacker.TryReadByte(out value);
        public static bool TryReadShort(MsgPackUnpacker unpacker, out short value) => unpacker.TryReadInt16(out value);
        public static bool TryReadUShort(MsgPackUnpacker unpacker, out ushort value) => unpacker.TryReadUInt16(out value);
        public static bool TryReadInt(MsgPackUnpacker unpacker, out int value) => unpacker.TryReadInt32(out value);
        public static bool TryReadUInt(MsgPackUnpacker unpacker, out uint value) => unpacker.TryReadUInt32(out value);
        public static bool TryReadLong(MsgPackUnpacker unpacker, out long value) => unpacker.TryReadInt64(out value);
        public static bool TryReadULong(MsgPackUnpacker unpacker, out ulong value) => unpacker.TryReadUInt64(out value);
        public static bool TryReadBool(MsgPackUnpacker unpacker, out bool value) => unpacker.TryReadBool(out value);
        /* TODO
        public static bool TryReadFloat(MsgPackUnpacker unpacker, out float value) => unpacker.TryReadFloat(out value);
        public static bool TryReadDouble(MsgPackUnpacker unpacker, out double value) => unpacker.TryReadDouble(out value);
        */
        public static bool TryReadString(MsgPackUnpacker unpacker, out string value) => unpacker.TryReadString(out value);

        public static bool TryReadDecimal(MsgPackUnpacker unpacker, out decimal value)
        {
            string s;
            if (!unpacker.TryReadString(out s))
            {
                value = default(decimal);
                return false;
            }
            return decimal.TryParse(s, out value);
        }

        public static bool TryReadComplex<T>(MsgPackUnpacker unpacker, out T value)
        {
            value = (T)StrictDeserialiser.Get(typeof(T)).Deserialise(unpacker);
            return true;
        }

        #endregion
    }
}