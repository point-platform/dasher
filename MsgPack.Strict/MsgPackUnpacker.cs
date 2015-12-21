using System;
using System.IO;
using System.Text;

namespace MsgPack.Strict
{
    public sealed class MsgPackUnpacker
    {
        private readonly Stream _stream;
        private int _nextByte = -1;

        public MsgPackUnpacker(Stream stream)
        {
            _stream = stream;
        }

        public bool HasStreamEnded
        {
            get
            {
                if (_nextByte == -1)
                {
                    _nextByte = _stream.ReadByte();
                    if (_nextByte == -1)
                        return true;
                }
                return false;
            }
        }

        private bool TryPrepareNextByte()
        {
            if (_nextByte == -1)
            {
                _nextByte = _stream.ReadByte();

                if (_nextByte == -1)
                    return false;
            }
            return true;
        }

        #region TryRead integral types

        public bool TryReadByte(out byte value)
        {
            if (TryPrepareNextByte())
            {
                if (_nextByte <= (byte)Format.MaximumPositiveFixInt)
                {
                    value = (byte)(_nextByte & (byte)Format.MaximumPositiveFixInt);
                    _nextByte = -1;
                    return true;
                }

                if (_nextByte == (byte)Format.UInt8)
                {
                    value = ReadByte();
                    _nextByte = -1;
                    return true;
                }
            }

            value = default(byte);
            return false;
        }

        public bool TryReadInt16(out short value)
        {
            if (!TryPrepareNextByte())
            {
                value = default(short);
                return false;
            }

            if (_nextByte <= (byte)Format.MaximumPositiveFixInt)
            {
                value = (short)(_nextByte & (byte)Format.MaximumPositiveFixInt);
                _nextByte = -1;
                return true;
            }

            if ((_nextByte & (byte)Format.MinimumNegativeFixInt) == (byte)Format.MinimumNegativeFixInt)
            {
                value = (sbyte)_nextByte;
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case (byte)Format.UInt8: value = ReadByte(); break;
                case (byte)Format.Int8: value = ReadSByte(); break;
                case (byte)Format.Int16: value = ReadInt16(); break;

                default:
                    value = default(short);
                    return false;
            }

            _nextByte = -1;
            return true;
        }

        public bool TryReadInt32(out int value)
        {
            if (!TryPrepareNextByte())
            {
                value = default(int);
                return false;
            }

            if (_nextByte <= (byte)Format.MaximumPositiveFixInt)
            {
                value = (_nextByte & (byte)Format.MaximumPositiveFixInt);
                _nextByte = -1;
                return true;
            }

            if ((_nextByte & (byte)Format.MinimumNegativeFixInt) == (byte)Format.MinimumNegativeFixInt)
            {
                value = (sbyte)_nextByte;
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case (byte)Format.UInt8: value = ReadByte(); break;
                case (byte)Format.UInt16: value = ReadUInt16(); break;
                case (byte)Format.Int8: value = ReadSByte(); break;
                case (byte)Format.Int16: value = ReadInt16(); break;
                case (byte)Format.Int32: value = ReadInt32(); break;

                default:
                    value = default(int);
                    return false;
            }

            _nextByte = -1;
            return true;
        }

        public bool TryReadInt64(out long value)
        {
            if (!TryPrepareNextByte())
            {
                value = default(long);
                return false;
            }

            if (_nextByte <= (byte)Format.MaximumPositiveFixInt)
            {
                value = (_nextByte & (byte)Format.MaximumPositiveFixInt);
                _nextByte = -1;
                return true;
            }

            if ((_nextByte & (byte)Format.MaximumPositiveFixInt) == (byte)Format.MaximumPositiveFixInt)
            {
                value = (sbyte)_nextByte;
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case (byte)Format.UInt8: value = ReadByte(); break;
                case (byte)Format.UInt16: value = ReadUInt16(); break;
                case (byte)Format.UInt32: value = ReadUInt32(); break;
                case (byte)Format.Int8: value = ReadSByte(); break;
                case (byte)Format.Int16: value = ReadInt16(); break;
                case (byte)Format.Int32: value = ReadInt32(); break;
                case (byte)Format.Int64: value = ReadInt64(); break;

                default:
                    value = default(long);
                    return false;
            }

            _nextByte = -1;
            return true;
        }

        public bool TryReadSByte(out sbyte value)
        {
            if (TryPrepareNextByte())
            {
                if (_nextByte <= (byte)Format.MaximumPositiveFixInt)
                {
                    value = (sbyte)(_nextByte & (byte)Format.MaximumPositiveFixInt);
                    _nextByte = -1;
                    return true;
                }

                if ((_nextByte & (byte)Format.MinimumNegativeFixInt) == (byte)Format.MinimumNegativeFixInt)
                {
                    value = (sbyte)_nextByte;
                    _nextByte = -1;
                    return true;
                }

                if (_nextByte == (byte)Format.Int8)
                {
                    value = ReadSByte();
                    _nextByte = -1;
                    return true;
                }
            }

            value = default(sbyte);
            return false;
        }

