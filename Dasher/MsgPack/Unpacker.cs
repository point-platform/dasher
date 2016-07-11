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
using System.Diagnostics;
using System.IO;
using System.Text;
using static Dasher.MsgPackConstants;

namespace Dasher
{
    public sealed
#if UNSAFE
        unsafe
#endif
        class Unpacker
    {
        private static Exception EOF() => new IOException("End of stream reached.");

        private readonly Stream _stream;
        private readonly byte[] _buffer = new byte[8];

        private int _nextByte = -1;

        public Unpacker(Stream stream)
        {
            _stream = stream;
        }

        public bool HasStreamEnded
        {
            get
            {
                if (_nextByte != -1)
                    return false;

                _nextByte = _stream.ReadByte();
                return _nextByte == -1;
            }
        }

        private void PrepareNextByte()
        {
            if (_nextByte != -1)
                return;

            _nextByte = _stream.ReadByte();

            if (_nextByte == -1)
                throw EOF();
        }

        #region TryRead integral types

        public bool TryReadByte(out byte value)
        {
            PrepareNextByte();

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

            value = default(byte);
            return false;
        }

        public bool TryReadInt16(out short value)
        {
            PrepareNextByte();

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
            PrepareNextByte();

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
            PrepareNextByte();

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
            PrepareNextByte();

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

            value = default(sbyte);
            return false;
        }

