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

using System;
using System.IO;
using Xunit;

namespace Dasher.Tests
{
    public sealed class SerialiserTests
    {
        [Fact]
        public void SerialisesClass()
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

        [Fact]
        public void DisallowsPrimitiveTypes()
        {
            var exception = Assert.Throws<SerialisationException>(() => new Serialiser<int>());
            Assert.Equal("Cannot serialise primitive types. The root type must contain properties and values to support future versioning.", exception.Message);
        }

        [Fact]
        public void HandlesComplex()
        {
            var after = RoundTrip(new WeightedUserScore(1.0, new UserScore("Bob", 123)));

            Assert.Equal(1.0, after.Weight);
            Assert.Equal("Bob", after.UserScore.Name);
            Assert.Equal(123, after.UserScore.Score);
        }

        [Fact]
        public void HandlesListOfList()
        {
            var after = RoundTrip(new ListOfList(new [] {new [] {1, 2, 3}, new [] {4, 5, 6}}));

            Assert.Equal(2, after.Jagged.Count);
            Assert.Equal(new[] {1, 2, 3}, after.Jagged[0]);
            Assert.Equal(new[] {4, 5, 6}, after.Jagged[1]);
        }

        [Fact]
        public void HandlesRecurringType()
        {
            var serialiser = new Serialiser<Recurring>();
            serialiser.Serialise(new Recurring(1, null));
            serialiser.Serialise(new Recurring(1, new Recurring(2, null)));
        }

        [Fact]
        public void HandlesRecurringTreeType()
        {
            var serialiser = new Serialiser<RecurringTree>();
            serialiser.Serialise(new RecurringTree(1, new [] {new RecurringTree(2, null), new RecurringTree(3, null) }));
            serialiser.Serialise(new RecurringTree(1, new RecurringTree[] { null, null }));
        }

        [Fact]
        public void HandlesClassWrappingCustomStruct()
        {
            var after = RoundTrip(new ValueWrapper<UserScoreStruct>(new UserScoreStruct("Foo", 123)));

            Assert.Equal("Foo", after.Value.Name);
            Assert.Equal(123, after.Value.Score);
        }

        [Fact]
        public void ThrowsIfNullStream()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new Serialiser<UserScore>().Serialise((Stream)null, new UserScore("Doug", 100)));

            Assert.Equal("stream", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => new Serialiser(typeof(UserScore)).Serialise((Stream)null, new UserScore("Doug", 100)));

            Assert.Equal("stream", ex.ParamName);
        }

        [Fact]
        public void ThrowsIfNullPacker()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new Serialiser<UserScore>().Serialise((UnsafePacker)null, new UserScore("Doug", 100)));

            Assert.Equal("packer", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => new Serialiser(typeof(UserScore)).Serialise((UnsafePacker)null, new UserScore("Doug", 100)));

            Assert.Equal("packer", ex.ParamName);
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