        public bool TryReadUInt16(out ushort value)
        {
            if (!TryPrepareNextByte())
            {
                value = default(ushort);
                return false;
            }

            if (_nextByte <= (byte)Format.MaximumPositiveFixInt)
            {
                value = (ushort)(_nextByte & (byte)Format.MaximumPositiveFixInt);
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case (byte)Format.UInt8: value = ReadByte(); break;
                case (byte)Format.UInt16: value = ReadUInt16(); break;

                default:
                    value = default(ushort);
                    return false;
            }

            _nextByte = -1;
            return true;
        }

        public bool TryReadUInt32(out uint value)
        {
            if (!TryPrepareNextByte())
            {
                value = default(uint);
                return false;
            }

            if (_nextByte <= (byte)Format.MaximumPositiveFixInt)
            {
                value = (uint)(_nextByte & (byte)Format.MaximumPositiveFixInt);
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case (byte)Format.UInt8: value = ReadByte(); break;
                case (byte)Format.UInt16: value = ReadUInt16(); break;
                case (byte)Format.UInt32: value = ReadUInt32(); break;

                default:
                    value = default(uint);
                    return false;
            }

            _nextByte = -1;
            return true;
        }

        public bool TryReadUInt64(out ulong value)
        {
            if (!TryPrepareNextByte())
            {
                value = default(ulong);
                return false;
            }

            if (_nextByte <= (byte)Format.MaximumPositiveFixInt)
            {
                value = (ulong)(_nextByte & (byte)Format.MaximumPositiveFixInt);
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case (byte)Format.UInt8: value = ReadByte(); break;
                case (byte)Format.UInt16: value = ReadUInt16(); break;
                case (byte)Format.UInt32: value = ReadUInt32(); break;
                case (byte)Format.UInt64: value = ReadUInt64(); break;

                default:
                    value = default(ulong);
                    return false;
            }

            _nextByte = -1;
            return true;
        }

        #endregion

        public unsafe bool TryReadSingle(out float value)
        {
            if (TryPrepareNextByte())
            {
                if (_nextByte == (byte)Format.Float32)
                {
                    // big-endian 32-bit IEEE 754 floating point
                    var bits = ReadUInt32();
                    value = *(float*)&bits;
                    _nextByte = -1;
                    return true;
                }
            }

            value = default(float);
            return false;
        }

        public unsafe bool TryReadDouble(out double value)
        {
            if (TryPrepareNextByte())
            {
                if (_nextByte == (byte)Format.Float64)
                {
                    // big-endian 64-bit IEEE 754 floating point
                    var bits = ReadUInt64();
                    value = *(double*)&bits;
                    _nextByte = -1;
                    return true;
                }
            }

            value = default(double);
            return false;
        }

        public bool TryReadBoolean(out bool value)
        {
            if (TryPrepareNextByte())
            {
                if (_nextByte == (byte)Format.False)
                {
                    value = false;
                    _nextByte = -1;
                    return true;
                }

                if (_nextByte == (byte)Format.True)
                {
                    value = true;
                    _nextByte = -1;
                    return true;
                }
            }

            value = default(bool);
            return false;
        }

        public bool TryReadArrayLength(out int value)
        {
            if (TryPrepareNextByte())
            {
                uint? length = null;
                if ((_nextByte & 0xF0) == (byte)Format.MinimumFixArray)
                {
                    length = (uint?)(_nextByte & 0x0F);
                }
                else
                {
                    switch (_nextByte)
                    {
                        case (byte)Format.Array16: length = ReadUInt16(); break;
                        case (byte)Format.Array32: length = ReadUInt32(); break;
                    }
                }

                if (length != null)
                {
                    if (length > int.MaxValue)
                        throw new Exception("Array length too large");
                    _nextByte = -1;
                    value = (int)length;
                    return true;
                }
            }

            value = default(int);
            return false;
        }

        public bool TryReadMapLength(out int value)
        {
            if (TryPrepareNextByte())
            {
                uint? length = null;
                if ((_nextByte & 0xF0) == (byte)Format.MinimumFixMap)
                {
                    length = (uint?)(_nextByte & 0x0F);
                }
                else
                {
                    switch (_nextByte)
                    {
                        case (byte)Format.Map16: length = ReadUInt16(); break;
                        case (byte)Format.Map32: length = ReadUInt32(); break;
                    }
                }

                if (length != null)
                {
                    if (length > int.MaxValue)
                        throw new Exception("Array length too large");
                    _nextByte = -1;
                    value = (int)length;
                    return true;
                }
            }

            value = default(int);
            return false;
        }

