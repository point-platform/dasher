using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace MsgPack.Strict.Tests
{
    public sealed class MsgPackPackerTests
    {
        [Fact]
        public void StreamWritePerfTest()
        {
            const int bufferSize = 1024;
            const int chunkCount = 1024;

            var s = new MemoryStream(bufferSize * chunkCount);

            s.WriteByte(1);

            var sw = Stopwatch.StartNew();

            for (var i = 0; i < bufferSize * chunkCount; i++)
                s.WriteByte((byte)i);

            var oneByOneTime = sw.Elapsed.TotalMilliseconds;

            s.Position = 0;

            var buffer = Enumerable.Range(0, bufferSize).Select(i => (byte)i).ToArray();

            sw.Restart();

            for (var i = 0; i < chunkCount; i++)
            {
                for (int j = 0; j < buffer.Length; j++)
                    buffer[j] = (byte)j;
                s.Write(buffer, 0, bufferSize);
            }

            var inChunksTime = sw.Elapsed.TotalMilliseconds;

            Console.Out.WriteLine("oneByOneTime = {0}", oneByOneTime);
            Console.Out.WriteLine("inChunksTime = {0}", inChunksTime);
        }

//        [Fact]
        public void PackPerfFaceOff()
        {
            var s = new MemoryStream();

            var thisPacker = new MsgPackPacker(s);
            var thisUnsafePacker = new UnsafeMsgPackPacker(s);
            var thatPacker = Packer.Create(s);

            var str = new string('a', 256);
            var bytes = new byte[256];

            const int loopCount = 1024 * 1024;

            Action thisBytePack = () =>
            {
                s.Position = 0;
                thisPacker.Pack(false);
                thisPacker.Pack(true);
                thisPacker.Pack((byte)1);
                thisPacker.Pack((sbyte)-1);
                thisPacker.Pack(1.1f);
                thisPacker.Pack(1.1d);
                thisPacker.Pack((short)1234);
                thisPacker.Pack((ushort)1234);
                thisPacker.Pack((int)1234);
                thisPacker.Pack((uint)1234);
                thisPacker.Pack((long)1234);
                thisPacker.Pack((ulong)1234);
                thisPacker.Pack("Hello World");
//                thisPacker.Pack(str);
//                thisPacker.Pack(bytes);
            };

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

            Action thatPack = () =>
            {
                s.Position = 0;
                thatPacker.Pack(false);
                thatPacker.Pack(true);
                thatPacker.Pack((byte)1);
                thatPacker.Pack((sbyte)-1);
                thatPacker.Pack(1.1f);
                thatPacker.Pack(1.1d);
                thatPacker.Pack((short)1234);
                thatPacker.Pack((ushort)1234);
                thatPacker.Pack((int)1234);
                thatPacker.Pack((uint)1234);
                thatPacker.Pack((long)1234);
                thatPacker.Pack((ulong)1234);
                thatPacker.Pack("Hello World");
//                thatPacker.Pack(str);
//                thatPacker.Pack(bytes);
            };

            for (var i = 0; i < 10; i++)
            {
                thisBytePack();
                thisUnsafePack();
                thatPack();
            }

            var sw = Stopwatch.StartNew();

            for (var i = 0; i < loopCount; i++)
                thisBytePack();

            var thisBytePackTime = sw.Elapsed.TotalMilliseconds;

            sw.Restart();

            for (var i = 0; i < loopCount; i++)
                thisUnsafePack();

            var thisUnsafeTime = sw.Elapsed.TotalMilliseconds;

            sw.Restart();

            for (var i = 0; i < loopCount; i++)
                thatPack();

            var thatPackTime = sw.Elapsed.TotalMilliseconds;

            Assert.True(false, $"thisBytePackTime={thisBytePackTime}, thisUnsafeTime={thisUnsafeTime}, thatPackTime={thatPackTime}");
        }

        [Fact]
        public void PacksByte()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            for (var i = (int)byte.MinValue; i <= byte.MaxValue; i++)
            {
                stream.Position = 0;

                packer.Pack((byte)i);

                stream.Position = 0;

                byte result;
                Assert.True(unpacker.ReadByte(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksSByte()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            for (var i = (int)sbyte.MinValue; i <= sbyte.MaxValue; i++)
            {
                stream.Position = 0;

                packer.Pack((sbyte)i);

                stream.Position = 0;

                sbyte result;
                Assert.True(unpacker.ReadSByte(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksInt16()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            for (var i = (int)short.MinValue; i <= short.MaxValue; i++)
            {
                stream.Position = 0;

                packer.Pack((short)i);

                stream.Position = 0;

                short result;
                Assert.True(unpacker.ReadInt16(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksUInt16()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            for (var i = (int)ushort.MinValue; i <= ushort.MaxValue; i++)
            {
                stream.Position = 0;

                packer.Pack((ushort)i);

                stream.Position = 0;

                ushort result;
                Assert.True(unpacker.ReadUInt16(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksInt32()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            var inputs = Enumerable.Range(-60000, 60000*2).Concat(new[] {int.MinValue, int.MaxValue, int.MinValue + 1, int.MaxValue - 1});

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

                stream.Position = 0;

                int result;
                Assert.True(unpacker.ReadInt32(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksUInt32()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            var inputs = Enumerable.Range(0, 60000 * 2)
                .Select(i => (uint)i)
                .Concat(new[] { (uint)int.MaxValue, (uint)int.MaxValue - 1, uint.MaxValue, uint.MaxValue - 1 });

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

                stream.Position = 0;

                uint result;
                Assert.True(unpacker.ReadUInt32(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksInt64()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            var inputs = Enumerable.Range(-60000, 60000*2)
                .Select(i => (long)i)
                .Concat(new[] {int.MinValue, int.MaxValue, int.MinValue + 1, int.MaxValue - 1, int.MaxValue + 1L, int.MinValue - 1L, long.MinValue, long.MaxValue, long.MinValue + 1, long.MaxValue - 1});

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

                stream.Position = 0;

                long result;
                Assert.True(unpacker.ReadInt64(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksUInt64()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            var inputs = Enumerable.Range(-60000, 60000 * 2)
                .Select(i => (ulong)i)
                .Concat(new ulong[] { int.MaxValue, int.MaxValue - 1, int.MaxValue + 1L, ulong.MinValue, ulong.MaxValue, ulong.MinValue + 1, ulong.MaxValue - 1 });

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

                stream.Position = 0;

                ulong result;
                Assert.True(unpacker.ReadUInt64(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksSingle()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            var inputs = new[] {float.MinValue, float.MaxValue, 0.0f, 1.0f, -1.0f, 0.1f, -0.1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity, float.Epsilon};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

                stream.Position = 0;

                float result;
                Assert.True(unpacker.ReadSingle(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksDouble()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            var inputs = new[] {double.MinValue, double.MaxValue, 0.0f, 1.0f, -1.0f, 0.1f, -0.1f, double.NaN, double.PositiveInfinity, double.NegativeInfinity, double.Epsilon};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

                stream.Position = 0;

                double result;
                Assert.True(unpacker.ReadDouble(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksString()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            var inputs = new[] {"Hello", "", Environment.NewLine, null, "\0"};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

                stream.Position = 0;

                string result;
                Assert.True(unpacker.ReadString(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksStringWithEncoding()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            var inputs = new[] {"Hello", "", Environment.NewLine, null, "\0", new string('A', 0xFF), new string('A', 0x100), new string('A', 0x10000) };

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i, Encoding.UTF8);

                stream.Position = 0;

                string result;
                Assert.True(unpacker.ReadString(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksBool()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            var inputs = new[] {true, false};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

                stream.Position = 0;

                bool result;
                Assert.True(unpacker.ReadBoolean(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksBytes()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            var inputs = new[] {null, new byte[0], new byte[0xFF], new byte[0xFFFF], new byte[0x10000], new byte[] {1,2,3}};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

                stream.Position = 0;

                byte[] result;
                Assert.True(unpacker.ReadBinary(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksArrayHeader()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            var inputs = new uint[] {0, 1, 255, 256, ushort.MaxValue, ushort.MaxValue + 1, int.MaxValue};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.PackArrayHeader(i);

                stream.Position = 0;

                long result;
                Assert.True(unpacker.ReadArrayLength(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksMapHeader()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            var inputs = new uint[] {0, 1, 255, 256, ushort.MaxValue, ushort.MaxValue + 1, int.MaxValue};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.PackMapHeader(i);

                stream.Position = 0;

                long result;
                Assert.True(unpacker.ReadMapLength(out result));
                Assert.Equal(i, result);
            }
        }
    }
}