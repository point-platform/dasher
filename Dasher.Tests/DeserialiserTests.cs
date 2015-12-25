using System;
using System.Collections.Generic;
using System.IO;
using MsgPack;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dasher.Tests
{
    // TODO mismatch between ctor args and properties (?)

    public sealed class DeserialiserTests
    {
        #region Test Types

        public sealed class UserScore
        {
            public UserScore(string name, int score)
            {
                Name = name;
                Score = score;
            }

            public string Name { get; }
            public int Score { get; }
        }

        public struct UserScoreStruct
        {
            public UserScoreStruct(string name, int score)
            {
                Name = name;
                Score = score;
            }

            public string Name { get; }
            public int Score { get; }
        }

        public sealed class UserScoreWithDefaultScore
        {
            public UserScoreWithDefaultScore(string name, int score = 100)
            {
                Name = name;
                Score = score;
            }

            public string Name { get; }
            public int Score { get; }
        }

        public sealed class UserScoreWrapper
        {
            public double Weight { get; }
            public UserScore UserScore { get; }

            public UserScoreWrapper(double weight, UserScore userScore)
            {
                Weight = weight;
                UserScore = userScore;
            }
        }

        public sealed class UserScoreDecimal
        {
            public UserScoreDecimal(string name, decimal score)
            {
                Name = name;
                Score = score;
            }

            public string Name { get; }
            public decimal Score { get; }
        }

        public enum TestEnum
        {
            Foo = 1,
            Bar = 2
        }

        public sealed class WithEnumProperty
        {
            public WithEnumProperty(TestEnum testEnum)
            {
                TestEnum = testEnum;
            }

            public TestEnum TestEnum { get; }
        }

        public sealed class TestDefaultParams
        {
            public byte      B       { get; }
            public sbyte     Sb      { get; }
            public short     S       { get; }
            public ushort    Us      { get; }
            public int       I       { get; }
            public uint      Ui      { get; }
            public long      L       { get; }
            public ulong     Ul      { get; }
            public string    Str     { get; }
            public float     F       { get; }
            public double    D       { get; }
            public decimal   Dc      { get; }
            public bool      Bo      { get; }
            public TestEnum  E       { get; }
            public UserScore Complex { get; }

            public TestDefaultParams(
                sbyte sb = -12,
                byte b = 12,
                short s = -1234,
                ushort us = 1234,
                int i = -12345,
                uint ui = 12345,
                long l = -12345678900L,
                ulong ul = 12345678900UL,
                string str = "str",
                float f = 1.23f,
                double d = 1.23,
                decimal dc = 1.23M,
                bool bo = true,
                TestEnum e = TestEnum.Bar,
                UserScore complex = null)
            {
                B = b;
                Sb = sb;
                S = s;
                Us = us;
                I = i;
                Ui = ui;
                L = l;
                Ul = ul;
                Str = str;
                F = f;
                D = d;
                Dc = dc;
                Bo = bo;
                E = e;
                Complex = complex;
            }
        }

        public sealed class MultipleConstructors
        {
            public int Number { get; }
            public string Text { get; }

            public MultipleConstructors(int number, string text)
            {
                Number = number;
                Text = text;
            }

            public MultipleConstructors(int number)
            {
                Number = number;
            }
        }

        public sealed class NoPublicConstructors
        {
            public int Number { get; }

            internal NoPublicConstructors(int number)
            {
                Number = number;
            }
        }

        public sealed class UserScoreList
        {
            public UserScoreList(string name, IReadOnlyList<int> scores)
            {
                Name = name;
                Scores = scores;
            }

            public string Name { get; }
            public IReadOnlyList<int> Scores { get; }
        }

        public sealed class ListOfList
        {
            public IReadOnlyList<IReadOnlyList<int>> Jagged { get; }

            public ListOfList(IReadOnlyList<IReadOnlyList<int>> jagged)
            {
                Jagged = jagged;
            }
        }

        public sealed class WithBinary
        {
            public byte[] Bytes { get; }

            public WithBinary(byte[] bytes)
            {
                Bytes = bytes;
            }
        }

        #endregion

        [Fact]
        public void ExactMatch()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123));

            var after = new Deserialiser<UserScore>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void HandlesDecimal()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack("123.4567"));

            var after = new Deserialiser<UserScoreDecimal>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123.4567m, after.Score);
        }

        [Fact]
        public void DeserialiseToStruct()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123));

            var after = new Deserialiser<UserScoreStruct>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void ReorderedFields()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Score").Pack(123)
                .Pack("Name").Pack("Bob"));

            var after = new Deserialiser<UserScore>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void MixedUpCapitalisation()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(2)
                .Pack("NaMe").Pack("Bob")
                .Pack("ScorE").Pack(123));

            var after = new Deserialiser<UserScore>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void ThrowsOnUnexpectedField()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(3)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123)
                .Pack("SUPRISE").Pack("Unexpected"));

            var deserialiser = new Deserialiser<UserScore>();
            var ex = Assert.Throws<DeserialisationException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Encountered unexpected field \"SUPRISE\".", ex.Message);
        }

        [Fact]
        public void IgnoresUnexpectedField()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(3)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123)
                .Pack("SUPRISE").Pack("Unexpected"));

            var deserialiser = new Deserialiser<UserScore>(UnexpectedFieldBehaviour.Ignore);
            var after = deserialiser.Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void ThrowsOnMissingField()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack("Name").Pack("Bob"));

            var deserialiser = new Deserialiser<UserScore>();
            var ex = Assert.Throws<DeserialisationException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Missing required field \"score\".", ex.Message);
        }

        [Fact]
        public void ThrowsOnIncorrectDataType()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123.4)); // double, should be int

            var deserialiser = new Deserialiser<UserScore>();
            var ex = Assert.Throws<DeserialisationException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Unexpected type for \"score\". Expected Int32, got Float64.", ex.Message);
        }

        [Fact]
        public void ThrowsOnDuplicateField()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(3)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123)
                .Pack("Score").Pack(321));

            var deserialiser = new Deserialiser<UserScore>();
            var ex = Assert.Throws<DeserialisationException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Encountered duplicate field \"Score\".", ex.Message);
        }

        [Fact]
        public void ThrowsOnNonMapData()
        {
            var bytes = PackBytes(packer => packer.PackArrayHeader(2)
                .Pack("Name").Pack(123));

            var deserialiser = new Deserialiser<UserScore>();
            var ex = Assert.Throws<DeserialisationException>(() => deserialiser.Deserialise(bytes));
            Assert.Equal("Message must be encoded as a MsgPack map", ex.Message);
            Assert.Equal(typeof(UserScore), ex.TargetType);
        }

        [Fact]
        public void ThrowsOnEmptyData()
        {
            var bytes = new byte[0];

            var deserialiser = new Deserialiser<UserScore>();
            var ex = Assert.Throws<DeserialisationException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Data stream empty", ex.Message);
        }

        [Fact]
        public void HandlesEnumPropertiesCorrectly()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack("TestEnum").Pack("Bar"));

            var after = new Deserialiser<WithEnumProperty>().Deserialise(bytes);

            Assert.Equal(TestEnum.Bar, after.TestEnum);
        }

        [Fact]
        public void DeserialisesEnumMembersCaseInsensitively()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack("TestEnum").Pack("BAR"));

            var after = new Deserialiser<WithEnumProperty>().Deserialise(bytes);

            Assert.Equal(TestEnum.Bar, after.TestEnum);
        }

        [Fact]
        public void ThrowsWhenEnumNotEncodedAsString()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack("TestEnum").Pack(123));

            var ex = Assert.Throws<DeserialisationException>(
                () => new Deserialiser<WithEnumProperty>().Deserialise(bytes));

            Assert.Equal(typeof(WithEnumProperty), ex.TargetType);
            Assert.Equal($"Unable to read string value for enum property testEnum of type {typeof(TestEnum)}", ex.Message);
        }

        [Fact]
        public void ThrowsWhenEnumStringNotValidMember()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack("TestEnum").Pack("Rubbish"));

            var ex = Assert.Throws<DeserialisationException>(
                () => new Deserialiser<WithEnumProperty>().Deserialise(bytes));

            Assert.Equal(typeof(WithEnumProperty), ex.TargetType);
            Assert.Equal($"Unable to parse value \"Rubbish\" as a member of enum type {typeof(TestEnum)}", ex.Message);
        }

        [Fact]
        public void UsesDefaultValuesIfNotInMessage()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(0));

            var deserialiser = new Deserialiser<TestDefaultParams>();
            var after = deserialiser.Deserialise(bytes);

            Assert.Equal(-12, after.Sb);
            Assert.Equal(12, after.B);
            Assert.Equal(-1234, after.S);
            Assert.Equal(1234, after.Us);
            Assert.Equal(-12345, after.I);
            Assert.Equal(12345u, after.Ui);
            Assert.Equal(-12345678900L, after.L);
            Assert.Equal(12345678900UL, after.Ul);
            Assert.Equal("str", after.Str);
            Assert.Equal(1.23f, after.F);
            Assert.Equal(1.23, after.D);
            Assert.Equal(1.23M, after.Dc);
            Assert.Equal(true, after.Bo);
            Assert.Equal(null, after.Complex);
        }

        [Fact]
        public void SpecifiedValueOverridesDefaultValue()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(12345)); // score has a default of 100

            var deserialiser = new Deserialiser<UserScoreWithDefaultScore>();
            var after = deserialiser.Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(12345, after.Score);
        }

        [Fact]
        public void ThrowsOnMultipleConstructors()
        {
            var ex = Assert.Throws<DeserialisationException>(
                () => new Deserialiser<MultipleConstructors>());
            Assert.Equal("Type must have a single public constructor.", ex.Message);
        }

        [Fact]
        public void ThrowsNoPublicConstructors()
        {
            var ex = Assert.Throws<DeserialisationException>(
                () => new Deserialiser<NoPublicConstructors>());
            Assert.Equal("Type must have a single public constructor.", ex.Message);
        }

        [Fact]
        public void HandlesNestedComplexTypes()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Weight").Pack(0.5d)
                .Pack("UserScore").PackMapHeader(2)
                    .Pack("Name").Pack("Bob")
                    .Pack("Score").Pack(123));

            var after = new Deserialiser<UserScoreWrapper>().Deserialise(bytes);

            Assert.Equal(0.5d, after.Weight);
            Assert.Equal("Bob", after.UserScore.Name);
            Assert.Equal(123, after.UserScore.Score);
        }

        [Fact]
        public void HandlesReadOnlyListProperty()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Scores").PackArrayHeader(3).Pack(1).Pack(2).Pack(3));

            var after = new Deserialiser<UserScoreList>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(new[] {1, 2, 3}, after.Scores);
        }

        [Fact]
        public void HandlesListOfListProperty()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack("Jagged").PackArrayHeader(2)
                    .PackArrayHeader(3).Pack(1).Pack(2).Pack(3)
                    .PackArrayHeader(3).Pack(4).Pack(5).Pack(6));

            var after = new Deserialiser<ListOfList>().Deserialise(bytes);

            Assert.Equal(2, after.Jagged.Count);
            Assert.Equal(new[] {1, 2, 3}, after.Jagged[0]);
            Assert.Equal(new[] {4, 5, 6}, after.Jagged[1]);
        }

        [Fact]
        public void HandlesBinary()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack("Bytes").PackBinary(new byte[] {1,2,3,4}));

            var after = new Deserialiser<WithBinary>().Deserialise(bytes);

            Assert.Equal(new byte[] {1, 2, 3, 4}, after.Bytes);
        }

        #region Helper

        private static byte[] PackBytes(Action<MsgPack.Packer> packAction)
        {
            var stream = new MemoryStream();
            var packer = MsgPack.Packer.Create(stream, PackerCompatibilityOptions.None);
            packAction(packer);
            stream.Position = 0;
            return stream.GetBuffer();
        }

        #endregion
    }
}
