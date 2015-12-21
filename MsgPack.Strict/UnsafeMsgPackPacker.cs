using System;
using System.IO;
using System.Text;

namespace MsgPack.Strict
{
    public unsafe class UnsafeMsgPackPacker : IDisposable
    {
        private readonly Stream _stream;
        private readonly byte[] _buffer;
        private int _offset;

        public UnsafeMsgPackPacker(Stream stream, int bufferSize = 4096)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (bufferSize < 1024)
                throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, "Must be 1024 or greater.");

            _stream = stream;
            _buffer = new byte[bufferSize];
        }

        public void Flush()
        {
            if (_offset == 0)
                return;
            _stream.Write(_buffer, 0, _offset);
            _offset = 0;
        }

        void IDisposable.Dispose()
        {
            Flush();
        }

        private void CheckBuffer(int space)
        {
            if (_offset + space > _buffer.Length)
                Flush();
        }

        private void Append(byte[] bytes)
        {
            if (_offset + bytes.Length <= _buffer.Length)
            {
                // copy to buffer
                Array.Copy(bytes, 0, _buffer, _offset, bytes.Length);
                _offset += bytes.Length;
            }
            else
            {
                // buffer will spill
                Flush();
//                if (bytes.Length < 20)
//                {
//                     // TODO copy short string to buffer
//                }
//                else
//                {
                    _stream.Write(bytes, 0, bytes.Length);
//                }
            }
        }

        public void PackNull()
        {
            CheckBuffer(1);

            fixed (byte* b = _buffer)
                *(b + _offset++) = 0xc0;
        }

        public void PackArrayHeader(uint length)
        {
            CheckBuffer(5);

            fixed (byte* b = _buffer)
            {
                if (length <= 0x0F)
                {
                    *(b + _offset++) = (byte)(0x90 | length);
                }
                else if (length <= 0xFFFF)
                {
                    var p = b + _offset;
                    *p++ = 0xDC;
                    *p++ = (byte)(length>>8);
                    *p   = (byte)length;
                    _offset += 3;
                }
                else
                {
                    var p = b + _offset;
                    *p++ = 0xDD;
                    *p++ = (byte)(length >> 24);
                    *p++ = (byte)(length >> 16);
                    *p++ = (byte)(length >> 8);
                    *p   = (byte)length;
                    _offset += 5;
                }
            }
        }

        public void PackMapHeader(uint length)
        {
            CheckBuffer(5);

            fixed (byte* b = _buffer)
            {
                if (length <= 0x0F)
                {
                    *(b + _offset++) = (byte)(0x80 | length);
                }
                else if (length <= 0xFFFF)
                {
                    var p = b + _offset;
                    *p++ = 0xDE;
                    *p++ = (byte)(length >> 8);
                    *p   = (byte)length;
                    _offset += 3;
                }
                else
                {
                    var p = b + _offset;
                    *p++ = 0xDF;
                    *p++ = (byte)(length >> 24);
                    *p++ = (byte)(length >> 16);
                    *p++ = (byte)(length >> 8);
                    *p   = (byte)length;
                    _offset += 5;
                }
            }
        }

        public void Pack(bool value)
        {
            CheckBuffer(1);

            fixed (byte* b = _buffer)
                *(b + _offset++) = value ? (byte)0xC3 : (byte)0xC2;
        }

        public void Pack(byte[] bytes)
        {
            if (bytes == null)
            {
                CheckBuffer(1);
                PackNull();
                return;
            }

            CheckBuffer(5);

            var length = bytes.Length;

            if (length <= byte.MaxValue)
            {
                fixed (byte* b = _buffer)
                {
                    var p = b + _offset;
                    *p++ = 0xC4;
                    *p = (byte)length;
                    _offset += 2;
                }
                Append(bytes);
            }
            else if (length <= ushort.MaxValue)
            {
                fixed (byte* b = _buffer)
                {
                    var p = b + _offset;
                    *p++ = 0xC5;
                    *p++ = (byte)(length >> 8);
                    *p   = (byte)length;
                    _offset += 3;
                }
                Append(bytes);
            }
            else
            {
                fixed (byte* b = _buffer)
                {
                    var p = b + _offset;
                    *p++ = 0xC6;
                    *p++ = (byte)(length >> 24);
                    *p++ = (byte)(length >> 16);
                    *p++ = (byte)(length >> 8);
                    *p   = (byte)length;
                    _offset += 5;
                }
                Append(bytes);
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
                CheckBuffer(1);
                PackNull();
                return;
            }

            var bytes = encoding.GetBytes(value);

            if (bytes.Length <= 0x1F)
            {
                CheckBuffer(1);
                fixed (byte* b = _buffer)
                    *(b + _offset++) = (byte)(0xA0 | bytes.Length);
                Append(bytes);
            }
            else if (bytes.Length <= 0xFF)
            {
                CheckBuffer(2);
                fixed (byte* b = _buffer)
                {
                    var p = b + _offset;
                    *p++ = 0xD9;
                    *p   = (byte)bytes.Length;
                    _offset += 2;
                }
                Append(bytes);
            }
            else if (bytes.Length <= 0xFFFF)
            {
                CheckBuffer(3);
                fixed (byte* b = _buffer)
                {
                    var p = b + _offset;
                    *p++ = 0xDA;
                    var l = bytes.Length;
                    *p++ = (byte)(l >> 8);
                    *p   = (byte)l;
                    _offset += 3;
                }
                Append(bytes);
            }
            else
            {
                CheckBuffer(5);
                fixed (byte* b = _buffer)
                {
                    var p = b + _offset;
                    *p++ = 0xDB;
                    var l = bytes.Length;
                    *p++ = (byte)(l >> 24);
                    *p++ = (byte)(l >> 16);
                    *p++ = (byte)(l >> 8);
                    *p   = (byte)l;
                    _offset += 5;
                }
                Append(bytes);
            }
        }

        public void Pack(float value)
        {
            CheckBuffer(5);

            fixed (byte* b = _buffer)
            {
                var p = b + _offset;
                *p++ = 0xCA;
                var l = *(int*)&value;
                *p++ = (byte)(l >> 24);
                *p++ = (byte)(l >> 16);
                *p++ = (byte)(l >> 8);
                *p = (byte)l;
                _offset += 5;
            }
        }

        public void Pack(double value)
        {
            CheckBuffer(9);

            fixed (byte* b = _buffer)
            {
                var p = b + _offset;
                *p++ = 0xCB;
                var l = *(ulong*)&value;
                *p++ = (byte)(l >> 56);
                *p++ = (byte)(l >> 48);
                *p++ = (byte)(l >> 40);
                *p++ = (byte)(l >> 32);
                *p++ = (byte)(l >> 24);
                *p++ = (byte)(l >> 16);
                *p++ = (byte)(l >> 8);
                *p = (byte)l;
                _offset += 9;
            }
        }

        public void Pack(byte value)
        {
            CheckBuffer(2);

            fixed (byte* b = _buffer)
            {
                var p = b + _offset;
                if (value <= 0x7F)
                {
                    // positive fixnum (7-bit positive number)
                    *p = value;
                    _offset++;
                }
                else
                {
                    *p++ = 0xCC;
                    *p = value;
                    _offset += 2;
                }
            }
        }

        public void Pack(sbyte value)
        {
            CheckBuffer(2);

            fixed (byte* b = _buffer)
            {
                var p = b + _offset;
                if (value >= 0x00)
                {
                    // positive fixnum (7-bit positive number)
                    *p = (byte)value;
                    _offset++;
                }
                else if (value >= -32 /*0b_1110_000*/ && value < 0x00)
                {
                    // negative fixnum (5-bit negative number)
                    *p = (byte)(value | 0xE0);
                    _offset++;
                }
                else
                {
                    *p++ = 0xD0;
                    *p = (byte)value;
                    _offset += 2;
                }
            }
        }

        public void Pack(ushort value)
        {
            CheckBuffer(3);

            fixed (byte* b = _buffer)
            {
                var p = b + _offset;
                if (value <= 0x7F)
                {
                    // positive fixnum (7-bit positive number)
                    *p = (byte)value;
                    _offset++;
                }
                else if (value <= byte.MaxValue)
                {
                    *p++ = 0xCC;
                    *p = (byte)value;
                    _offset += 2;
                }
                else
                {
                    *p++ = 0xCD;
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 3;
                }
            }
        }

        public void Pack(short value)
        {
            CheckBuffer(3);

            fixed (byte* b = _buffer)
            {
                var p = b + _offset;
                if (value >= 0x00 && value <= sbyte.MaxValue)
                {
                    // positive fixnum (7-bit positive number)
                    *p = (byte)value;
                    _offset++;
                }
                else if (value >= -32 /*0b_1110_000*/ && value < 0x00)
                {
                    // negative fixnum (5-bit negative number)
                    *p = (byte)(value | 0xE0);
                    _offset++;
                }
                else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
                {
                    *p++ = 0xD0;
                    *p = (byte)value;
                    _offset += 2;
                }
                else
                {
                    *p++ = 0xD1;
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 3;
                }
            }
        }

        public void Pack(uint value)
        {
            CheckBuffer(5);

            fixed (byte* b = _buffer)
            {
                var p = b + _offset;
                if (value <= 0x7F)
                {
                    // positive fixnum (7-bit positive number)
                    *p = (byte)value;
                    _offset++;
                }
                else if (value <= byte.MaxValue)
                {
                    *p++ = 0xCC;
                    *p   = (byte)value;
                    _offset += 2;
                }
                else if (value <= ushort.MaxValue)
                {
                    *p++ = 0xCD;
                    *p++ = (byte)(value >> 8);
                    *p   = (byte)value;
                    _offset += 3;
                }
                else
                {
                    *p++ = 0xCE;
                    *p++ = (byte)(value >> 24);
                    *p++ = (byte)(value >> 16);
                    *p++ = (byte)(value >> 8);
                    *p   = (byte)value;
                    _offset += 5;
                }
            }
        }

        public void Pack(int value)
        {
            CheckBuffer(5);

            fixed (byte* b = _buffer)
            {
                var p = b + _offset;
                if (value >= 0x00 && value <= sbyte.MaxValue)
                {
                    // positive fixnum (7-bit positive number)
                    *p = (byte)value;
                    _offset++;
                }
                else if (value >= -32 /*0b_1110_000*/ && value < 0x00)
                {
                    // negative fixnum (5-bit negative number)
                    *p = (byte)(value | 0xE0);
                    _offset++;
                }
                else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
                {
                    *p++ = 0xD0;
                    *p = (byte)value;
                    _offset += 2;
                }
                else if (value >= short.MinValue && value <= short.MaxValue)
                {
                    *p++ = 0xD1;
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 3;
                }
                else // if (value >= int.MinValue && value <= int.MaxValue)
                {
                    *p++ = 0xD2;
                    *p++ = (byte)(value >> 24);
                    *p++ = (byte)(value >> 16);
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 5;
                }
            }
        }

        public void Pack(long value)
        {
            CheckBuffer(9);

            fixed (byte* b = _buffer)
            {
                var p = b + _offset;
                if (value >= 0x00 && value <= sbyte.MaxValue)
                {
                    // positive fixnum (7-bit positive number)
                    *p = (byte)value;
                    _offset++;
                }
                else if (value >= -32 /*0b_1110_000*/ && value < 0x00)
                {
                    // negative fixnum (5-bit negative number)
                    *p = (byte)(value | 0xE0);
                    _offset++;
                }
                else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
                {
                    *p++ = 0xD0;
                    *p = (byte)value;
                    _offset += 2;
                }
                else if (value >= short.MinValue && value <= short.MaxValue)
                {
                    *p++ = 0xD1;
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 3;
                }
                else if (value >= int.MinValue && value <= int.MaxValue)
                {
                    *p++ = 0xD2;
                    *p++ = (byte)(value >> 24);
                    *p++ = (byte)(value >> 16);
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 5;
                }
                else
                {
                    *p++ = 0xD3;
                    *p++ = (byte)(value >> 56);
                    *p++ = (byte)(value >> 48);
                    *p++ = (byte)(value >> 40);
                    *p++ = (byte)(value >> 32);
                    *p++ = (byte)(value >> 24);
                    *p++ = (byte)(value >> 16);
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 9;
                }
            }
        }

        public void Pack(ulong value)
        {
            CheckBuffer(9);

            fixed (byte* b = _buffer)
            {
                var p = b + _offset;
                if (value <= 0x7F)
                {
                    // positive fixnum (7-bit positive number)
                    *p = (byte)value;
                    _offset++;
                }
                else if (value <= byte.MaxValue)
                {
                    *p++ = 0xCC;
                    *p = (byte)value;
                    _offset += 2;
                }
                else if (value <= ushort.MaxValue)
                {
                    *p++ = 0xCD;
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 3;
                }
                else if (value <= uint.MaxValue)
                {
                    *p++ = 0xCE;
                    *p++ = (byte)(value >> 24);
                    *p++ = (byte)(value >> 16);
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 5;
                }
                else
                {
                    *p++ = 0xCF;
                    *p++ = (byte)(value >> 56);
                    *p++ = (byte)(value >> 48);
                    *p++ = (byte)(value >> 40);
                    *p++ = (byte)(value >> 32);
                    *p++ = (byte)(value >> 24);
                    *p++ = (byte)(value >> 16);
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 9;
                }
            }
        }
    }
}