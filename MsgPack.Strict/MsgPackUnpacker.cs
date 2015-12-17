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
                if ((_nextByte & 0x80) == 0)
                {
                    value = (byte)(_nextByte & 0x7F);
                    _nextByte = -1;
                    return true;
                }

                if (_nextByte == 0xCC)
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

            if ((_nextByte & 0x80) == 0)
            {
                value = (short)(_nextByte & 0x7F);
                _nextByte = -1;
                return true;
            }

            if ((_nextByte & 0xE0) == 0xE0)
            {
                value = (sbyte)_nextByte;
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case 0xCC: value = ReadByte(); break;
                case 0xD0: value = ReadSByte(); break;
                case 0xD1: value = ReadInt16(); break;

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

            if ((_nextByte & 0x80) == 0)
            {
                value = _nextByte & 0x7F;
                _nextByte = -1;
                return true;
            }

            if ((_nextByte & 0xE0) == 0xE0)
            {
                value = (sbyte)_nextByte;
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case 0xCC: value = ReadByte(); break;
                case 0xCD: value = ReadUInt16(); break;
                case 0xD0: value = ReadSByte(); break;
                case 0xD1: value = ReadInt16(); break;
                case 0xD2: value = ReadInt32(); break;

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

            if ((_nextByte & 0x80) == 0)
            {
                value = _nextByte & 0x7F;
                _nextByte = -1;
                return true;
            }

            if ((_nextByte & 0xE0) == 0xE0)
            {
                value = (sbyte)_nextByte;
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case 0xCC: value = ReadByte(); break;
                case 0xCD: value = ReadUInt16(); break;
                case 0xCE: value = ReadUInt32(); break;
                case 0xD0: value = ReadSByte(); break;
                case 0xD1: value = ReadInt16(); break;
                case 0xD2: value = ReadInt32(); break;
                case 0xD3: value = ReadInt64(); break;

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
                if ((_nextByte & 0x80) == 0)
                {
                    value = (sbyte)(_nextByte & 0x7F);
                    _nextByte = -1;
                    return true;
                }

                if ((_nextByte & 0xE0) == 0xE0)
                {
                    value = (sbyte)_nextByte;
                    _nextByte = -1;
                    return true;
                }

                if (_nextByte == 0xD0)
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

            if ((_nextByte & 0x80) == 0)
            {
                value = (ushort)(_nextByte & 0x7F);
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case 0xCC: value = ReadByte(); break;
                case 0xCD: value = ReadUInt16(); break;

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

            if ((_nextByte & 0x80) == 0)
            {
                value = (uint)(_nextByte & 0x7F);
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case 0xCC: value = ReadByte(); break;
                case 0xCD: value = ReadUInt16(); break;
                case 0xCE: value = ReadUInt32(); break;

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

            if ((_nextByte & 0x80) == 0)
            {
                value = (ulong)(_nextByte & 0x7F);
                _nextByte = -1;
                return true;
            }

            switch (_nextByte)
            {
                case 0xCC: value = ReadByte(); break;
                case 0xCD: value = ReadUInt16(); break;
                case 0xCE: value = ReadUInt32(); break;
                case 0xCF: value = ReadUInt64(); break;

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
                if (_nextByte == 0xca)
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
                if (_nextByte == 0xcb)
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
                if (_nextByte == 0xc2)
                {
                    value = false;
                    _nextByte = -1;
                    return true;
                }

                if (_nextByte == 0xc3)
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
                if ((_nextByte & 0xF0) == 0x90)
                {
                    length = (uint?)(_nextByte & 0x0F);
                }
                else
                {
                    switch (_nextByte)
                    {
                        case 0xDC: length = ReadUInt16(); break;
                        case 0xDD: length = ReadUInt32(); break;
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
                if ((_nextByte & 0xF0) == 0x80)
                {
                    length = (uint?)(_nextByte & 0x0F);
                }
                else
                {
                    switch (_nextByte)
                    {
                        case 0xDE: length = ReadUInt16(); break;
                        case 0xDF: length = ReadUInt32(); break;
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

        #region Reading strings

        public bool TryReadString(out string value)
        {
            return TryReadString(out value, Encoding.UTF8);
        }

        public bool TryReadString(out string value, Encoding encoding)
        {
            if (TryPrepareNextByte())
            {
                if (_nextByte == 0xc0)
                {
                    value = null;
                    _nextByte = -1;
                    return true;
                }

                uint? length = null;
                if ((_nextByte & 0xE0) == 0xA0)
                {
                    length = (uint)(_nextByte & 0x1F);
                }
                else
                {
                    switch (_nextByte)
                    {
                        case 0xD9: length = ReadByte();  break;
                        case 0xDA: length = ReadUInt16(); break;
                        case 0xDB: length = ReadUInt32(); break;
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