        public bool TryPeekFormatFamily(out FormatFamily family)
        {
            if (TryPrepareNextByte())
            {
                if ((_nextByte >= 0 && _nextByte <= (byte)Format.MaximumPositiveFixInt) || (_nextByte >= (byte)Format.UInt8 && _nextByte <= (byte)Format.Int64) || (_nextByte >= (byte)Format.MinimumNegativeFixInt && _nextByte <= 0xff))
                {
                    family = FormatFamily.Integer;
                    return true;
                }
                if ((_nextByte >= (byte)Format.MinimumFixMap && _nextByte <= (byte)Format.MaximumFixMap) || _nextByte == (byte)Format.Map16 || _nextByte == (byte)Format.Map32)
                {
                    family = FormatFamily.Map;
                    return true;
                }
                if ((_nextByte >= (byte)Format.MinimumFixArray && _nextByte <= (byte)Format.MaximumFixArray) || _nextByte == (byte)Format.Array16 || _nextByte == (byte)Format.Array32)
                {
                    family = FormatFamily.Array;
                    return true;
                }
                if ((_nextByte >= (byte)Format.MinimumFixedRaw && _nextByte <= (byte)Format.MaximumFixedRaw) || (_nextByte >= (byte)Format.Str8 && _nextByte <= (byte)Format.Str32))
                {
                    family = FormatFamily.String;
                    return true;
                }
                switch (_nextByte)
                {
                    case (byte)Format.Null:
                        family = FormatFamily.Null;
                        return true;
                    case (byte)Format.False:
                    case (byte)Format.True:
                        family = FormatFamily.Boolean;
                        return true;
                    case (byte)Format.Bin8:
                    case (byte)Format.Bin16:
                    case (byte)Format.Bin32:
                        family = FormatFamily.Binary;
                        return true;
                    case (byte)Format.Float32:
                    case (byte)Format.Float64:
                        family = FormatFamily.Float;
                        return true;
                }
            }

            family = default(FormatFamily);
            return false;
        }

        public bool TryPeekFormat(out Format format)
        {
            if (TryPrepareNextByte())
            {
                format = DecodeFormat((byte)_nextByte);
                return format != Format.Unknown;
            }

            format = default(Format);
            return false;
        }

        private static Format DecodeFormat(byte b)
        {
            if (b <= (byte)Format.MaximumPositiveFixInt)
                return Format.PositiveFixInt;
            if (b >= (byte)Format.MinimumFixMap && b <= (byte)Format.MaximumFixMap)
                return Format.FixMap;
            if (b >= (byte)Format.MinimumFixArray && b <= (byte)Format.MaximumFixArray)
                return Format.FixArray;
            if (b >= (byte)Format.MinimumFixStr && b <= (byte)Format.MaximumFixStr)
                return Format.FixStr;
            if (b >= (byte)Format.MinimumNegativeFixInt)
                return Format.NegativeFixInt;

            Format format = (Format)b;
            switch (format)
            {
                case Format.Null:
                case Format.False:
                case Format.True:
                case Format.Bin8:
                case Format.Bin16:
                case Format.Bin32:
                case Format.Ext8:
                case Format.Ext16:
                case Format.Ext32:
                case Format.Float32:
                case Format.Float64:
                case Format.UInt8:
                case Format.UInt16:
                case Format.UInt32:
                case Format.UInt64:
                case Format.Int8:
                case Format.Int16:
                case Format.Int32:
                case Format.Int64:
                case Format.FixExt1:
                case Format.FixExt2:
                case Format.FixExt4:
                case Format.FixExt8:
                case Format.FixExt16:
                case Format.Str8:
                case Format.Str16:
                case Format.Str32:
                case Format.Array16:
                case Format.Array32:
                case Format.Map16:
                case Format.Map32:
                    return format;

                default:
                    return Format.Unknown;
            }
        }

        #region Reading strings

        public bool TryReadString(out string value)
        {
            return TryReadString(out value, Encoding.UTF8);
        }

        public bool TryReadString(out string value, Encoding encoding)
        {
            if (TryPrepareNextByte())
            {
                if (_nextByte == (byte)Format.Null)
                {
                    value = null;
                    _nextByte = -1;
                    return true;
                }

                uint? length = null;
                if ((_nextByte & (byte)Format.MinimumNegativeFixInt) == (byte)Format.MinimumFixedRaw)
                {
                    length = (uint)(_nextByte & 0x1F);
                }
                else
                {
                    switch (_nextByte)
                    {
                        case (byte)Format.Str8: length = ReadByte();  break;
                        case (byte)Format.Str16: length = ReadUInt16(); break;
                        case (byte)Format.Str32: length = ReadUInt32(); break;
                    }
                }

                if (length != null)
                {
                    if (length > int.MaxValue)
                        throw new Exception("String length is too long to read");

                    var bytes = Read((int)length);
                    value = encoding.GetString(bytes);
                    _nextByte = -1;
                    return true;
                }
            }

            value = default(string);
            return false;
        }