        public bool TryReadUInt16(out ushort value)
        {
            PrepareNextByte();

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
            PrepareNextByte();

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
            PrepareNextByte();

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

        public bool TryReadSingle(out float value)
        {
            PrepareNextByte();

            if (_nextByte == Float32PrefixByte)
            {
                // big-endian 32-bit IEEE 754 floating point
#if UNSAFE
                var bits = ReadUInt32();
                value = *(float*)&bits;
#else
                Read(4, _buffer);
                value = BitConverter.ToSingle(_buffer, 0);
#endif
                _nextByte = -1;
                return true;
            }

            value = default(float);
            return false;
        }

        public bool TryReadDouble(out double value)
        {
            PrepareNextByte();

            if (_nextByte == Float64PrefixByte)
            {
                // big-endian 64-bit IEEE 754 floating point
#if UNSAFE
                var bits = ReadUInt64();
                value = *(double*)&bits;
#else
                Read(8, _buffer);
                value = BitConverter.ToDouble(_buffer, 0);
#endif
                _nextByte = -1;
                return true;
            }

            value = default(double);
            return false;
        }

        public bool TryReadBoolean(out bool value)
        {
            PrepareNextByte();

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

            value = default(bool);
            return false;
        }

        public bool TryReadNull()
        {
            PrepareNextByte();

            if (_nextByte != NullByte)
                return false;

            _nextByte = -1;
            return true;
        }

        public bool TryReadArrayLength(out int value)
        {
            PrepareNextByte();

            uint? length = null;
            if ((_nextByte & FixArrayPrefixBitsMask) == FixArrayPrefixBits)
            {
                length = (uint)(_nextByte & FixArrayMaxLength);
            }
            else
            {
                switch (_nextByte)
                {
                    case Array16PrefixByte:
                        length = ReadUInt16();
                        break;
                    case Array32PrefixByte:
                        length = ReadUInt32();
                        break;
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

            value = default(int);
            return false;
        }

        public bool TryReadMapLength(out int value)
        {
            PrepareNextByte();

            uint? length = null;
            if ((_nextByte & FixMapPrefixBitsMask) == FixMapPrefixBits)
            {
                length = (uint)(_nextByte & FixMapMaxLength);
            }
            else
            {
                switch (_nextByte)
                {
                    case Map16PrefixByte:
                        length = ReadUInt16();
                        break;
                    case Map32PrefixByte:
                        length = ReadUInt32();
                        break;
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

            value = default(int);
            return false;
        }

        public bool TryReadBinary(out byte[] value)
        {
            PrepareNextByte();

            if (_nextByte == NullByte)
            {
                value = null;
                _nextByte = -1;
                return true;
            }

            uint? length = null;
            switch (_nextByte)
            {
                case Bin8PrefixByte:
                    length = ReadByte();
                    break;
                case Bin16PrefixByte:
                    length = ReadUInt16();
                    break;
                case Bin32PrefixByte:
                    length = ReadUInt32();
                    break;
            }

            if (length != null)
            {
                if (length > int.MaxValue)
                    throw new Exception("Byte array length is too long to read");
                _nextByte = -1;
                value = new byte[(int)length];
                Read((int)length, value);
                return true;
            }

            value = default(byte[]);
            return false;
        }

        public bool TryPeekFormatFamily(out FormatFamily family)
        {
            PrepareNextByte();

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

            family = default(FormatFamily);
            return false;
        }

        public bool TryPeekFormat(out Format format)
        {
            PrepareNextByte();

            format = DecodeFormat((byte)_nextByte);
            return format != Format.Unknown;
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

        #region Skip value

        public void SkipValue()
        {
            PrepareNextByte();

            var b = (byte)_nextByte;

            // TODO can arrange these comparisons to take advantage of mutual information
            if (b <= PosFixIntMaxByte || b >= NegFixIntMinByte)
            {
                _nextByte = -1;
                return;
            }

            if (b >= FixMapMinPrefixByte && b <= FixMapMaxPrefixByte)
            {
                _nextByte = -1;
                SkipValues(unchecked((uint)((b ^ FixMapPrefixBits)<<1)));
                return;
            }

            if (b >= FixArrayMinPrefixByte && b <= FixArrayMaxPrefixByte)
            {
                _nextByte = -1;
                SkipValues(unchecked((uint)(b ^ FixArrayPrefixBits)));
                return;
            }

            if (b >= FixStrMinPrefixByte && b <= FixStrMaxPrefixByte)
            {
                _nextByte = -1;
                SkipBytes(unchecked((uint)(b ^ FixStrPrefixBits)));
                return;
            }

            switch (b)
            {
                case NullByte:           _nextByte = -1; return;
                case FalseByte:          _nextByte = -1; return;
                case TrueByte:           _nextByte = -1; return;
                case Bin8PrefixByte:     _nextByte = -1; SkipBytes(ReadByte());   return;
                case Bin16PrefixByte:    _nextByte = -1; SkipBytes(ReadUInt16()); return;
                case Bin32PrefixByte:    _nextByte = -1; SkipBytes(ReadUInt32()); return;
                case Ext8PrefixByte:     _nextByte = -1; SkipBytes(ReadByte());   return;
                case Ext16PrefixByte:    _nextByte = -1; SkipBytes(ReadUInt16()); return;
                case Ext32PrefixByte:    _nextByte = -1; SkipBytes(ReadUInt32()); return;
                case Float32PrefixByte:  SkipBytes(5); return;
                case Float64PrefixByte:  SkipBytes(9); return;
                case UInt8PrefixByte:    SkipBytes(2); return;
                case UInt16PrefixByte:   SkipBytes(3); return;
                case UInt32PrefixByte:   SkipBytes(5); return;
                case UInt64PrefixByte:   SkipBytes(9); return;
                case Int8PrefixByte:     SkipBytes(2); return;
                case Int16PrefixByte:    SkipBytes(3); return;
                case Int32PrefixByte:    SkipBytes(5); return;
                case Int64PrefixByte:    SkipBytes(9); return;
                case FixExt1PrefixByte:  SkipBytes(3); return;
                case FixExt2PrefixByte:  SkipBytes(4); return;
                case FixExt4PrefixByte:  SkipBytes(6); return;
                case FixExt8PrefixByte:  SkipBytes(10); return;
                case FixExt16PrefixByte: SkipBytes(18); return;
                case Str8PrefixByte:     _nextByte = -1; SkipBytes(ReadByte());    return;
                case Str16PrefixByte:    _nextByte = -1; SkipBytes(ReadUInt16());  return;
                case Str32PrefixByte:    _nextByte = -1; SkipBytes(ReadUInt32());  return;
                case Array16PrefixByte:  _nextByte = -1; SkipValues(ReadUInt16()); return;
                case Array32PrefixByte:  _nextByte = -1; SkipValues(ReadUInt32()); return;
                case Map16PrefixByte:    _nextByte = -1; SkipValues((uint)ReadUInt16()<<1); return;
                case Map32PrefixByte:    _nextByte = -1; SkipValues((ulong)ReadUInt32()<<1); return;

                default:
                    throw new Exception("Cannot decode type to skip");
            }
        }

        private void SkipValues(uint count)
        {
            for (var i = 0; i < count; i++)
                SkipValue();
        }

        private void SkipValues(ulong count)
        {
            for (var i = 0UL; i < count; i++)
                SkipValue();
        }

        private void SkipBytes(uint count)
        {
            switch (count)
            {
                case 0:
                    return;
                case 1:
                    if (_nextByte == -1)
                        PrepareNextByte();
                    _nextByte = -1;
                    return;
            }

            if (_nextByte != -1)
            {
                count--;
                _nextByte = -1;
            }

            Debug.Assert(count > 0);

            if (_stream.CanSeek)
            {
                var startPos = _stream.Position;
                var endPos = _stream.Seek(count, SeekOrigin.Current);
                if (endPos - startPos != count)
                    throw EOF();
            }
            else
            {
                // TODO do this without allocating a byte[]
                var length = checked((int)count);
                Read(length, new byte[length]);
            }
        }

        #endregion

        #region Reading strings

        public bool TryReadString(out string value)
        {
            return TryReadString(out value, Encoding.UTF8);
        }

        public bool TryReadString(out string value, Encoding encoding)
        {
            PrepareNextByte();

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
                    case Str8PrefixByte:
                        length = ReadByte();
                        break;
                    case Str16PrefixByte:
                        length = ReadUInt16();
                        break;
                    case Str32PrefixByte:
                        length = ReadUInt32();
                        break;
                }
            }

            if (length != null)
            {
                if (length > int.MaxValue)
                    throw new Exception("String length is too long to read");

                _nextByte = -1;
                var bytes = new byte[(int)length];
                Read((int)length, bytes);
                value = encoding.GetString(bytes);
                return true;
            }

            value = default(string);
            return false;
        }

        #endregion

        private void Read(int length, byte[] bytes)
        {
            var pos = 0;
            while (pos != length)
            {
                var read = _stream.Read(bytes, pos, length - pos);
                if (read == 0)
                    throw EOF();
                pos += read;
            }
        }

        #region Reading values directly (for content after marker byte)

        private byte ReadByte()
        {
            var i = _stream.ReadByte();
            if (i == -1)
                throw EOF();
            return (byte)i;
        }

        private sbyte ReadSByte()
        {
            var i = _stream.ReadByte();
            if (i == -1)
                throw EOF();
            return (sbyte)(byte)i;
        }

        private ushort ReadUInt16()
        {
            Read(2, _buffer);
#if UNSAFE
            fixed (byte* p = _buffer)
                return (ushort)(*p << 8 | *(p + 1));
#else
            return (ushort)(_buffer[0] << 8 | _buffer[1]);
#endif
        }

        private uint ReadUInt32()
        {
            Read(4, _buffer);
#if UNSAFE
            fixed (byte* b = _buffer)
            {
                var p = b;
                return (uint)(*p++ << 24 |
                              *p++ << 16 |
                              *p++ << 8 |
                              *p);
            }
#else
            return (uint)(_buffer[0] << 24 |
                          _buffer[1] << 16 |
                          _buffer[2] << 8 |
                          _buffer[3]);
#endif
        }

        private ulong ReadUInt64()
        {
            Read(8, _buffer);
#if UNSAFE
            fixed (byte* b = _buffer)
            {
                var p = b;
                return (ulong)*p++ << 56 |
                       (ulong)*p++ << 48 |
                       (ulong)*p++ << 40 |
                       (ulong)*p++ << 32 |
                       (ulong)*p++ << 24 |
                       (ulong)*p++ << 16 |
                       (ulong)*p++ << 8 |
                       (ulong)*p;
            }
#else
            return (ulong)_buffer[0] << 56 |
                   (ulong)_buffer[1] << 48 |
                   (ulong)_buffer[2] << 40 |
                   (ulong)_buffer[3] << 32 |
                   (ulong)_buffer[4] << 24 |
                   (ulong)_buffer[5] << 16 |
                   (ulong)_buffer[6] <<  8 |
                   (ulong)_buffer[7];
#endif
        }

        private short ReadInt16()
        {
            Read(2, _buffer);
#if UNSAFE
            fixed (byte* p = _buffer)
                return (short)(*p << 8 | *(p + 1));
#else
            return (short)(_buffer[0] << 8 | _buffer[1]);
#endif
        }

        private int ReadInt32()
        {
            Read(4, _buffer);
#if UNSAFE
            fixed (byte* b = _buffer)
            {
                var p = b;
                return *p++ << 24 |
                       *p++ << 16 |
                       *p++ << 8 |
                       *p;
            }
#else
            return _buffer[0] << 24 |
                   _buffer[1] << 16 |
                   _buffer[2] << 8 |
                   _buffer[3];
#endif
        }

        private long ReadInt64()
        {
            Read(8, _buffer);

#if UNSAFE
            fixed (byte* b = _buffer)
            {
                var p = b;
                return (long)*p++ << 56 |
                       (long)*p++ << 48 |
                       (long)*p++ << 40 |
                       (long)*p++ << 32 |
                       (long)*p++ << 24 |
                       (long)*p++ << 16 |
                       (long)*p++ << 8 |
                       (long)*p;
            }
#else
            return (long)_buffer[0] << 56 |
                   (long)_buffer[1] << 48 |
                   (long)_buffer[2] << 40 |
                   (long)_buffer[3] << 32 |
                   (long)_buffer[4] << 24 |
                   (long)_buffer[5] << 16 |
                   (long)_buffer[6] << 8 |
                   (long)_buffer[7];
#endif
        }

        #endregion
    }
}
