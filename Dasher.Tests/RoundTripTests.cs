#region License
//
// Dasher
//
// Copyright 2015-2016 Drew Noakes
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