using System.Collections.Generic;
using System.IO;
using Xunit;

namespace MsgPack.Strict.Tests
{
    public sealed class StrictSerialiserTests
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
        public void SerialisesProperties()
        {
            var after = RoundTrip(new UserScore("Bob", 123));

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void SerialisesStruct()
        {
            var after = RoundTrip(new UserScoreStruct("Bob", 123));

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        // TODO decimal
        // TODO complex
        // TODO IROL

        #region Test helpers

        private static T RoundTrip<T>(T before)
        {
            var stream = new MemoryStream();

            StrictSerialiser.Get<T>().Serialise(stream, before);

            stream.Position = 0;

            return StrictDeserialiser.Get<T>().Deserialise(stream.ToArray());
        }

        #endregion
    }
}