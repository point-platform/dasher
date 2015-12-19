using System;
using System.IO;
using System.Text;

namespace MsgPack.Strict
{
    // TODO compare perf with MsgPack.Cli
    // TODO compare perf if using intermediate buffer
    // TODO compare perf if using unsafe code
    // TODO support nullable values

    public class MsgPackPacker
    {
        private readonly Stream _stream;

        public MsgPackPacker(Stream stream)
        {
            _stream = stream;
        }

        public static MsgPackPacker Create(Stream stream)
        {
            return new MsgPackPacker(stream);
        }

        public MsgPackPacker PackNull()
        {
            _stream.WriteByte(MsgPackCode.NilValue);
            return this;
        }

        public MsgPackPacker PackArrayHeader(int length)
        {
            if (length <= 0x0F)
            {
                _stream.WriteByte((byte)(MsgPackCode.MinimumFixedArray | length));
            }
            else if (length <= 0xFFFF)
            {
                _stream.WriteByte(MsgPackCode.Array16);
                _stream.WriteByte((byte)(length >> 8));
                _stream.WriteByte((byte)length);
            }
            else
            {
                _stream.WriteByte(MsgPackCode.Array32);
                _stream.WriteByte((byte)(length >> 24));
                _stream.WriteByte((byte)(length >> 16));
                _stream.WriteByte((byte)(length >> 8));
                _stream.WriteByte((byte)length);
            }
            return this;
        }

        public MsgPackPacker PackMapHeader(int length)
        {
            if (length <= 0x0F)
            {
                _stream.WriteByte((byte)(MsgPackCode.MinimumFixedMap | length));
            }
            else if (length <= 0xFFFF)
            {
                _stream.WriteByte(MsgPackCode.Map16);
                _stream.WriteByte((byte)(length >> 8));
                _stream.WriteByte((byte)length);
            }
            else
            {
                _stream.WriteByte(MsgPackCode.Map32);
                _stream.WriteByte((byte)(length >> 24));
                _stream.WriteByte((byte)(length >> 16));
                _stream.WriteByte((byte)(length >> 8));
                _stream.WriteByte((byte)length);
            }
            return this;
        }

        public MsgPackPacker Pack(bool value)
        {
            _stream.WriteByte(value ? (byte)MsgPackCode.TrueValue : (byte)MsgPackCode.FalseValue);
            return this;
        }

        public MsgPackPacker Pack(byte[] bytes)
        {
            if (bytes == null)
            {
                PackNull();
                return this;
            }

            if (bytes.Length <= 0xFF)
            {
                _stream.WriteByte(MsgPackCode.Bin8);
                _stream.WriteByte((byte)bytes.Length);
                _stream.Write(bytes, 0, bytes.Length);
            }
            else if (bytes.Length <= 0xFFFF)
            {
                _stream.WriteByte(MsgPackCode.Bin16);
                var l = bytes.Length;
                _stream.WriteByte((byte)(l >> 8));
                _stream.WriteByte((byte)l);
                _stream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                _stream.WriteByte(MsgPackCode.Bin32);
                var l = bytes.Length;
                _stream.WriteByte((byte)(l >> 24));
                _stream.WriteByte((byte)(l >> 16));
                _stream.WriteByte((byte)(l >> 8));
                _stream.WriteByte((byte)l);
                _stream.Write(bytes, 0, bytes.Length);
            }
            return this;
        }

        public MsgPackPacker Pack(string value)
        {
            return Pack(value, Encoding.UTF8);
        }

        public MsgPackPacker Pack(string value, Encoding encoding)
        {
            if (value == null)
            {
                PackNull();
                return this;
            }

            var bytes = encoding.GetBytes(value);

            if (bytes.Length <= 0x1F)
            {
                _stream.WriteByte((byte)(MsgPackCode.MinimumFixedRaw | bytes.Length));
                _stream.Write(bytes, 0, bytes.Length);
            }
            else if (bytes.Length <= 0xFF)
            {
                _stream.WriteByte(MsgPackCode.Str8);
                _stream.WriteByte((byte)bytes.Length);
                _stream.Write(bytes, 0, bytes.Length);
            }
            else if (bytes.Length <= 0xFFFF)
            {
                _stream.WriteByte(MsgPackCode.Raw16);
                var l = bytes.Length;
                _stream.WriteByte((byte)(l >> 8));
                _stream.WriteByte((byte)l);
                _stream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                _stream.WriteByte(MsgPackCode.Raw32);
                var l = bytes.Length;
                _stream.WriteByte((byte)(l >> 24));
                _stream.WriteByte((byte)(l >> 16));
                _stream.WriteByte((byte)(l >> 8));
                _stream.WriteByte((byte)l);
                _stream.Write(bytes, 0, bytes.Length);
            }
            return this;
        }

        public MsgPackPacker Pack(float value)
        {
            _stream.WriteByte(MsgPackCode.Real32);
            // TODO this is a terrible, but probably correct, hack that technically could be broken by future releases of .NET, though that seems unlikely
            var i = value.GetHashCode();
            _stream.WriteByte((byte)(i >> 24));
            _stream.WriteByte((byte)(i >> 16));
            _stream.WriteByte((byte)(i >> 8));
            _stream.WriteByte((byte)i);
            return this;
        }

        public MsgPackPacker Pack(double value)
        {
            _stream.WriteByte(MsgPackCode.Real64);
            var l = BitConverter.DoubleToInt64Bits(value);
            _stream.WriteByte((byte)(l >> 56));
            _stream.WriteByte((byte)(l >> 48));
            _stream.WriteByte((byte)(l >> 40));
            _stream.WriteByte((byte)(l >> 32));
            _stream.WriteByte((byte)(l >> 24));
            _stream.WriteByte((byte)(l >> 16));
            _stream.WriteByte((byte)(l >> 8));
            _stream.WriteByte((byte)l);
            return this;
        }

