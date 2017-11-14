#region License
//
// Dasher
//
// Copyright 2015-2017 Drew Noakes
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MsgPack;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Dasher.Tests
{
    public sealed class UnpackerTests
    {
        private static readonly float[] _testFloats = {0.0f, 1.0f, 0.5f, -1.5f, (float)Math.PI, float.NaN, float.MinValue, float.MaxValue, float.PositiveInfinity, float.NegativeInfinity};
        private static readonly double[] _testDoubles = {0.0d, 1.0d, 0.5d, -1.5d, Math.PI, double.NaN, double.MinValue, double.MaxValue, double.PositiveInfinity, double.NegativeInfinity};

        private readonly ITestOutputHelper _output;

        public UnpackerTests(ITestOutputHelper output) => _output = output;

        [Fact]
        public void TryReadByte()
        {
            foreach (var input in Enumerable.Range(0, 255).Select(i => (byte)i))
            {
                var unpacker = InitTest(p => p.Pack(input));

                Assert.True(unpacker.TryReadByte(out byte value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadSByte()
        {
            foreach (var input in Enumerable.Range(sbyte.MinValue, 255).Select(i => (sbyte)i))
            {
                var unpacker = InitTest(p => p.Pack(input));

                Assert.True(unpacker.TryReadSByte(out sbyte value), $"Processing {input}");
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

                Assert.True(unpacker.TryReadInt16(out short value), $"Processing {input}");
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

                Assert.True(unpacker.TryReadUInt16(out ushort value), $"Processing {input}");
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

                Assert.True(unpacker.TryReadInt32(out int value), $"Processing {input}");
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

                Assert.True(unpacker.TryReadUInt32(out uint value), $"Processing {input}");
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

                Assert.True(unpacker.TryReadInt64(out long value), $"Processing {input}");
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

                Assert.True(unpacker.TryReadUInt64(out ulong value), $"Processing {input}");
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

                Assert.True(unpacker.TryReadString(out string value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadSingle()
        {
            foreach (var input in _testFloats)
            {
                var unpacker = InitTest(p => p.Pack(input));

                Assert.True(unpacker.TryReadSingle(out float value), $"Processing {input}");
                Assert.Equal(input, value);
            }
        }

        [Fact]
        public void TryReadDouble()
        {
            foreach (var input in _testDoubles)
            {
                var unpacker = InitTest(p => p.Pack(input));

                Assert.True(unpacker.TryReadDouble(out double value), $"Processing {input}");
                Assert.Equal(input, value);
            }

            // Float can be losslessly widened to double, by IEEE spec
            foreach (var input in _testFloats)
            {
                var unpacker = InitTest(p => p.Pack(input));

                Assert.True(unpacker.TryReadDouble(out double value), $"Processing {input}");
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

                Assert.True(unpacker.TryReadArrayLength(out int value), $"Processing {input}");
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

                Assert.True(unpacker.TryReadMapLength(out int value), $"Processing {input}");
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

                Assert.True(unpacker.TryReadBinary(out byte[] value), $"Processing {(input == null ? "null" : $"[{string.Join(",", input)}]")}");
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
        public void TryPeekEmptyMap()
        {
            TestEmptyMap(true, packer => packer.PackMapHeader(0));

            for (var i = 1; i < 1024; i++)
                TestEmptyMap(false, packer => packer.PackMapHeader(i));

            TestEmptyMap(false, packer => packer.PackNull());
            TestEmptyMap(false, packer => packer.PackArrayHeader(1));
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
                        Assert.True(unpacker.TryReadArrayLength(out int output));
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
                        Assert.True(unpacker.TryReadMapLength(out int output));
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
                        Assert.True(unpacker.TryReadSByte(out sbyte output));
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
                        Assert.True(unpacker.TryReadInt16(out short output));
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
                        Assert.True(unpacker.TryReadUInt16(out ushort output));
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
                        Assert.True(unpacker.TryReadInt32(out int output));
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
                        Assert.True(unpacker.TryReadUInt32(out uint output));
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
                        Assert.True(unpacker.TryReadInt64(out long output));
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
                        Assert.True(unpacker.TryReadUInt64(out ulong output));
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
                        Assert.True(unpacker.TryReadBoolean(out bool output));
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
                        Assert.True(unpacker.TryReadString(out string output));
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
                        Assert.True(unpacker.TryReadSingle(out float output));
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
                        Assert.True(unpacker.TryReadDouble(out double output));
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
                        _output.WriteLine(step);

                    throw;
                }
            }
        }

        [Fact]
        public void SkipValue()
        {
            var stream = new MemoryStream();
            var packer = new Packer(stream);

            var unpacker = new Unpacker(stream);
            var random = new Random();

            void PackArray(int count)
            {
                packer.PackArrayHeader((uint)count);
                for (var i = 0; i < count; i++)
                    packer.PackNull();
            }

            void PackMap(int count)
            {
                packer.PackMapHeader((uint)count);
                for (var i = 0; i < count; i++)
                {
                    packer.PackNull();
                    packer.PackNull();
                }
            }

            Action[] scenarios =
            {
                // Array
                () => PackArray(0),
                () => PackArray(1),
                () => PackArray(10),
                () => PackArray(127),
                () => PackArray(255),
                () => PackArray(1024),
                // Map
                () => PackMap(0),
                () => PackMap(1),
                () => PackMap(10),
                () => PackMap(127),
                () => PackMap(255),
                () => PackMap(1024),
                // SByte
                () => packer.Pack((sbyte)random.Next()),
                // Int16
                () => packer.Pack((short)random.Next()),
                // UInt16
                () => packer.Pack((ushort)random.Next()),
                // Int32
                () => packer.Pack(random.Next()),
                // UInt32
                () => packer.Pack((uint)random.Next()),
                // Int64
                #pragma warning disable CS0675
                () => packer.Pack(random.Next() | ((long)random.Next() << 32)),
                #pragma warning restore CS0675
                // UInt64
                #pragma warning disable CS0675
                () => packer.Pack((ulong)random.Next() | ((ulong)random.Next() << 32)),
                #pragma warning restore CS0675
                // Boolean
                () => packer.Pack(true),
                () => packer.Pack(false),
                // Null
                () => packer.PackNull(),
                // String
                () => packer.Pack(new string('A', 0)),
                () => packer.Pack(new string('A', 10)),
                () => packer.Pack(new string('A', 127)),
                () => packer.Pack(new string('A', 255)),
                () => packer.Pack(new string('A', 1024)),
                // Single
                () => packer.Pack((float)random.NextDouble()),
                // Double
                () => packer.Pack(random.NextDouble())
            };

            foreach (var scenario in scenarios)
            {
                stream.Position = 0;

                // Pack the first sentinel
                packer.Pack(float.NaN);

                // Run the packing scenario
                scenario();

                // Flush the packer to the stream
                packer.Flush();

                // Remember the packed byte count
                var packedByteCount = stream.Position;

                // Pack the second sentinel
                packer.Pack(float.NaN);

                // Flush the packer to the stream
                packer.Flush();

                // Reset the stream position
                stream.Position = 0;

                // Catch exceptions, log the current format, then rethrow
                var format = Format.Unknown;
                try
                {
                    // Read the first sentinel
                    Assert.True(unpacker.TryReadSingle(out float value));
                    Assert.True(float.IsNaN(value));

                    // Peek at the value's format
                    Assert.True(unpacker.TryPeekFormat(out format));

                    // Perform the skip
                    unpacker.SkipValue();

                    // Ensure same number of bytes packed and unpacked
                    Assert.Equal(packedByteCount, stream.Position);

                    // Read the second sentinel
                    Assert.True(unpacker.TryReadSingle(out value));
                    Assert.True(float.IsNaN(value));
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Error with format: {format}");
                    _output.WriteLine(ex.ToString());
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

            Assert.True(unpacker.TryPeekFormatFamily(out FormatFamily actual));
            Assert.Equal(expected, actual);
        }

        private static void TestFormat(Action<MsgPack.Packer> packerAction, Format expected)
        {
            var stream = new MemoryStream();
            packerAction(MsgPack.Packer.Create(stream, PackerCompatibilityOptions.None));
            stream.Position = 0;

            var unpacker = new Unpacker(stream);

            Assert.True(unpacker.TryPeekFormat(out Format actual));
            Assert.Equal(expected, actual);
        }

        private static void TestEmptyMap(bool expected, Action<MsgPack.Packer> packerAction)
        {
            var stream = new MemoryStream();
            packerAction(MsgPack.Packer.Create(stream, PackerCompatibilityOptions.None));
            stream.Position = 0;

            var unpacker = new Unpacker(stream);

            Assert.Equal(expected, unpacker.TryPeekEmptyMap());
        }

        #endregion
    }
}
