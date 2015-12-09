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

        public void PackNull()
        {
            _stream.WriteByte(0xc0);
        }

        public void PackArrayHeader(uint length)
        {
            if (length <= 0x0F)
            {
                _stream.WriteByte((byte)(0x90 | length));
            }
            else if (length <= 0xFFFF)
            {
                _stream.WriteByte(0xDC);
                _stream.WriteByte((byte)(length >> 8));
                _stream.WriteByte((byte)length);
            }
            else
            {
                _stream.WriteByte(0xDD);
                _stream.WriteByte((byte)(length >> 24));
                _stream.WriteByte((byte)(length >> 16));
                _stream.WriteByte((byte)(length >> 8));
                _stream.WriteByte((byte)length);
            }
        }

        public void PackMapHeader(uint length)
        {
            if (length <= 0x0F)
            {
                _stream.WriteByte((byte)(0x80 | length));
            }
            else if (length <= 0xFFFF)
            {
                _stream.WriteByte(0xDE);
                _stream.WriteByte((byte)(length >> 8));
                _stream.WriteByte((byte)length);
            }
            else
            {
                _stream.WriteByte(0xDF);
                _stream.WriteByte((byte)(length >> 24));
                _stream.WriteByte((byte)(length >> 16));
                _stream.WriteByte((byte)(length >> 8));
                _stream.WriteByte((byte)length);
            }
        }

        public void Pack(bool value)
        {
            _stream.WriteByte(value ? (byte)0xC3 : (byte)0xC2);
        }

        public void Pack(byte[] bytes)
        {
            if (bytes == null)
            {
                PackNull();
                return;
            }

            if (bytes.Length <= 0xFF)
            {
                _stream.WriteByte(0xC4);
                _stream.WriteByte((byte)bytes.Length);
                _stream.Write(bytes, 0, bytes.Length);
            }
            else if (bytes.Length <= 0xFFFF)
            {
                _stream.WriteByte(0xC5);
                var l = bytes.Length;
                _stream.WriteByte((byte)(l >> 8));
                _stream.WriteByte((byte)l);
                _stream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                _stream.WriteByte(0xC6);
                var l = bytes.Length;
                _stream.WriteByte((byte)(l >> 24));
                _stream.WriteByte((byte)(l >> 16));
                _stream.WriteByte((byte)(l >> 8));
                _stream.WriteByte((byte)l);
                _stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void PackString(string value)
        {
            PackString(value, Encoding.UTF8);
        }

        public void PackString(string value, Encoding encoding)
        {
            if (value == null)
            {
                PackNull();
                return;
            }

            var bytes = encoding.GetBytes(value);

            if (bytes.Length <= 0x1F)
            {
                _stream.WriteByte((byte)(0xA0 | bytes.Length));
                _stream.Write(bytes, 0, bytes.Length);
            }
            else if (bytes.Length <= 0xFF)
            {
                _stream.WriteByte(0xD9);
                _stream.WriteByte((byte)bytes.Length);
                _stream.Write(bytes, 0, bytes.Length);
            }
            else if (bytes.Length <= 0xFFFF)
            {
                _stream.WriteByte(0xDA);
                var l = bytes.Length;
                _stream.WriteByte((byte)(l >> 8));
                _stream.WriteByte((byte)l);
                _stream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                _stream.WriteByte(0xDB);
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
            _stream.WriteByte(0xCA);
            _stream.Write(BitConverter.GetBytes(value), 0, 4);
        }

        public void Pack(double value)
        {
            _stream.WriteByte(0xCB);
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
            Pack((uint)value);
        }

        private void Pack(uint value)
        {
            if (value <= 0x7F)
            {
                // positive fixnum (7-bit positive number)
                _stream.WriteByte((byte)value);
            }
            else if (value <= byte.MaxValue)
            {
                _stream.WriteByte(0xCC);
                _stream.WriteByte((byte)value);
            }
            else if (value <= ushort.MaxValue)
            {
                _stream.WriteByte(0xCD);
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else // if (value <= uint.MaxValue)
            {
                _stream.WriteByte(0xCE);
                _stream.WriteByte((byte)(value >> 24));
                _stream.WriteByte((byte)(value >> 16));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
        }

        public void Pack(int value)
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
                _stream.WriteByte(0xD0);
                _stream.WriteByte((byte)value);
            }
            else if (value >= short.MinValue && value <= short.MaxValue)
            {
                _stream.WriteByte(0xD1);
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
            else // if (value >= int.MinValue && value <= int.MaxValue)
            {
                _stream.WriteByte(0xD2);
                _stream.WriteByte((byte)(value >> 24));
                _stream.WriteByte((byte)(value >> 16));
                _stream.WriteByte((byte)(value >> 8));
                _stream.WriteByte((byte)value);
            }
        }
    }
}