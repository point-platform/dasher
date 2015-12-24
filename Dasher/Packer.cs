using System;
using System.IO;
using System.Text;
using static Dasher.MsgPackConstants;

namespace Dasher
{
    // TODO support nullable values

    public sealed class Packer
    {
        private readonly Stream _stream;

        public Packer(Stream stream)
        {
            _stream = stream;
        }

        public void PackNull()
        {
            _stream.WriteByte(NullByte);
        }

        public void PackArrayHeader(uint length)
        {
            if (length <= FixArrayMaxLength)
            {
                _stream.WriteByte((byte)(FixArrayPrefixBits | length));
            }
            else if (length <= ushort.MaxValue)
            {
                _stream.WriteByte(Array16PrefixByte);
                _stream.WriteByte((byte)(length >> 8));
                _stream.WriteByte((byte)length);
            }
            else
            {
                _stream.WriteByte(Array32PrefixByte);
                _stream.WriteByte((byte)(length >> 24));
                _stream.WriteByte((byte)(length >> 16));
                _stream.WriteByte((byte)(length >> 8));
                _stream.WriteByte((byte)length);
            }
        }

        public void PackMapHeader(uint length)
        {
            if (length <= FixMapMaxLength)
            {
                _stream.WriteByte((byte)(FixMapPrefixBits | length));
            }
            else if (length <= ushort.MaxValue)
            {
                _stream.WriteByte(Map16PrefixByte);
                _stream.WriteByte((byte)(length >> 8));
                _stream.WriteByte((byte)length);
            }
            else
            {
                _stream.WriteByte(Map32PrefixByte);
                _stream.WriteByte((byte)(length >> 24));
                _stream.WriteByte((byte)(length >> 16));
                _stream.WriteByte((byte)(length >> 8));
                _stream.WriteByte((byte)length);
            }
        }

        public void Pack(bool value)
        {
            _stream.WriteByte(value ? TrueByte : FalseByte);
        }