        #endregion

        public bool TryReadBinary(out byte[] value)
        {
            if (TryPrepareNextByte())
            {
                if (_nextByte == (byte)Format.Null)
                {
                    value = null;
                    _nextByte = -1;
                    return true;
                }

                uint? length = null;
                switch (_nextByte)
                {
                    case (byte)Format.Bin8: length = ReadByte(); break;
                    case (byte)Format.Bin16: length = ReadUInt16(); break;
                    case (byte)Format.Bin32: length = ReadUInt32(); break;
                }
                if (length != null)
                {
                    if (length > int.MaxValue)
                        throw new Exception("Byte array length is too long to read");
                    value = Read((int)length);
                    _nextByte = -1;
                    return true;
                }
            }

            value = default(byte[]);
            return false;
        }

        private byte[] Read(int length)
        {
            var bytes = new byte[length];
            var pos = 0;
            while (pos != length)
            {
                var read = _stream.Read(bytes, pos, length - pos);
                if (read == 0)
                    throw new IOException("End of stream reached.");
                pos += read;
            }
            return bytes;
        }

        #region Reading values directly (for content after marker byte)

        private byte ReadByte()
        {
            var i = _stream.ReadByte();
            if (i == -1)
                throw new IOException("Unexpected end of stream.");
            return (byte)i;
        }

        private sbyte ReadSByte()
        {
            var i = _stream.ReadByte();
            if (i == -1)
                throw new IOException("Unexpected end of stream.");
            return (sbyte)(byte)i;
        }

        private ushort ReadUInt16()
        {
            var b1 = _stream.ReadByte();
            var b2 = _stream.ReadByte();
            if (b2 == -1)
                throw new IOException("Unexpected end of stream.");
            return (ushort)(b1 << 8 | b2);
        }

        private uint ReadUInt32()
        {
            var b1 = _stream.ReadByte();
            var b2 = _stream.ReadByte();
            var b3 = _stream.ReadByte();
            var b4 = _stream.ReadByte();
            if (b4 == -1)
                throw new IOException("Unexpected end of stream.");
            return (uint)(b1 << 24 | b2 << 16 | b3 << 8 | b4);
        }

        private ulong ReadUInt64()
        {
            var b1 = _stream.ReadByte();
            var b2 = _stream.ReadByte();
            var b3 = _stream.ReadByte();
            var b4 = _stream.ReadByte();
            var b5 = _stream.ReadByte();
            var b6 = _stream.ReadByte();
            var b7 = _stream.ReadByte();
            var b8 = _stream.ReadByte();
            if (b8 == -1)
                throw new IOException("Unexpected end of stream.");
            #pragma warning disable CS0675
            return (ulong)b1 << 56 | (ulong)b2 << 48 | (ulong)b3 << 40 | (ulong)b4 << 32 | (ulong)b5 << 24 | (ulong)b6 << 16 | (ulong)b7 << 8 | (ulong)b8;
            #pragma warning restore CS0675
        }

        private short ReadInt16()
        {
            var b1 = _stream.ReadByte();
            var b2 = _stream.ReadByte();
            if (b2 == -1)
                throw new IOException("Unexpected end of stream.");
            return (short)(b1 << 8 | b2);
        }

        private int ReadInt32()
        {
            var b1 = _stream.ReadByte();
            var b2 = _stream.ReadByte();
            var b3 = _stream.ReadByte();
            var b4 = _stream.ReadByte();
            if (b4 == -1)
                throw new IOException("Unexpected end of stream.");
            return (int)(b1 << 24 | b2 << 16 | b3 << 8 | b4);
        }

        private long ReadInt64()
        {
            var b1 = _stream.ReadByte();
            var b2 = _stream.ReadByte();
            var b3 = _stream.ReadByte();
            var b4 = _stream.ReadByte();
            var b5 = _stream.ReadByte();
            var b6 = _stream.ReadByte();
            var b7 = _stream.ReadByte();
            var b8 = _stream.ReadByte();
            if (b8 == -1)
                throw new IOException("Unexpected end of stream.");
            return (long)b1 << 56 | (long)b2 << 48 | (long)b3 << 40 | (long)b4 << 32 | (long)b5 << 24 | (long)b6 << 16 | (long)b7 << 8 | (long)b8;
        }

        #endregion
    }
}