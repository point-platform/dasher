using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Dasher.Tests
{
    public sealed class UnsafePackerTests
    {
        private readonly MemoryStream _stream;
        private readonly UnsafePacker _packer;
        private readonly MsgPack.Unpacker _unpacker;

        public UnsafePackerTests()
        {
            _stream = new MemoryStream();
            _packer = new UnsafePacker(_stream);
            _unpacker = MsgPack.Unpacker.Create(_stream);
        }

        [Fact]
        public void PacksByte()
        {
            for (var i = (int)byte.MinValue; i <= byte.MaxValue; i++)
            {
                _stream.Position = 0;

                _packer.Pack((byte)i);
                _packer.Flush();

                _stream.Position = 0;

                byte result;
                Assert.True(_unpacker.ReadByte(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksSByte()
        {
            for (var i = (int)sbyte.MinValue; i <= sbyte.MaxValue; i++)
            {
                _stream.Position = 0;

                _packer.Pack((sbyte)i);
                _packer.Flush();

                _stream.Position = 0;

                sbyte result;
                Assert.True(_unpacker.ReadSByte(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksInt16()
        {
            for (var i = (int)short.MinValue; i <= short.MaxValue; i++)
            {
                _stream.Position = 0;

                _packer.Pack((short)i);
                _packer.Flush();

                _stream.Position = 0;

                short result;
                Assert.True(_unpacker.ReadInt16(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksUInt16()
        {
            for (var i = (int)ushort.MinValue; i <= ushort.MaxValue; i++)
            {
                _stream.Position = 0;

                _packer.Pack((ushort)i);
                _packer.Flush();

                _stream.Position = 0;

                ushort result;
                Assert.True(_unpacker.ReadUInt16(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksInt32()
        {
            var inputs = Enumerable.Range(-60000, 60000*2).Concat(new[] {int.MinValue, int.MaxValue, int.MinValue + 1, int.MaxValue - 1});

            foreach (var i in inputs)
            {
                _stream.Position = 0;

                _packer.Pack(i);
                _packer.Flush();

                _stream.Position = 0;

                int result;
                Assert.True(_unpacker.ReadInt32(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksUInt32()
        {
            var inputs = Enumerable.Range(0, 60000 * 2)
                .Select(i => (uint)i)
                .Concat(new[] { (uint)int.MaxValue, (uint)int.MaxValue - 1, uint.MaxValue, uint.MaxValue - 1 });

            foreach (var i in inputs)
            {
                _stream.Position = 0;

                _packer.Pack(i);
                _packer.Flush();

                _stream.Position = 0;

                uint result;
                Assert.True(_unpacker.ReadUInt32(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksInt64()
        {
            var inputs = Enumerable.Range(-60000, 60000*2)
                .Select(i => (long)i)
                .Concat(new[] {int.MinValue, int.MaxValue, int.MinValue + 1, int.MaxValue - 1, int.MaxValue + 1L, int.MinValue - 1L, long.MinValue, long.MaxValue, long.MinValue + 1, long.MaxValue - 1});

            foreach (var i in inputs)
            {
                _stream.Position = 0;

                _packer.Pack(i);
                _packer.Flush();

                _stream.Position = 0;

                long result;
                Assert.True(_unpacker.ReadInt64(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksUInt64()
        {
            var inputs = Enumerable.Range(-60000, 60000 * 2)
                .Select(i => (ulong)i)
                .Concat(new ulong[] { int.MaxValue, int.MaxValue - 1, int.MaxValue + 1L, ulong.MinValue, ulong.MaxValue, ulong.MinValue + 1, ulong.MaxValue - 1 });

            foreach (var i in inputs)
            {
                _stream.Position = 0;

                _packer.Pack(i);
                _packer.Flush();

                _stream.Position = 0;

                ulong result;
                Assert.True(_unpacker.ReadUInt64(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksSingle()
        {
            var inputs = new[] {float.MinValue, float.MaxValue, 0.0f, 1.0f, -1.0f, 0.1f, -0.1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity, float.Epsilon};

            foreach (var i in inputs)
            {
                _stream.Position = 0;

                _packer.Pack(i);
                _packer.Flush();

                _stream.Position = 0;

                float result;
                Assert.True(_unpacker.ReadSingle(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksDouble()
        {
            var inputs = new[] {double.MinValue, double.MaxValue, 0.0f, 1.0f, -1.0f, 0.1f, -0.1f, double.NaN, double.PositiveInfinity, double.NegativeInfinity, double.Epsilon};

            foreach (var i in inputs)
            {
                _stream.Position = 0;

                _packer.Pack(i);
                _packer.Flush();

                _stream.Position = 0;

                double result;
                Assert.True(_unpacker.ReadDouble(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksString()
        {
            var inputs = new[] {"Hello", "", Environment.NewLine, null, "\0"};

            foreach (var i in inputs)
            {
                _stream.Position = 0;

                _packer.Pack(i);
                _packer.Flush();

                _stream.Position = 0;

                string result;
                Assert.True(_unpacker.ReadString(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksStringWithEncoding()
        {
            var inputs = new[] {"Hello", "", Environment.NewLine, null, "\0", new string('A', 0xFF), new string('A', 0x100), new string('A', 0x10000) };

            foreach (var i in inputs)
            {
                _stream.Position = 0;

                _packer.Pack(i, Encoding.UTF8);
                _packer.Flush();

                _stream.Position = 0;

                string result;
                Assert.True(_unpacker.ReadString(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksBool()
        {
            var inputs = new[] {true, false};

            foreach (var i in inputs)
            {
                _stream.Position = 0;

                _packer.Pack(i);
                _packer.Flush();

                _stream.Position = 0;

                bool result;
                Assert.True(_unpacker.ReadBoolean(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksBytes()
        {
            var inputs = new[] {new byte[0xFF], /*new byte[0xFFFF], null, new byte[0], new byte[0x10000], new byte[] {1,2,3}*/};

            foreach (var i in inputs)
            {
                _stream.Position = 0;

                _packer.Pack(i);
                _packer.Flush();

                _stream.Position = 0;

                byte[] result;
                Assert.True(_unpacker.ReadBinary(out result));
                if (i != null)
                    Assert.Equal(i.Length, result.Length);
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksArrayHeader()
        {
            var inputs = new uint[] {0, 1, 255, 256, ushort.MaxValue, ushort.MaxValue + 1, int.MaxValue};

            foreach (var i in inputs)
            {
                _stream.Position = 0;

                _packer.PackArrayHeader(i);
                _packer.Flush();

                _stream.Position = 0;

                long result;
                Assert.True(_unpacker.ReadArrayLength(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksMapHeader()
        {
            var inputs = new uint[] {0, 1, 255, 256, ushort.MaxValue, ushort.MaxValue + 1, int.MaxValue};

            foreach (var i in inputs)
            {
                _stream.Position = 0;

                _packer.PackMapHeader(i);
                _packer.Flush();

                _stream.Position = 0;

                long result;
                Assert.True(_unpacker.ReadMapLength(out result));
                Assert.Equal(i, result);
            }
        }
    }
}