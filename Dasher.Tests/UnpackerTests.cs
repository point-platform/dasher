using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MsgPack;
using Xunit;
using Xunit.Sdk;

namespace Dasher.Tests
{
    public sealed class UnpackerTests
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
            var inputs = new short[] { short.MinValue, short.MaxValue, 0, 1, -1, 127, -127, 128, -128, 1000, -1000 };

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                short value;
                Assert.True(unpacker.TryReadInt16(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadUInt16()
        {
            var inputs = new ushort[] { ushort.MinValue, ushort.MaxValue, 0, 1, 127, 128, 1000 };

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                ushort value;
                Assert.True(unpacker.TryReadUInt16(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadInt32()
        {
            var inputs = new[] {int.MinValue, int.MaxValue, 0, 1, -1, 127, -127, 128, -128, 1000, -1000, 12345678, -12345678};

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                int value;
                Assert.True(unpacker.TryReadInt32(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadUInt32()
        {
            var inputs = new uint[] {uint.MinValue, uint.MaxValue, 0, 1, 127, 128, 1000, 12345678};

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                uint value;
                Assert.True(unpacker.TryReadUInt32(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadInt64()
        {
            var inputs = new[] {long.MinValue, long.MaxValue, 0, 1, -1, 127, -127, 128, -128, 1000, -1000, 12345678, -12345678, int.MinValue, int.MaxValue};

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                long value;
                Assert.True(unpacker.TryReadInt64(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadUInt64()
        {
            var inputs = new ulong[] {ulong.MinValue, ulong.MaxValue, 0, 1, 127, 128, 1000, 12345678, int.MaxValue, long.MaxValue};

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                ulong value;
                Assert.True(unpacker.TryReadUInt64(out value), $"Processing {input}");
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
        public void TryReadSingle()
        {
            var inputs = new[] {0.0f, 1.0f, 0.5f, -1.5f, float.NaN, float.MinValue, float.MaxValue, float.PositiveInfinity, float.NegativeInfinity};

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                float value;
                Assert.True(unpacker.TryReadSingle(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadDouble()
        {
            var inputs = new[] {0.0d, 1.0d, 0.5d, -1.5d, double.NaN, double.MinValue, double.MaxValue, double.PositiveInfinity, double.NegativeInfinity};

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.Pack(input));

                double value;
                Assert.True(unpacker.TryReadDouble(out value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadNull()
        {
            var stream = new MemoryStream();

            var packer = MsgPack.Packer.Create(stream, PackerCompatibilityOptions.None);
            packer.PackNull();
            packer.PackNull();
            packer.Pack(1);

            stream.Position = 0;

            var unpacker = new Unpacker(stream);

            Assert.True(unpacker.TryReadNull());
            Assert.True(unpacker.TryReadNull());
            Assert.False(unpacker.TryReadNull());
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
        public void TryReadBinary()
        {
            var inputs = new[]
            {
                null,
                new byte[0],
                new byte[] {1, 2, 3, 4, byte.MaxValue},
                new byte[0xFF],
                new byte[0x100],
                new byte[0xFFFF],
                new byte[0x10000]
            };

            foreach (var input in inputs)
            {
                var unpacker = InitTest(p => p.PackBinary(input));

                byte[] value;
                Assert.True(unpacker.TryReadBinary(out value), $"Processing {(input == null ? "null" : $"[{string.Join(",", input)}]")}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryPeekFormatFamily()
        {
            TestFamily(p => p.PackMapHeader(1),   FormatFamily.Map);
            TestFamily(p => p.PackMapHeader(200), FormatFamily.Map);

            TestFamily(p => p.PackArrayHeader(1),   FormatFamily.Array);
            TestFamily(p => p.PackArrayHeader(200), FormatFamily.Array);

            TestFamily(p => p.Pack(0), FormatFamily.Integer);
            TestFamily(p => p.Pack(1), FormatFamily.Integer);
            TestFamily(p => p.Pack(-1), FormatFamily.Integer);
            TestFamily(p => p.Pack(128), FormatFamily.Integer);
            TestFamily(p => p.Pack(-128), FormatFamily.Integer);
            TestFamily(p => p.Pack(256), FormatFamily.Integer);
            TestFamily(p => p.Pack(int.MaxValue), FormatFamily.Integer);
            TestFamily(p => p.Pack(int.MinValue), FormatFamily.Integer);

            TestFamily(p => p.Pack(0.0f), FormatFamily.Float);
            TestFamily(p => p.Pack(0.0d), FormatFamily.Float);
            TestFamily(p => p.Pack(double.NaN), FormatFamily.Float);
            TestFamily(p => p.Pack(float.NaN), FormatFamily.Float);

            TestFamily(p => p.Pack(true), FormatFamily.Boolean);
            TestFamily(p => p.Pack(false), FormatFamily.Boolean);

            TestFamily(p => p.PackNull(), FormatFamily.Null);

            TestFamily(p => p.Pack("Hello"), FormatFamily.String);
            TestFamily(p => p.Pack(""), FormatFamily.String);
        }

        [Fact]
        public void TryPeekFormat()
        {
            TestFormat(p => p.PackMapHeader(1), Format.FixMap);
            TestFormat(p => p.PackMapHeader(ushort.MaxValue), Format.Map16);
            TestFormat(p => p.PackMapHeader(ushort.MaxValue + 1), Format.Map32);

            TestFormat(p => p.PackArrayHeader(1), Format.FixArray);
            TestFormat(p => p.PackArrayHeader(ushort.MaxValue), Format.Array16);
            TestFormat(p => p.PackArrayHeader(ushort.MaxValue + 1), Format.Array32);

            TestFormat(p => p.Pack(0), Format.PositiveFixInt);
            TestFormat(p => p.Pack(1), Format.PositiveFixInt);
            TestFormat(p => p.Pack(-1), Format.NegativeFixInt);
            TestFormat(p => p.Pack(127), Format.PositiveFixInt);
            TestFormat(p => p.Pack(-127), Format.Int8);
            TestFormat(p => p.Pack(127u), Format.PositiveFixInt);
            TestFormat(p => p.Pack(128u), Format.UInt8);
            TestFormat(p => p.Pack(255u), Format.UInt8);
            TestFormat(p => p.Pack(-128), Format.Int8);
            TestFormat(p => p.Pack(128), Format.Int16);
            TestFormat(p => p.Pack(256), Format.Int16);
            TestFormat(p => p.Pack(256u), Format.UInt16);
            TestFormat(p => p.Pack(short.MaxValue), Format.Int16);
            TestFormat(p => p.Pack(ushort.MaxValue), Format.UInt16);
            TestFormat(p => p.Pack(short.MaxValue + 1), Format.Int32);
            TestFormat(p => p.Pack(ushort.MaxValue + 1u), Format.UInt32);
            TestFormat(p => p.Pack(int.MaxValue + 1L), Format.Int64);
            TestFormat(p => p.Pack(uint.MaxValue + 1UL), Format.UInt64);

            TestFormat(p => p.Pack(0.0f), Format.Float32);
            TestFormat(p => p.Pack(0.0d), Format.Float64);
            TestFormat(p => p.Pack(float.NaN), Format.Float32);
            TestFormat(p => p.Pack(double.NaN), Format.Float64);

            TestFormat(p => p.Pack(true), Format.True);
            TestFormat(p => p.Pack(false), Format.False);

            TestFormat(p => p.PackNull(), Format.Null);

            TestFormat(p => p.Pack("Hello"), Format.FixStr);
            TestFormat(p => p.Pack(new string('!', 255)), Format.Str8);
            TestFormat(p => p.Pack(new string('!', 256)), Format.Str16);
            TestFormat(p => p.Pack(new string('!', ushort.MaxValue + 1)), Format.Str32);

            TestFormat(p => p.Pack(new byte[255]), Format.Bin8);
            TestFormat(p => p.Pack(new byte[256]), Format.Bin16);
            TestFormat(p => p.Pack(new byte[ushort.MaxValue + 1]), Format.Bin32);
        }

        [Fact]
        public void Sequences()
        {
            var stream = new MemoryStream();
            var packer = MsgPack.Packer.Create(stream);

            var unpacker = new Unpacker(stream);
            var random = new Random();
            var sequence = new List<string>();

            Func<Action>[] scenarios =
            {
                // Array Header
                () =>
                {
                    var input = random.Next();
                    packer.PackArrayHeader(input);
                    return () =>
                    {
                        sequence.Add($"Array Header {input}");
                        int output;
                        Assert.True(unpacker.TryReadArrayLength(out output));
                        Assert.Equal(input, output);
                    };
                },
                // Map Header
                () =>
                {
                    var input = random.Next();
                    packer.PackMapHeader(input);
                    return () =>
                    {
                        sequence.Add($"Map Header {input}");
                        int output;
                        Assert.True(unpacker.TryReadMapLength(out output));
                        Assert.Equal(input, output);
                    };
                },
                // SByte
                () =>
                {
                    var input = (sbyte)random.Next();
                    packer.Pack(input);
                    return () =>
                    {
                        sequence.Add($"SByte {input}");
                        sbyte output;
                        Assert.True(unpacker.TryReadSByte(out output));
                        Assert.Equal(input, output);
                    };
                },
                // Int16
                () =>
                {
                    var input = (short)random.Next();
                    packer.Pack(input);
                    return () =>
                    {
                        sequence.Add($"Int16 {input}");
                        short output;
                        Assert.True(unpacker.TryReadInt16(out output));
                        Assert.Equal(input, output);
                    };
                },
                // UInt16
                () =>
                {
                    var input = (ushort)random.Next();
                    packer.Pack(input);
                    return () =>
                    {
                        sequence.Add($"UInt16 {input}");
                        ushort output;
                        Assert.True(unpacker.TryReadUInt16(out output));
                        Assert.Equal(input, output);
                    };
                },
                // Int32
                () =>
                {
                    var input = random.Next();
                    packer.Pack(input);
                    return () =>
                    {
                        sequence.Add($"Int32 {input}");
                        int output;
                        Assert.True(unpacker.TryReadInt32(out output));
                        Assert.Equal(input, output);
                    };
                },
                // UInt32
                () =>
                {
                    var input = (uint)random.Next();
                    packer.Pack(input);
                    return () =>
                    {
                        sequence.Add($"UInt32 {input}");
                        uint output;
                        Assert.True(unpacker.TryReadUInt32(out output));
                        Assert.Equal(input, output);
                    };
                },
                // Int64
                () =>
                {
                    #pragma warning disable CS0675
                    var input = random.Next() | ((long)random.Next() << 32);
                    #pragma warning restore CS0675
                    packer.Pack(input);
                    return () =>
                    {
                        sequence.Add($"Int64 {input}");
                        long output;
                        Assert.True(unpacker.TryReadInt64(out output));
                        Assert.Equal(input, output);
                    };
                },
                // UInt64
                () =>
                {
                    #pragma warning disable CS0675
                    var input = (ulong)random.Next() | ((ulong)random.Next() << 32);
                    #pragma warning restore CS0675
                    packer.Pack(input);
                    return () =>
                    {
                        sequence.Add($"UInt64 {input}");
                        ulong output;
                        Assert.True(unpacker.TryReadUInt64(out output));
                        Assert.Equal(input, output);
                    };
                },
                // Boolean
                () =>
                {
                    var input = random.NextDouble() < 0.5;
                    packer.Pack(input);
                    return () =>
                    {
                        sequence.Add($"Bool {input}");
                        bool output;
                        Assert.True(unpacker.TryReadBoolean(out output));
                        Assert.Equal(input, output);
                    };
                },
                // String
                () =>
                {
                    var input = random.NextDouble() < 0.5 ? "hello" : null;
                    packer.Pack(input);
                    return () =>
                    {
                        sequence.Add($"String {input}");
                        string output;
                        Assert.True(unpacker.TryReadString(out output));
                        Assert.Equal(input, output);
                    };
                },
                // Single
                () =>
                {
                    var input = (float)random.NextDouble();
                    packer.Pack(input);
                    return () =>
                    {
                        sequence.Add($"Single {input}");
                        float output;
                        Assert.True(unpacker.TryReadSingle(out output));
                        Assert.Equal(input, output);
                    };
                },
                // Double
                () =>
                {
                    var input = random.NextDouble();
                    packer.Pack(input);
                    return () =>
                    {
                        sequence.Add($"Double {input}");
                        double output;
                        Assert.True(unpacker.TryReadDouble(out output));
                        Assert.Equal(input, output);
                    };
                }
            };

            var verifiers = Enumerable.Range(0, 10000)
                .Select(_ => scenarios[random.Next()%scenarios.Length]())
                .ToList();

            stream.Position = 0;

            foreach (var verifier in verifiers)
            {
                try
                {
                    verifier();
                }
                catch (XunitException)
                {
                    foreach (var step in sequence.Skip(sequence.Count - 10))
                        Console.Out.WriteLine(step);

                    throw;
                }
            }
        }

        #region Test support

        private static Unpacker InitTest(Action<MsgPack.Packer> packerAction)
        {
            var stream = new MemoryStream();
            packerAction(MsgPack.Packer.Create(stream, PackerCompatibilityOptions.None));
            stream.Position = 0;

            return new Unpacker(stream);
        }

        private static void TestFamily(Action<MsgPack.Packer> packerAction, FormatFamily expected)
        {
            var stream = new MemoryStream();
            packerAction(MsgPack.Packer.Create(stream, PackerCompatibilityOptions.None));
            stream.Position = 0;

            var unpacker = new Unpacker(stream);

            FormatFamily actual;
            Assert.True(unpacker.TryPeekFormatFamily(out actual));
            Assert.Equal(expected, actual);
        }

        private static void TestFormat(Action<MsgPack.Packer> packerAction, Format expected)
        {
            var stream = new MemoryStream();
            packerAction(MsgPack.Packer.Create(stream, PackerCompatibilityOptions.None));
            stream.Position = 0;

            var unpacker = new Unpacker(stream);

            Format actual;
            Assert.True(unpacker.TryPeekFormat(out actual));
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