        public MsgPackPacker Pack(byte value)
        {
            if (value <= 0x7F)
            {
                // positive fixnum (7-bit positive number)
                _stream.WriteByte(value);
            }
            else
            {
                _stream.WriteByte(MsgPackCode.UnsignedInt8);
                _stream.WriteByte(value);
            }
            return this;
        }

        public MsgPackPacker Pack(sbyte value)
        {
            if (value >= 0x00)
            {
                // positive fixnum (7-bit positive number)
                _stream.WriteByte((byte)value);
            }
            else if (value >= -32 /*0b_1110_000*/ && value < 0x00)
            {
                // negative fixnum (5-bit negative number)
                _stream.WriteByte((byte)(value | 0xE0));
            }
            else
            {
                _stream.WriteByte(MsgPackCode.SignedInt8);
                _stream.WriteByte((byte)value);
            }
            return this;
        }

        public MsgPackPacker Pack(ushort value)
        {
            if (value <= 0x7F)
            {
                // positive fixnum (7-bit positive number)
                _stream.WriteByte((byte)value);
            }
            else if (value <= byte.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.UnsignedInt8);
                _stream.WriteByte((byte)value);
            }
            else // if (value <= ushort.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.UnsignedInt16);
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            return this;
        }

        public MsgPackPacker Pack(short value)
        {
            if (value >= 0x00 && value <= sbyte.MaxValue)
            {
                // positive fixnum (7-bit positive number)
                _stream.WriteByte((byte)value);
            }
            else if (value >= -32 /*0b_1110_000*/ && value < 0x00)
            {
                // negative fixnum (5-bit negative number)
                _stream.WriteByte((byte)(value | 0xE0));
            }
            else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.SignedInt8);
                _stream.WriteByte((byte)value);
            }
            else // if (value >= short.MinValue && value <= short.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.SignedInt16);
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            return this;
        }

        public MsgPackPacker Pack(uint value)
        {
            if (value <= 0x7F)
            {
                // positive fixnum (7-bit positive number)
                _stream.WriteByte((byte)value);
            }
            else if (value <= byte.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.UnsignedInt8);
                _stream.WriteByte((byte)value);
            }
            else if (value <= ushort.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.UnsignedInt16);
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else // if (value <= uint.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.UnsignedInt32);
                _stream.WriteByte((byte)(value >> 24));
                _stream.WriteByte((byte)(value >> 16));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            return this;
        }

        public MsgPackPacker Pack(int value)
        {
            if (value >= 0x00 && value <= sbyte.MaxValue)
            {
                // positive fixnum (7-bit positive number)
                _stream.WriteByte((byte)value);
            }
            else if (value >= -32 /*0b_1110_000*/ && value < 0x00)
            {
                // negative fixnum (5-bit negative number)
                _stream.WriteByte((byte)(value | 0xE0));
            }
            else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.SignedInt8);
                _stream.WriteByte((byte)value);
            }
            else if (value >= short.MinValue && value <= short.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.SignedInt16);
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else // if (value >= int.MinValue && value <= int.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.SignedInt32);
                _stream.WriteByte((byte)(value >> 24));
                _stream.WriteByte((byte)(value >> 16));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            return this;
        }

        public MsgPackPacker Pack(long value)
        {
            if (value >= 0x00 && value <= sbyte.MaxValue)
            {
                // positive fixnum (7-bit positive number)
                _stream.WriteByte((byte)value);
            }
            else if (value >= -32 /*0b_1110_000*/ && value < 0x00)
            {
                // negative fixnum (5-bit negative number)
                _stream.WriteByte((byte)(value | 0xE0));
            }
            else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.SignedInt8);
                _stream.WriteByte((byte)value);
            }
            else if (value >= short.MinValue && value <= short.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.SignedInt16);
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else if (value >= int.MinValue && value <= int.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.SignedInt32);
                _stream.WriteByte((byte)(value >> 24));
                _stream.WriteByte((byte)(value >> 16));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else
            {
                _stream.WriteByte(MsgPackCode.SignedInt64);
                _stream.WriteByte((byte)(value >> 56));
                _stream.WriteByte((byte)(value >> 48));
                _stream.WriteByte((byte)(value >> 40));
                _stream.WriteByte((byte)(value >> 32));
                _stream.WriteByte((byte)(value >> 24));
                _stream.WriteByte((byte)(value >> 16));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            return this;
        }

        public MsgPackPacker Pack(ulong value)
        {
            if (value <= 0x7F)
            {
                // positive fixnum (7-bit positive number)
                _stream.WriteByte((byte)value);
            }
            else if (value <= byte.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.UnsignedInt8);
                _stream.WriteByte((byte)value);
            }
            else if (value <= ushort.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.UnsignedInt16);
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else if (value <= uint.MaxValue)
            {
                _stream.WriteByte(MsgPackCode.UnsignedInt32);
                _stream.WriteByte((byte)(value >> 24));
                _stream.WriteByte((byte)(value >> 16));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else
            {
                _stream.WriteByte(MsgPackCode.UnsignedInt64);
                _stream.WriteByte((byte)(value >> 56));
                _stream.WriteByte((byte)(value >> 48));
                _stream.WriteByte((byte)(value >> 40));
                _stream.WriteByte((byte)(value >> 32));
                _stream.WriteByte((byte)(value >> 24));
                _stream.WriteByte((byte)(value >> 16));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            return this;
        }
    }
}