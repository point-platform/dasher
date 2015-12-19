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

        [Fact]
        public void PackPerfFaceOff()
        {
            var s = new MemoryStream();

            var customSafePacker = new MsgPackPacker(s);
            var customUnsafePacker = new UnsafeMsgPackPacker(s);
            var msgpack_cli_Packer = Packer.Create(s);

            var str = new string('a', 256);
            var bytes = new byte[256];

            const int loopCount = 1024 * 1024;

            Action customSafePackerPack = () =>
            {
                s.Position = 0;
                customSafePacker.Pack(false);
                customSafePacker.Pack(true);
                customSafePacker.Pack((byte)1);
                customSafePacker.Pack((sbyte)-1);
                customSafePacker.Pack(1.1f);
                customSafePacker.Pack(1.1d);
                customSafePacker.Pack((short)1234);
                customSafePacker.Pack((ushort)1234);
                customSafePacker.Pack((int)1234);
                customSafePacker.Pack((uint)1234);
                customSafePacker.Pack((long)1234);
                customSafePacker.Pack((ulong)1234);
                customSafePacker.Pack("Hello World");
                customSafePacker.Pack(str);
                customSafePacker.Pack(bytes);
            };

            Action customUnsafePackerPack = () =>
            {
                s.Position = 0;
                customUnsafePacker.Pack(false);
                customUnsafePacker.Pack(true);
                customUnsafePacker.Pack((byte)1);
                customUnsafePacker.Pack((sbyte)-1);
                customUnsafePacker.Pack(1.1f);
                customUnsafePacker.Pack(1.1d);
                customUnsafePacker.Pack((short)1234);
                customUnsafePacker.Pack((ushort)1234);
                customUnsafePacker.Pack((int)1234);
                customUnsafePacker.Pack((uint)1234);
                customUnsafePacker.Pack((long)1234);
                customUnsafePacker.Pack((ulong)1234);
                customUnsafePacker.Pack("Hello World");
                customUnsafePacker.Pack(str);
                customUnsafePacker.Pack(bytes);
                customUnsafePacker.Flush();
            };

            Action msgpack_cli_PackerPack = () =>
            {
                s.Position = 0;
                msgpack_cli_Packer.Pack(false);
                msgpack_cli_Packer.Pack(true);
                msgpack_cli_Packer.Pack((byte)1);
                msgpack_cli_Packer.Pack((sbyte)-1);
                msgpack_cli_Packer.Pack(1.1f);
                msgpack_cli_Packer.Pack(1.1d);
                msgpack_cli_Packer.Pack((short)1234);
                msgpack_cli_Packer.Pack((ushort)1234);
                msgpack_cli_Packer.Pack((int)1234);
                msgpack_cli_Packer.Pack((uint)1234);
                msgpack_cli_Packer.Pack((long)1234);
                msgpack_cli_Packer.Pack((ulong)1234);
                msgpack_cli_Packer.Pack("Hello World");
                msgpack_cli_Packer.Pack(str);
                msgpack_cli_Packer.Pack(bytes);
            };

            for (var i = 0; i < 10; i++)
            {
                customSafePackerPack();
                customUnsafePackerPack();
                msgpack_cli_PackerPack();
            }

            var sw = Stopwatch.StartNew();

            for (var i = 0; i < loopCount; i++)
                customSafePackerPack();

            var customSafePackerPackTime = sw.Elapsed.TotalMilliseconds;

            sw.Restart();

            for (var i = 0; i < loopCount; i++)
                customUnsafePackerPack();

            var customUnsafePackerPackTime = sw.Elapsed.TotalMilliseconds;

            sw.Restart();

            for (var i = 0; i < loopCount; i++)
                msgpack_cli_PackerPack();

            var msgpack_cli_PackerPackTime = sw.Elapsed.TotalMilliseconds;

            Assert.True(false, $"custom safe packer time = {customSafePackerPackTime}, customer unsafe packer time = {customUnsafePackerPackTime}, MsgPack-cli packer time = {msgpack_cli_PackerPackTime}");
        }

        [Fact]
        public void PacksByte()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            for (var i = (int)byte.MinValue; i <= byte.MaxValue; i++)
            {
                stream.Position = 0;

                packer.Pack((byte)i);

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
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            for (var i = (int)sbyte.MinValue; i <= sbyte.MaxValue; i++)
            {
                stream.Position = 0;

                packer.Pack((sbyte)i);

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
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            for (var i = (int)short.MinValue; i <= short.MaxValue; i++)
            {
                stream.Position = 0;

                packer.Pack((short)i);

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
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            for (var i = (int)ushort.MinValue; i <= ushort.MaxValue; i++)
            {
                stream.Position = 0;

                packer.Pack((ushort)i);

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
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = Enumerable.Range(-60000, 60000*2).Concat(new[] {int.MinValue, int.MaxValue, int.MinValue + 1, int.MaxValue - 1});

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

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
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = Enumerable.Range(0, 60000 * 2)
                .Select(i => (uint)i)
                .Concat(new[] { (uint)int.MaxValue, (uint)int.MaxValue - 1, uint.MaxValue, uint.MaxValue - 1 });

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

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
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = Enumerable.Range(-60000, 60000*2)
                .Select(i => (long)i)
                .Concat(new[] {int.MinValue, int.MaxValue, int.MinValue + 1, int.MaxValue - 1, int.MaxValue + 1L, int.MinValue - 1L, long.MinValue, long.MaxValue, long.MinValue + 1, long.MaxValue - 1});

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

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
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = Enumerable.Range(-60000, 60000 * 2)
                .Select(i => (ulong)i)
                .Concat(new ulong[] { int.MaxValue, int.MaxValue - 1, int.MaxValue + 1L, ulong.MinValue, ulong.MaxValue, ulong.MinValue + 1, ulong.MaxValue - 1 });

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

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
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new[] {float.MinValue, float.MaxValue, 0.0f, 1.0f, -1.0f, 0.1f, -0.1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity, float.Epsilon};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

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
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new[] {123.4d, double.MinValue, double.MaxValue, 0.0f, 1.0f, -1.0f, 0.1f, -0.1f, double.NaN, double.PositiveInfinity, double.NegativeInfinity, double.Epsilon};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

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
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new[] {"Hello", "", Environment.NewLine, null, "\0"};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

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
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new[] {"Hello", "", Environment.NewLine, null, "\0", new string('A', 0xFF), new string('A', 0x100), new string('A', 0x10000) };

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i, Encoding.UTF8);

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
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new[] {true, false};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

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
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new[] {null, new byte[0], new byte[0xFF], new byte[0xFFFF], new byte[0x10000], new byte[] {1,2,3}};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.Pack(i);

                stream.Position = 0;

                byte[] result;
                Assert.True(unpacker.TryReadBinary(out result));
                Assert.Equal(i, result);
            }
        }

        [Fact]
        public void PacksArrayHeader()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new int[] {0, 1, 255, 256, short.MaxValue, short.MaxValue + 1, int.MaxValue};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.PackArrayHeader(i);

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
            var packer = new MsgPackPacker(stream);
            var unpacker = MsgPackUnpacker.Create(stream);

            var inputs = new int[] {0, 1, 255, 256, short.MaxValue, short.MaxValue + 1, int.MaxValue};

            foreach (var i in inputs)
            {
                stream.Position = 0;

                packer.PackMapHeader(i);

                stream.Position = 0;

                int result;
                Assert.True(unpacker.TryReadMapLength(out result));
                Assert.Equal(i, result);
            }
        }
    }
}