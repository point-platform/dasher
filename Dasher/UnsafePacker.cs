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
    public sealed unsafe class UnsafePacker : IDisposable
    {
        private readonly Stream _stream;
        private readonly byte[] _buffer;
        private int _offset;

        public UnsafePacker(Stream stream, int bufferSize = 4096)
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
                *(b + _offset++) = NullByte;
        }

        public void PackArrayHeader(uint length)
        {
            CheckBuffer(5);

            fixed (byte* b = _buffer)
            {
                if (length <= FixArrayMaxLength)
                {
                    *(b + _offset++) = (byte)(FixArrayPrefixBits | length);
                }
                else if (length <= ushort.MaxValue)
                {
                    var p = b + _offset;
                    *p++ = Array16PrefixByte;
                    *p++ = (byte)(length>>8);
                    *p   = (byte)length;
                    _offset += 3;
                }
                else
                {
                    var p = b + _offset;
                    *p++ = Array32PrefixByte;
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
                if (length <= FixMapMaxLength)
                {
                    *(b + _offset++) = (byte)(FixMapPrefixBits | length);
                }
                else if (length <= ushort.MaxValue)
                {
                    var p = b + _offset;
                    *p++ = Map16PrefixByte;
                    *p++ = (byte)(length >> 8);
                    *p   = (byte)length;
                    _offset += 3;
                }
                else
                {
                    var p = b + _offset;
                    *p++ = Map32PrefixByte;
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
                *(b + _offset++) = value ? TrueByte : FalseByte;
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
                    *p++ = Bin8PrefixByte;
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
                    *p++ = Bin16PrefixByte;
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
                    *p++ = Bin32PrefixByte;
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

            var byteCount = encoding.GetByteCount(value);

            if (byteCount <= FixStrMaxLength)
            {
                CheckBuffer(1);
                fixed (byte* b = _buffer)
                {
                    *(b + _offset++) = (byte)(FixStrPrefixBits | byteCount);

                    if (_offset + byteCount + 1 <= _buffer.Length)
                    {
                        fixed (char* c = value)
                            encoding.GetBytes(c, value.Length, b + _offset, byteCount);
                        _offset += byteCount;
                        return;
                    }
                }
                Append(encoding.GetBytes(value));
            }
            else if (byteCount <= byte.MaxValue)
            {
                CheckBuffer(2);
                fixed (byte* b = _buffer)
                {
                    var p = b + _offset;
                    *p++ = Str8PrefixByte;
                    *p   = (byte)byteCount;
                    _offset += 2;

                    if (_offset + byteCount + 1 <= _buffer.Length)
                    {
                        fixed (char* c = value)
                            encoding.GetBytes(c, value.Length, b + _offset, byteCount);
                        _offset += byteCount;
                        return;
                    }
                }
                Append(encoding.GetBytes(value));
            }
            else if (byteCount <= ushort.MaxValue)
            {
                CheckBuffer(3);
                fixed (byte* b = _buffer)
                {
                    var p = b + _offset;
                    *p++ = Str16PrefixByte;
                    var l = byteCount;
                    *p++ = (byte)(l >> 8);
                    *p   = (byte)l;
                    _offset += 3;

                    if (_offset + byteCount + 1 <= _buffer.Length)
                    {
                        fixed (char* c = value)
                            encoding.GetBytes(c, value.Length, b + _offset, byteCount);
                        _offset += byteCount;
                        return;
                    }
                }
                Append(encoding.GetBytes(value));
            }
            else
            {
                CheckBuffer(5);
                fixed (byte* b = _buffer)
                {
                    var p = b + _offset;
                    *p++ = Str32PrefixByte;
                    var l = byteCount;
                    *p++ = (byte)(l >> 24);
                    *p++ = (byte)(l >> 16);
                    *p++ = (byte)(l >> 8);
                    *p   = (byte)l;
                    _offset += 5;
                }
                Append(encoding.GetBytes(value));
            }
        }

        public void Pack(float value)
        {
            CheckBuffer(5);

            fixed (byte* b = _buffer)
            {
                var p = b + _offset;
                *p++ = Float32PrefixByte;
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
                *p++ = Float64PrefixByte;
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
                if (value <= PosFixIntMaxValue)
                {
                    *p = value;
                    _offset++;
                }
                else
                {
                    *p++ = UInt8PrefixByte;
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
                if (value >= 0)
                {
                    *p = (byte)value;
                    _offset++;
                }
                else if (value >= NegFixIntMinValue)
                {
                    *p = (byte)value;
                    _offset++;
                }
                else
                {
                    *p++ = Int8PrefixByte;
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
                if (value <= PosFixIntMaxValue)
                {
                    *p = (byte)value;
                    _offset++;
                }
                else if (value <= byte.MaxValue)
                {
                    *p++ = UInt8PrefixByte;
                    *p = (byte)value;
                    _offset += 2;
                }
                else
                {
                    *p++ = UInt16PrefixByte;
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
                if (value >= 0 && value <= PosFixIntMaxValue)
                {
                    *p = (byte)value;
                    _offset++;
                }
                else if (value >= NegFixIntMinValue && value < 0)
                {
                    *p = (byte)value;
                    _offset++;
                }
                else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
                {
                    *p++ = Int8PrefixByte;
                    *p = (byte)value;
                    _offset += 2;
                }
                else
                {
                    *p++ = Int16PrefixByte;
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
                if (value <= PosFixIntMaxValue)
                {
                    *p = (byte)value;
                    _offset++;
                }
                else if (value <= byte.MaxValue)
                {
                    *p++ = UInt8PrefixByte;
                    *p   = (byte)value;
                    _offset += 2;
                }
                else if (value <= ushort.MaxValue)
                {
                    *p++ = UInt16PrefixByte;
                    *p++ = (byte)(value >> 8);
                    *p   = (byte)value;
                    _offset += 3;
                }
                else
                {
                    *p++ = UInt32PrefixByte;
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
                if (value >= 0 && value <= PosFixIntMaxValue)
                {
                    *p = (byte)value;
                    _offset++;
                }
                else if (value >= NegFixIntMinValue && value < 0)
                {
                    *p = (byte)value;
                    _offset++;
                }
                else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
                {
                    *p++ = Int8PrefixByte;
                    *p = (byte)value;
                    _offset += 2;
                }
                else if (value >= short.MinValue && value <= short.MaxValue)
                {
                    *p++ = Int16PrefixByte;
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 3;
                }
                else
                {
                    *p++ = Int32PrefixByte;
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
                if (value >= 0 && value <= PosFixIntMaxValue)
                {
                    *p = (byte)value;
                    _offset++;
                }
                else if (value >= NegFixIntMinValue && value < 0)
                {
                    *p = (byte)value;
                    _offset++;
                }
                else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
                {
                    *p++ = Int8PrefixByte;
                    *p = (byte)value;
                    _offset += 2;
                }
                else if (value >= short.MinValue && value <= short.MaxValue)
                {
                    *p++ = Int16PrefixByte;
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 3;
                }
                else if (value >= int.MinValue && value <= int.MaxValue)
                {
                    *p++ = Int32PrefixByte;
                    *p++ = (byte)(value >> 24);
                    *p++ = (byte)(value >> 16);
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 5;
                }
                else
                {
                    *p++ = Int64PrefixByte;
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
                if (value <= PosFixIntMaxValue)
                {
                    *p = (byte)value;
                    _offset++;
                }
                else if (value <= byte.MaxValue)
                {
                    *p++ = UInt8PrefixByte;
                    *p = (byte)value;
                    _offset += 2;
                }
                else if (value <= ushort.MaxValue)
                {
                    *p++ = UInt16PrefixByte;
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 3;
                }
                else if (value <= uint.MaxValue)
                {
                    *p++ = UInt32PrefixByte;
                    *p++ = (byte)(value >> 24);
                    *p++ = (byte)(value >> 16);
                    *p++ = (byte)(value >> 8);
                    *p = (byte)value;
                    _offset += 5;
                }
                else
                {
                    *p++ = UInt64PrefixByte;
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