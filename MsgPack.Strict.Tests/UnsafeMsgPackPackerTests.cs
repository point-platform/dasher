using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace MsgPack.Strict.Tests
{
    public sealed class UnsafeUnsafeMsgPackPackerTests
    {
        [Fact]
        public void PackPerfFaceOff()
        {
            var s = new MemoryStream();

            var thisUnsafePacker = new UnsafeMsgPackPacker(s);

            var str = new string('a', 256);
            var bytes = new byte[256];

            const int loopCount = 1024 * 1024;

            Action thisUnsafePack = () =>
            {
                s.Position = 0;
                thisUnsafePacker.Pack(false);
                thisUnsafePacker.Pack(true);
                thisUnsafePacker.Pack((byte)1);
                thisUnsafePacker.Pack((sbyte)-1);
                thisUnsafePacker.Pack(1.1f);
                thisUnsafePacker.Pack(1.1d);
                thisUnsafePacker.Pack((short)1234);
                thisUnsafePacker.Pack((ushort)1234);
                thisUnsafePacker.Pack((int)1234);
                thisUnsafePacker.Pack((uint)1234);
                thisUnsafePacker.Pack((long)1234);
                thisUnsafePacker.Pack((ulong)1234);
                thisUnsafePacker.Pack("Hello World");
//                thisUnsafePacker.Pack(str);
//                thisUnsafePacker.Pack(bytes);
                thisUnsafePacker.Flush();
            };

            for (var i = 0; i < 10; i++)
            {
                thisUnsafePack();
            }

            var sw = Stopwatch.StartNew();

            for (var i = 0; i < loopCount; i++)
                thisUnsafePack();

            var thisUnsafeTime = sw.Elapsed.TotalMilliseconds;
        }

        [Fact]
        public void PacksByte()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            for (var i = (int)byte.MinValue; i <= byte.MaxValue; i++)
            {
                stream.Position = 0;

                packer.Pack((byte)i);
                packer.Flush();

                stream.Position = 0;

                byte result;
                Assert.True(unpacker.TryReadByte(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksSByte()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            for (var i = (int)sbyte.MinValue; i <= sbyte.MaxValue; i++)
            {
                stream.Position = 0;

                packer.Pack((sbyte)i);
                packer.Flush();

                stream.Position = 0;

                sbyte result;
                Assert.True(unpacker.TryReadSByte(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksInt16()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            for (var i = (int)short.MinValue; i <= short.MaxValue; i++)
            {
                stream.Position = 0;

                packer.Pack((short)i);
                packer.Flush();

                stream.Position = 0;

                short result;
                Assert.True(unpacker.TryReadInt16(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksUInt16()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            for (var i = (int)ushort.MinValue; i <= ushort.MaxValue; i++)
            {
                stream.Position = 0;

                packer.Pack((ushort)i);
                packer.Flush();

                stream.Position = 0;

                ushort result;
                Assert.True(unpacker.TryReadUInt16(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksInt32()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = Enumerable.Range(-60000, 60000*2).Concat(new[] {int.MinValue, int.MaxValue, int.MinValue + 1, int.MaxValue - 1});

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);
                packer.Flush();

                stream.Position = 0;

                int result;
                Assert.True(unpacker.TryReadInt32(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksUInt32()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = Enumerable.Range(0, 60000 * 2)
                .Select(i => (uint)i)
                .Concat(new[] { (uint)int.MaxValue, (uint)int.MaxValue - 1, uint.MaxValue, uint.MaxValue - 1 });

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);
                packer.Flush();

                stream.Position = 0;

                uint result;
                Assert.True(unpacker.TryReadUInt32(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksInt64()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = Enumerable.Range(-60000, 60000*2)
                .Select(i => (long)i)
                .Concat(new[] {int.MinValue, int.MaxValue, int.MinValue + 1, int.MaxValue - 1, int.MaxValue + 1L, int.MinValue - 1L, long.MinValue, long.MaxValue, long.MinValue + 1, long.MaxValue - 1});

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);
                packer.Flush();

                stream.Position = 0;

                long result;
                Assert.True(unpacker.TryReadInt64(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksUInt64()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = Enumerable.Range(-60000, 60000 * 2)
                .Select(i => (ulong)i)
                .Concat(new ulong[] { int.MaxValue, int.MaxValue - 1, int.MaxValue + 1L, ulong.MinValue, ulong.MaxValue, ulong.MinValue + 1, ulong.MaxValue - 1 });

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);
                packer.Flush();

                stream.Position = 0;

                ulong result;
                Assert.True(unpacker.TryReadUInt64(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksSingle()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new[] {123.4f, float.MinValue, float.MaxValue, 0.0f, 1.0f, -1.0f, 0.1f, -0.1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity, float.Epsilon};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);
                packer.Flush();

                stream.Position = 0;

                float result;
                Assert.True(unpacker.TryReadFloat(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksDouble()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new[] {double.MinValue, double.MaxValue, 0.0f, 1.0f, -1.0f, 0.1f, -0.1f, double.NaN, double.PositiveInfinity, double.NegativeInfinity, double.Epsilon};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);
                packer.Flush();

                stream.Position = 0;

                double result;
                Assert.True(unpacker.TryReadDouble(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksString()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new[] {"Hello", "", Environment.NewLine, null, "\0"};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);
                packer.Flush();

                stream.Position = 0;

                string result;
                Assert.True(unpacker.TryReadString(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksStringWithEncoding()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new[] {"Hello", "", Environment.NewLine, null, "\0", new string('A', 0xFF), new string('A', 0x100), new string('A', 0x10000) };

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i, Encoding.UTF8);
                packer.Flush();

                stream.Position = 0;

                string result;
                Assert.True(unpacker.TryReadString(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksBool()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new[] {true, false};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);
                packer.Flush();

                stream.Position = 0;

                bool result;
                Assert.True(unpacker.TryReadBool(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksBytes()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new[] {new byte[0xFF], /*new byte[0xFFFF], null, new byte[0], new byte[0x10000], new byte[] {1,2,3}*/};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);
                packer.Flush();

                stream.Position = 0;

                byte[] result;
                Assert.True(unpacker.TryReadBinary(out result));
                if (i != null)
                    Assert.Equal(i.Length, result.Length);
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksArrayHeader()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new int[] {0, 1, 255, 256, short.MaxValue, short.MaxValue + 1, int.MaxValue};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.PackArrayHeader(i);
                packer.Flush();

                stream.Position = 0;

                int result;
                Assert.True(unpacker.TryReadArrayLength(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksMapHeader()
        {
            var stream = new MemoryStream();
            var packer = new UnsafeMsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new int[] {0, 1, 255, 256, short.MaxValue, short.MaxValue + 1, int.MaxValue};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.PackMapHeader(i);
                packer.Flush();

                stream.Position = 0;

                int result;
                Assert.True(unpacker.TryReadMapLength(out result));
                Assert.Equal(i, result);
            }
        }
    }
}