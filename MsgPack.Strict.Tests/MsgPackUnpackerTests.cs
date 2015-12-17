using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Sdk;

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
        public void Sequences()
        {
            var stream = new MemoryStream();
            var packer = Packer.Create(stream);

            var unpacker = new MsgPackUnpacker(stream);
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
                    var input = random.Next() | ((long)random.Next() << 32);
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
                    var input = (ulong)random.Next() | ((ulong)random.Next() << 32);
                    packer.Pack(input);
                    return () =>
                    {
                        sequence.Add($"UInt64 {input}");
                        ulong output;
                        Assert.True(unpacker.TryReadUInt64(out output));
                        Assert.Equal(input, output);
                    };
                },
                // Bool
                () =>
                {
                    var input = random.NextDouble() < 0.5;
                    packer.Pack(input);
                    return () =>
                    {
                        sequence.Add($"Bool {input}");
                        bool output;
                        Assert.True(unpacker.TryReadBool(out output));
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
/*
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
*/
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