        public void Pack(byte[] bytes)
        {
            if (bytes == null)
            {
                PackNull();
                return;
            }

            if (bytes.Length <= byte.MaxValue)
            {
                _stream.WriteByte(Bin8PrefixByte);
                _stream.WriteByte((byte)bytes.Length);
                _stream.Write(bytes, 0, bytes.Length);
            }
            else if (bytes.Length <= ushort.MaxValue)
            {
                _stream.WriteByte(Bin16PrefixByte);
                var l = bytes.Length;
                _stream.WriteByte((byte)(l >> 8));
                _stream.WriteByte((byte)l);
                _stream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                _stream.WriteByte(Bin32PrefixByte);
                var l = bytes.Length;
                _stream.WriteByte((byte)(l >> 24));
                _stream.WriteByte((byte)(l >> 16));
                _stream.WriteByte((byte)(l >> 8));
                _stream.WriteByte((byte)l);
                _stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void Pack(string value)
        {
            Pack(value, Encoding.UTF8);
        }

        public void Pack(string value, Encoding encoding)
        {
            if (value == null)
            {
                PackNull();
                return;
            }

            var bytes = encoding.GetBytes(value);

            if (bytes.Length <= FixStrMaxLength)
            {
                _stream.WriteByte((byte)(FixStrPrefixBits | bytes.Length));
                _stream.Write(bytes, 0, bytes.Length);
            }
            else if (bytes.Length <= byte.MaxValue)
            {
                _stream.WriteByte(Str8PrefixByte);
                _stream.WriteByte((byte)bytes.Length);
                _stream.Write(bytes, 0, bytes.Length);
            }
            else if (bytes.Length <= ushort.MaxValue)
            {
                _stream.WriteByte(Str16PrefixByte);
                var l = bytes.Length;
                _stream.WriteByte((byte)(l >> 8));
                _stream.WriteByte((byte)l);
                _stream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                _stream.WriteByte(Str32PrefixByte);
                var l = bytes.Length;
                _stream.WriteByte((byte)(l >> 24));
                _stream.WriteByte((byte)(l >> 16));
                _stream.WriteByte((byte)(l >> 8));
                _stream.WriteByte((byte)l);
                _stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void Pack(float value)
        {
            _stream.WriteByte(Float32PrefixByte);
            // TODO this is a terrible, but probably correct, hack that technically could be broken by future releases of .NET, though that seems unlikely
            var i = value.GetHashCode();
            _stream.WriteByte((byte)(i >> 24));
            _stream.WriteByte((byte)(i >> 16));
            _stream.WriteByte((byte)(i >> 8));
            _stream.WriteByte((byte)i);
        }

        public void Pack(double value)
        {
            _stream.WriteByte(Float64PrefixByte);
            var l = BitConverter.DoubleToInt64Bits(value);
            _stream.WriteByte((byte)(l >> 56));
            _stream.WriteByte((byte)(l >> 48));
            _stream.WriteByte((byte)(l >> 40));
            _stream.WriteByte((byte)(l >> 32));
            _stream.WriteByte((byte)(l >> 24));
            _stream.WriteByte((byte)(l >> 16));
            _stream.WriteByte((byte)(l >> 8));
            _stream.WriteByte((byte)l);
        }

        public void Pack(byte value)
        {
            if (value <= PosFixIntMaxValue)
            {
                _stream.WriteByte(value);
            }
            else
            {
                _stream.WriteByte(UInt8PrefixByte);
                _stream.WriteByte(value);
            }
        }

        public void Pack(sbyte value)
        {
            if (value >= 0)
            {
                _stream.WriteByte((byte)value);
            }
            else if (value >= NegFixIntMinValue)
            {
                _stream.WriteByte((byte)value);
            }
            else
            {
                _stream.WriteByte(Int8PrefixByte);
                _stream.WriteByte((byte)value);
            }
        }

        public void Pack(ushort value)
        {
            if (value <= PosFixIntMaxValue)
            {
                _stream.WriteByte((byte)value);
            }
            else if (value <= byte.MaxValue)
            {
                _stream.WriteByte(UInt8PrefixByte);
                _stream.WriteByte((byte)value);
            }
            else
            {
                _stream.WriteByte(UInt16PrefixByte);
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
        }

        public void Pack(short value)
        {
            if (value >= 0 && value <= PosFixIntMaxValue)
            {
                _stream.WriteByte((byte)value);
            }
            else if (value >= NegFixIntMinValue && value < 0)
            {
                _stream.WriteByte((byte)value);
            }
            else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
            {
                _stream.WriteByte(Int8PrefixByte);
                _stream.WriteByte((byte)value);
            }
            else
            {
                _stream.WriteByte(Int16PrefixByte);
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
        }

        public void Pack(uint value)
        {
            if (value <= PosFixIntMaxValue)
            {
                _stream.WriteByte((byte)value);
            }
            else if (value <= byte.MaxValue)
            {
                _stream.WriteByte(UInt8PrefixByte);
                _stream.WriteByte((byte)value);
            }
            else if (value <= ushort.MaxValue)
            {
                _stream.WriteByte(UInt16PrefixByte);
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else
            {
                _stream.WriteByte(UInt32PrefixByte);
                _stream.WriteByte((byte)(value >> 24));
                _stream.WriteByte((byte)(value >> 16));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
        }

        public void Pack(int value)
        {
            if (value >= 0 && value <= PosFixIntMaxValue)
            {
                _stream.WriteByte((byte)value);
            }
            else if (value >= NegFixIntMinValue && value < 0)
            {
                _stream.WriteByte((byte)value);
            }
            else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
            {
                _stream.WriteByte(Int8PrefixByte);
                _stream.WriteByte((byte)value);
            }
            else if (value >= short.MinValue && value <= short.MaxValue)
            {
                _stream.WriteByte(Int16PrefixByte);
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else
            {
                _stream.WriteByte(Int32PrefixByte);
                _stream.WriteByte((byte)(value >> 24));
                _stream.WriteByte((byte)(value >> 16));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
        }

        public void Pack(long value)
        {
            if (value >= 0 && value <= PosFixIntMaxValue)
            {
                _stream.WriteByte((byte)value);
            }
            else if (value >= NegFixIntMinValue && value < 0)
            {
                _stream.WriteByte((byte)value);
            }
            else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
            {
                _stream.WriteByte(Int8PrefixByte);
                _stream.WriteByte((byte)value);
            }
            else if (value >= short.MinValue && value <= short.MaxValue)
            {
                _stream.WriteByte(Int16PrefixByte);
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else if (value >= int.MinValue && value <= int.MaxValue)
            {
                _stream.WriteByte(Int32PrefixByte);
                _stream.WriteByte((byte)(value >> 24));
                _stream.WriteByte((byte)(value >> 16));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else
            {
                _stream.WriteByte(Int64PrefixByte);
                _stream.WriteByte((byte)(value >> 56));
                _stream.WriteByte((byte)(value >> 48));
                _stream.WriteByte((byte)(value >> 40));
                _stream.WriteByte((byte)(value >> 32));
                _stream.WriteByte((byte)(value >> 24));
                _stream.WriteByte((byte)(value >> 16));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
        }

        public void Pack(ulong value)
        {
            if (value <= PosFixIntMaxValue)
            {
                _stream.WriteByte((byte)value);
            }
            else if (value <= byte.MaxValue)
            {
                _stream.WriteByte(UInt8PrefixByte);
                _stream.WriteByte((byte)value);
            }
            else if (value <= ushort.MaxValue)
            {
                _stream.WriteByte(UInt16PrefixByte);
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else if (value <= uint.MaxValue)
            {
                _stream.WriteByte(UInt32PrefixByte);
                _stream.WriteByte((byte)(value >> 24));
                _stream.WriteByte((byte)(value >> 16));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else
            {
                _stream.WriteByte(UInt64PrefixByte);
                _stream.WriteByte((byte)(value >> 56));
                _stream.WriteByte((byte)(value >> 48));
                _stream.WriteByte((byte)(value >> 40));
                _stream.WriteByte((byte)(value >> 32));
                _stream.WriteByte((byte)(value >> 24));
                _stream.WriteByte((byte)(value >> 16));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
        }
    }
}