using System;
using System.IO;
using MsgPack;
using Xunit;

namespace Dasher.Tests
{
    // TODO mismatch between ctor args and properties (?)

    public sealed class DeserialiserTests
    {
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
        public void HandlesDateTime()
        {
            var dateTime = new DateTime(2015, 12, 25);

            var bytes = PackBytes(packer =>
            {
                packer.PackMapHeader(1)
                    .Pack("Date").Pack(dateTime.Ticks);
            });

            var after = new Deserialiser<WithDateTimeProperty>().Deserialise(bytes);

            Assert.Equal(dateTime, after.Date);
        }

        [Fact]
        public void HandlesTimeSpan()
        {
            var timeSpan = TimeSpan.FromSeconds(1234.5678);

            var bytes = PackBytes(packer =>
            {
                packer.PackMapHeader(1)
                    .Pack("Time").Pack(timeSpan.Ticks);
            });

            var after = new Deserialiser<WithTimeSpanProperty>().Deserialise(bytes);

            Assert.Equal(timeSpan, after.Time);
        }

        [Fact]
        public void HandlesIntPtr()
        {
            var intPtr = new IntPtr(12345678);

            var bytes = PackBytes(packer =>
            {
                packer.PackMapHeader(1)
                    .Pack("IntPtr").Pack(intPtr.ToInt64());
            });

            var after = new Deserialiser<WithIntPtrProperty>().Deserialise(bytes);

            Assert.Equal(intPtr, after.IntPtr);
        }

        [Fact]
        public void HandlesVersion()
        {
            var version = new Version("1.2.3");

            var bytes = PackBytes(packer =>
            {
                packer.PackMapHeader(1)
                    .Pack("Version").Pack(version.ToString());
            });

            var after = new Deserialiser<WithVersionProperty>().Deserialise(bytes);

            Assert.Equal(version, after.Version);
        }

        [Fact]
        public void HandlesGuid()
        {
            var guid = new Guid();

            var bytes = PackBytes(packer =>
            {
                packer.PackMapHeader(1)
                    .Pack("Guid").Pack(guid.ToString());
            });

            var after = new Deserialiser<WithGuidProperty>().Deserialise(bytes);

            Assert.Equal(guid, after.Guid);
        }

        [Fact]
        public void HandlesNullableValueTypes()
        {
            var bytes = PackBytes(packer =>
            {
                packer.PackMapHeader(4)
                    .Pack("Int").PackNull()
                    .Pack("Double").PackNull()
                    .Pack("DateTime").PackNull()
                    .Pack("Decimal").PackNull();
            });

            var after = new Deserialiser<WithNullableProperties>().Deserialise(bytes);

            Assert.Null(after.Int);
            Assert.Null(after.Double);
            Assert.Null(after.DateTime);
            Assert.Null(after.Decimal);
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
