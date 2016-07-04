using System.IO;
using Xunit;

namespace Dasher.Tests
{
    public class RoundTripTests
    {
        [Fact]
        public void Class()
        {
            var after = RoundTrip(new UserScore("Bob", 123));

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void Struct()
        {
            var after = RoundTrip(new UserScoreStruct("Bob", 123));

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void NestedClass()
        {
            var after = RoundTrip(new WeightedUserScore(1.0, new UserScore("Bob", 123)));

            Assert.Equal(1.0, after.Weight);
            Assert.Equal("Bob", after.UserScore.Name);
            Assert.Equal(123, after.UserScore.Score);
        }

        [Fact]
        public void NestedStruct()
        {
            var after = RoundTrip(new ValueWrapper<UserScoreStruct>(new UserScoreStruct("Foo", 123)));

            Assert.Equal("Foo", after.Value.Name);
            Assert.Equal(123, after.Value.Score);
        }

        [Fact]
        public void ListOfList()
        {
            var after = RoundTrip(new ListOfList(new[] { new[] { 1, 2, 3 }, new[] { 4, 5, 6 } }));

            Assert.Equal(2, after.Jagged.Count);
            Assert.Equal(new[] { 1, 2, 3 }, after.Jagged[0]);
            Assert.Equal(new[] { 4, 5, 6 }, after.Jagged[1]);
        }

        #region Helper

        private static T RoundTrip<T>(T before)
        {
            var stream = new MemoryStream();

            new Serialiser<T>().Serialise(stream, before);

            stream.Position = 0;

            return new Deserialiser<T>().Deserialise(stream.ToArray());
        }

        #endregion
    }
}