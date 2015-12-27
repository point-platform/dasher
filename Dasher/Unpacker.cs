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
using System.Text;
using static Dasher.MsgPackConstants;

namespace Dasher
{
    public sealed class Unpacker
    {
        private readonly Stream _stream;
        private int _nextByte = -1;

        public Unpacker(Stream stream)
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
                if ((_nextByte & PosFixIntPrefixBitsMask) == PosFixIntPrefixBits)
                {
                    value = (byte)_nextByte;
                    _nextByte = -1;
                    return true;
                }

                if (_nextByte == UInt8PrefixByte)
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

            if ((_nextByte & PosFixIntPrefixBitsMask) == PosFixIntPrefixBits)
            {
                value = (short)_nextByte;
                _nextByte = -1;
                return true;
            }

            if ((_nextByte & NegFixIntPrefixBitsMask) == NegFixIntPrefixBits)
            {
                value = (sbyte)_nextByte;
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case UInt8PrefixByte: value = ReadByte();  break;
                case Int8PrefixByte:  value = ReadSByte(); break;
                case Int16PrefixByte: value = ReadInt16(); break;

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

            if ((_nextByte & PosFixIntPrefixBitsMask) == PosFixIntPrefixBits)
            {
                value = _nextByte;
                _nextByte = -1;
                return true;
            }

            if ((_nextByte & NegFixIntPrefixBitsMask) == NegFixIntPrefixBits)
            {
                value = (sbyte)_nextByte;
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case UInt8PrefixByte:  value = ReadByte();   break;
                case UInt16PrefixByte: value = ReadUInt16(); break;
                case Int8PrefixByte:   value = ReadSByte();  break;
                case Int16PrefixByte:  value = ReadInt16();  break;
                case Int32PrefixByte:  value = ReadInt32();  break;

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

            if ((_nextByte & PosFixIntPrefixBitsMask) == PosFixIntPrefixBits)
            {
                value = _nextByte;
                _nextByte = -1;
                return true;
            }

            if ((_nextByte & NegFixIntPrefixBitsMask) == NegFixIntPrefixBits)
            {
                value = (sbyte)_nextByte;
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case UInt8PrefixByte:  value = ReadByte();   break;
                case UInt16PrefixByte: value = ReadUInt16(); break;
                case UInt32PrefixByte: value = ReadUInt32(); break;
                case Int8PrefixByte:   value = ReadSByte();  break;
                case Int16PrefixByte:  value = ReadInt16();  break;
                case Int32PrefixByte:  value = ReadInt32();  break;
                case Int64PrefixByte:  value = ReadInt64();  break;

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
                if ((_nextByte & PosFixIntPrefixBitsMask) == PosFixIntPrefixBits)
                {
                    value = (sbyte)_nextByte;
                    _nextByte = -1;
                    return true;
                }

                if ((_nextByte & NegFixIntPrefixBitsMask) == NegFixIntPrefixBits)
                {
                    value = (sbyte)_nextByte;
                    _nextByte = -1;
                    return true;
                }

                if (_nextByte == Int8PrefixByte)
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

            if ((_nextByte & PosFixIntPrefixBitsMask) == PosFixIntPrefixBits)
            {
                value = (ushort)_nextByte;
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case UInt8PrefixByte:  value = ReadByte();   break;
                case UInt16PrefixByte: value = ReadUInt16(); break;

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

            if ((_nextByte & PosFixIntPrefixBitsMask) == PosFixIntPrefixBits)
            {
                value = (uint)_nextByte;
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case UInt8PrefixByte:  value = ReadByte();   break;
                case UInt16PrefixByte: value = ReadUInt16(); break;
                case UInt32PrefixByte: value = ReadUInt32(); break;

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

            if ((_nextByte & PosFixIntPrefixBitsMask) == PosFixIntPrefixBits)
            {
                value = (ulong)_nextByte;
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case UInt8PrefixByte:  value = ReadByte();   break;
                case UInt16PrefixByte: value = ReadUInt16(); break;
                case UInt32PrefixByte: value = ReadUInt32(); break;
                case UInt64PrefixByte: value = ReadUInt64(); break;

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
                if (_nextByte == Float32PrefixByte)
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
                if (_nextByte == Float64PrefixByte)
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
                if (_nextByte == FalseByte)
                {
                    value = false;
                    _nextByte = -1;
                    return true;
                }

                if (_nextByte == TrueByte)
                {
                    value = true;
                    _nextByte = -1;
                    return true;
                }
            }

            value = default(bool);
            return false;
        }

        public bool TryReadNull()
        {
            if (TryPrepareNextByte() && _nextByte == NullByte)
            {
                _nextByte = -1;
                return true;
            }

            return false;
        }

        public bool TryReadArrayLength(out int value)
        {
            if (TryPrepareNextByte())
            {
                uint? length = null;
                if ((_nextByte & FixArrayPrefixBitsMask) == FixArrayPrefixBits)
                {
                    length = (uint)(_nextByte & FixArrayMaxLength);
                }
                else
                {
                    switch (_nextByte)
                    {
                        case Array16PrefixByte: length = ReadUInt16(); break;
                        case Array32PrefixByte: length = ReadUInt32(); break;
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
                if ((_nextByte & FixMapPrefixBitsMask) == FixMapPrefixBits)
                {
                    length = (uint)(_nextByte & FixMapMaxLength);
                }
                else
                {
                    switch (_nextByte)
                    {
                        case Map16PrefixByte: length = ReadUInt16(); break;
                        case Map32PrefixByte: length = ReadUInt32(); break;
                    }
                }

                if (length != null)
                {
                    if (length > int.MaxValue)
                        throw new Exception("Map length too large");
                    _nextByte = -1;
                    value = (int)length;
                    return true;
                }
            }

            value = default(int);
            return false;
        }

        public bool TryReadBinary(out byte[] value)
        {
            if (TryPrepareNextByte())
            {
                if (_nextByte == NullByte)
                {
                    value = null;
                    _nextByte = -1;
                    return true;
                }

                uint? length = null;
                switch (_nextByte)
                {
                    case Bin8PrefixByte:  length = ReadByte();   break;
                    case Bin16PrefixByte: length = ReadUInt16(); break;
                    case Bin32PrefixByte: length = ReadUInt32(); break;
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

        public bool TryPeekFormatFamily(out FormatFamily family)
        {
            if (TryPrepareNextByte())
            {
                if ((_nextByte <= PosFixIntMaxByte) ||
                    (_nextByte >= UInt8PrefixByte && _nextByte <= Int64PrefixByte) ||
                    (_nextByte >= NegFixIntMinByte))
                {
                    family = FormatFamily.Integer;
                    return true;
                }

                if ((_nextByte >= FixMapMinPrefixByte && _nextByte <= FixMapMaxPrefixByte) ||
                     _nextByte == Map16PrefixByte ||
                     _nextByte == Map32PrefixByte)
                {
                    family = FormatFamily.Map;
                    return true;
                }

                if ((_nextByte >= FixArrayMinPrefixByte && _nextByte <= FixArrayMaxPrefixByte) ||
                     _nextByte == Array16PrefixByte ||
                     _nextByte == Array32PrefixByte)
                {
                    family = FormatFamily.Array;
                    return true;
                }

                if ((_nextByte >= FixStrMinPrefixByte && _nextByte <= FixStrMaxPrefixByte) ||
                    (_nextByte >= Str8PrefixByte && _nextByte <= Str32PrefixByte))
                {
                    family = FormatFamily.String;
                    return true;
                }

                switch (_nextByte)
                {
                    case NullByte:
                        family = FormatFamily.Null;
                        return true;
                    case TrueByte:
                    case FalseByte:
                        family = FormatFamily.Boolean;
                        return true;
                    case Bin8PrefixByte:
                    case Bin16PrefixByte:
                    case Bin32PrefixByte:
                        family = FormatFamily.Binary;
                        return true;
                    case Float32PrefixByte:
                    case Float64PrefixByte:
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
            if (b <= PosFixIntMaxByte)
                return Format.PositiveFixInt;
            if (b >= FixMapMinPrefixByte && b <= FixMapMaxPrefixByte)
                return Format.FixMap;
            if (b >= FixArrayMinPrefixByte && b <= FixArrayMaxPrefixByte)
                return Format.FixArray;
            if (b >= FixStrMinPrefixByte && b <= FixStrMaxPrefixByte)
                return Format.FixStr;
            if (b >= NegFixIntMinByte)
                return Format.NegativeFixInt;

            switch (b)
            {
                case NullByte:           return Format.Null;
                case FalseByte:          return Format.False;
                case TrueByte:           return Format.True;
                case Bin8PrefixByte:     return Format.Bin8;
                case Bin16PrefixByte:    return Format.Bin16;
                case Bin32PrefixByte:    return Format.Bin32;
                case Ext8PrefixByte:     return Format.Ext8;
                case Ext16PrefixByte:    return Format.Ext16;
                case Ext32PrefixByte:    return Format.Ext32;
                case Float32PrefixByte:  return Format.Float32;
                case Float64PrefixByte:  return Format.Float64;
                case UInt8PrefixByte:    return Format.UInt8;
                case UInt16PrefixByte:   return Format.UInt16;
                case UInt32PrefixByte:   return Format.UInt32;
                case UInt64PrefixByte:   return Format.UInt64;
                case Int8PrefixByte:     return Format.Int8;
                case Int16PrefixByte:    return Format.Int16;
                case Int32PrefixByte:    return Format.Int32;
                case Int64PrefixByte:    return Format.Int64;
                case FixExt1PrefixByte:  return Format.FixExt1;
                case FixExt2PrefixByte:  return Format.FixExt2;
                case FixExt4PrefixByte:  return Format.FixExt4;
                case FixExt8PrefixByte:  return Format.FixExt8;
                case FixExt16PrefixByte: return Format.FixExt16;
                case Str8PrefixByte:     return Format.Str8;
                case Str16PrefixByte:    return Format.Str16;
                case Str32PrefixByte:    return Format.Str32;
                case Array16PrefixByte:  return Format.Array16;
                case Array32PrefixByte:  return Format.Array32;
                case Map16PrefixByte:    return Format.Map16;
                case Map32PrefixByte:    return Format.Map32;

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
                if (_nextByte == NullByte)
                {
                    value = null;
                    _nextByte = -1;
                    return true;
                }

                uint? length = null;
                if ((_nextByte & FixStrPrefixBitsMask) == FixStrPrefixBits)
                {
                    length = (uint)(_nextByte & FixStrMaxLength);
                }
                else
                {
                    switch (_nextByte)
                    {
                        case Str8PrefixByte:  length = ReadByte();   break;
                        case Str16PrefixByte: length = ReadUInt16(); break;
                        case Str32PrefixByte: length = ReadUInt32(); break;
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