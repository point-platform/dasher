using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MsgPack.Strict.Tests
{
    // TODO mismatch between ctor args and properties (?)

    public sealed class StrictDeserialiserTests
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

        #endregion

        [Fact]
        public void ExactMatch()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123));

            var after = StrictDeserialiser.Get<UserScore>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void HandlesDecimalProperty()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack("123.4567"));

            var after = StrictDeserialiser.Get<UserScoreDecimal>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123.4567m, after.Score);
        }

        [Fact]
        public void DeserialiseToStruct()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123));

            var after = StrictDeserialiser.Get<UserScoreStruct>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void ReorderedFields()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Score").Pack(123)
                .Pack("Name").Pack("Bob"));

            var after = StrictDeserialiser.Get<UserScore>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void MixedUpCapitalisation()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(2)
                .Pack("NaMe").Pack("Bob")
                .Pack("ScorE").Pack(123));

            var after = StrictDeserialiser.Get<UserScore>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void ThrowsOnUnexpectedField()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(3)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123)
                .Pack("SUPRISE").Pack("Unexpected"));

            var deserialiser = StrictDeserialiser.Get<UserScore>();
            var ex = Assert.Throws<StrictDeserialisationException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Encountered unexpected field \"SUPRISE\".", ex.Message);
        }

        [Fact]
        public void ThrowsOnMissingField()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(1)
                .Pack("Name").Pack("Bob"));

            var deserialiser = StrictDeserialiser.Get<UserScore>();
            var ex = Assert.Throws<StrictDeserialisationException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Missing required field \"score\".", ex.Message);
        }

        [Fact]
        public void ThrowsOnIncorrectDataType()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123.4)); // double, should be int

            var deserialiser = StrictDeserialiser.Get<UserScore>();
            var ex = Assert.Throws<StrictDeserialisationException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Unexpected type for \"score\". Expected Int32, got Float64.", ex.Message);
        }

        [Fact]
        public void ThrowsOnDuplicateField()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(3)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123)
                .Pack("Score").Pack(321));

            var deserialiser = StrictDeserialiser.Get<UserScore>();
            var ex = Assert.Throws<StrictDeserialisationException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Encountered duplicate field \"Score\".", ex.Message);
        }

        [Fact]
        public void ThrowsOnNonMapData()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackArrayHeader(2)
                .Pack("Name").Pack(123));

            var deserialiser = StrictDeserialiser.Get<UserScore>();
            var ex = Assert.Throws<StrictDeserialisationException>(() => deserialiser.Deserialise(bytes));
            Assert.Equal("Message must be encoded as a MsgPack map", ex.Message);
            Assert.Equal(typeof(UserScore), ex.TargetType);
        }

        [Fact]
        public void ThrowsOnEmptyData()
        {
            var bytes = new byte[0];

            var deserialiser = StrictDeserialiser.Get<UserScore>();
            var ex = Assert.Throws<StrictDeserialisationException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Data stream empty", ex.Message);
        }

        [Fact]
        public void HandlesEnumPropertiesCorrectly()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(1)
                .Pack("TestEnum").Pack("Bar"));

            var after = StrictDeserialiser.Get<WithEnumProperty>().Deserialise(bytes);

            Assert.Equal(TestEnum.Bar, after.TestEnum);
        }

        [Fact]
        public void DeserialisesEnumMembersCaseInsensitively()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(1)
                .Pack("TestEnum").Pack("BAR"));

            var after = StrictDeserialiser.Get<WithEnumProperty>().Deserialise(bytes);

            Assert.Equal(TestEnum.Bar, after.TestEnum);
        }

        [Fact]
        public void ThrowsWhenEnumNotEncodedAsString()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(1)
                .Pack("TestEnum").Pack(123));

            var ex = Assert.Throws<StrictDeserialisationException>(
                () => StrictDeserialiser.Get<WithEnumProperty>().Deserialise(bytes));

            Assert.Equal(typeof(WithEnumProperty), ex.TargetType);
            Assert.Equal($"Unable to read string value for enum property testEnum of type {typeof(TestEnum)}", ex.Message);
        }

        [Fact]
        public void ThrowsWhenEnumStringNotValidMember()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(1)
                .Pack("TestEnum").Pack("Rubbish"));

            var ex = Assert.Throws<StrictDeserialisationException>(
                () => StrictDeserialiser.Get<WithEnumProperty>().Deserialise(bytes));

            Assert.Equal(typeof(WithEnumProperty), ex.TargetType);
            Assert.Equal($"Unable to parse value \"Rubbish\" as a member of enum type {typeof(TestEnum)}", ex.Message);
        }

        [Fact]
        public void UsesDefaultValuesIfNotInMessage()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(0));

            var deserialiser = StrictDeserialiser.Get<TestDefaultParams>();
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
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(12345)); // score has a default of 100

            var deserialiser = StrictDeserialiser.Get<UserScoreWithDefaultScore>();
            var after = deserialiser.Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(12345, after.Score);
        }

        [Fact]
        public void ThrowsOnMultipleConstructors()
        {
            var ex = Assert.Throws<StrictDeserialisationException>(
                () => StrictDeserialiser.Get<MultipleConstructors>());
            Assert.Equal("Type must have a single public constructor.", ex.Message);
        }

        [Fact]
        public void ThrowsNoPublicConstructors()
        {
            var ex = Assert.Throws<StrictDeserialisationException>(
                () => StrictDeserialiser.Get<NoPublicConstructors>());
            Assert.Equal("Type must have a single public constructor.", ex.Message);
        }

        [Fact]
        public void HandlesNestedComplexTypes()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Weight").Pack(0.5d)
                .Pack("UserScore").PackMapHeader(2)
                    .Pack("Name").Pack("Bob")
                    .Pack("Score").Pack(123));

            var after = StrictDeserialiser.Get<UserScoreWrapper>().Deserialise(bytes);

            Assert.Equal(0.5d, after.Weight);
            Assert.Equal("Bob", after.UserScore.Name);
            Assert.Equal(123, after.UserScore.Score);
        }

        [Fact]
        public void HandlesReadOnlyListProperty()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Scores").PackArrayHeader(3).Pack(1).Pack(2).Pack(3));

            var after = StrictDeserialiser.Get<UserScoreList>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(new[] {1, 2, 3}, after.Scores);
        }

        [Fact]
        public void HandlesListOfListProperty()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(1)
                .Pack("Jagged").PackArrayHeader(2)
                    .PackArrayHeader(3).Pack(1).Pack(2).Pack(3)
                    .PackArrayHeader(3).Pack(4).Pack(5).Pack(6));

            var after = StrictDeserialiser.Get<ListOfList>().Deserialise(bytes);

            Assert.Equal(2, after.Jagged.Count);
            Assert.Equal(new[] {1, 2, 3}, after.Jagged[0]);
            Assert.Equal(new[] {4, 5, 6}, after.Jagged[1]);
        }
    }
}
