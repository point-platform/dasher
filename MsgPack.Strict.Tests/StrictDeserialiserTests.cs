using Xunit;

namespace MsgPack.Strict.Tests
{
    // TODO no constructor
    // TODO multiple constructors
    // TODO class/ctor private
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

        public class TestDefaultParams
        {
            public byte    B   { get; }
            public sbyte   Sb  { get; }
            public short   S   { get; }
            public ushort  Us  { get; }
            public int     I   { get; }
            public uint    Ui  { get; }
            public long    L   { get; }
            public ulong   Ul  { get; }
            public string  Str { get; }
            public float   F   { get; }
            public double  D   { get; }
            public decimal Dc  { get; }
            public bool    Bo  { get; }
            public object  O   { get; }

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
                object o = null)
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
                O = o;
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
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123)
                .Pack("SUPRISE").Pack("Unexpected"));

            var deserialiser = StrictDeserialiser.Get<UserScore>();
            var ex = Assert.Throws<StrictDeserialisationException>(() => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Encountered unexpected field \"SUPRISE\".", ex.Message);
        }

        [Fact]
        public void ThrowsOnDuplicateField()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123)
                .Pack("Score").Pack(321));

            var deserialiser = StrictDeserialiser.Get<UserScore>();
            var ex = Assert.Throws<StrictDeserialisationException>(() => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Duplicate field \"Score\".", ex.Message);
        }

        [Fact]
        public void ThrowsOnNonMapData()
        {
            var bytes = TestUtil.PackBytes(packer => packer.PackArrayHeader(2)
                .Pack("Name").Pack(123));

            var deserialiser = StrictDeserialiser.Get<UserScore>();
            Assert.Throws<MessageTypeException>(() => deserialiser.Deserialise(bytes));
        }

        [Fact]
        public void ThrowsOnEmptyData()
        {
            var bytes = new byte[0];

            var deserialiser = StrictDeserialiser.Get<UserScore>();
            var ex = Assert.Throws<StrictDeserialisationException>(() => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Data stream ended.", ex.Message);
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
            Assert.Equal(null, after.O);
        }
    }
}
