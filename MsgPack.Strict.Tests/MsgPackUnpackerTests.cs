using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace MsgPack.Strict.Tests
{
    public sealed class MsgPackUnpackerTests
    {
        [Fact]
        public void TryReadByte()
        {
            foreach (var input in Enumerable.Range(0, 255).Select(i => (byte)i))
            {
                var unpacker = InitTest(p => p.Pack(input));

                byte value;
                Assert.True(unpacker.TryReadByte(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadSByte()
        {
            foreach (var input in Enumerable.Range(sbyte.MinValue, 255).Select(i => (sbyte)i))
            {
                var unpacker = InitTest(p => p.Pack(input));

                sbyte value;
                Assert.True(unpacker.TryReadSByte(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadInt16()
        {
            var inputs = new short[] { short.MinValue, short.MaxValue, 0, 1, -1, 127, -127, 128, 128, 1000, -1000 };

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                short value;
                Assert.True(unpacker.TryReadInt16(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadInt32()
        {
            var inputs = new[] {int.MinValue, int.MaxValue, 0, 1, -1, 127, -127, 128, 128, 1000, -1000, 12345678, -12345678};

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                int value;
                Assert.True(unpacker.TryReadInt32(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadInt64()
        {
            var inputs = new[] {long.MinValue, long.MaxValue, 0, 1, -1, 127, -127, 128, 128, 1000, -1000, 12345678, -12345678, int.MinValue, int.MaxValue};

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                long value;
                Assert.True(unpacker.TryReadInt64(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadFloat()
        {
            var inputs = new[] { 123.4f, float.MinValue, float.MaxValue, 0.0f, 1.0f, -1.0f, 0.1f, -0.1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity, float.Epsilon };

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                float value;
                Assert.True(unpacker.TryReadFloat(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadDouble()
        {
            var inputs = new[] { 123.4d, double.MinValue, double.MaxValue, 0.0f, 1.0f, -1.0f, 0.1f, -0.1f, double.NaN, double.PositiveInfinity, double.NegativeInfinity, double.Epsilon };

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                double value;
                Assert.True(unpacker.TryReadDouble(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadString()
        {
            var inputs = new[] {null, "", "hello", "world"};

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                string value;
                Assert.True(unpacker.TryReadString(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadArrayLength()
        {
            var inputs = new[] {0, 1, 2, 8, 50, 127, 128, 255, 256, short.MaxValue, ushort.MaxValue, int.MaxValue};

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.PackArrayHeader(input));

                int value;
                Assert.True(unpacker.TryReadArrayLength(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadMapLength()
        {
            var inputs = new[] {0, 1, 2, 8, 50, 127, 128, 255, 256, short.MaxValue, ushort.MaxValue, int.MaxValue};

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.PackMapHeader(input));

                int value;
                Assert.True(unpacker.TryReadMapLength(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadMapLengthThenString()
        {
            var stream = new MemoryStream();
            var packer = Packer.Create(stream);
            packer.PackMapHeader(1);
            packer.PackString("hello");

            stream.Position = 0;

            var unpacker = new MsgPackUnpacker(stream);
            int mapLength;
            Assert.True(unpacker.TryReadMapLength(out mapLength), "Unpacking map length");
            Assert.Equal(1, mapLength);

            string hello;
            Assert.True(unpacker.TryReadString(out hello), "Unpacking string");
            Assert.Equal("hello", hello);
        }

        [Fact]
        public void TryReadTwoStrings()
        {
            var stream = new MemoryStream();
            var packer = Packer.Create(stream);
            packer.Pack("hello");
            packer.Pack("world");

            stream.Position = 0;

            var unpacker = new MsgPackUnpacker(stream);
            string hello;
            Assert.True(unpacker.TryReadString(out hello), "Unpacking string 1");
            Assert.Equal("hello", hello);

            string world;
            Assert.True(unpacker.TryReadString(out world), "Unpacking string 2");
            Assert.Equal("world", world);
        }

        [Fact]
        public void TryReadBool()
        {
            var inputs = new[] { true, false};

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                bool value;
                Assert.True(unpacker.TryReadBool(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        #region Test support

        private static MsgPackUnpacker InitTest(Action<Packer> packerAction)
        {
            var stream = new MemoryStream();
            packerAction(Packer.Create(stream));
            stream.Position = 0;

            return new MsgPackUnpacker(stream);
        }

        #endregion
    }
}
