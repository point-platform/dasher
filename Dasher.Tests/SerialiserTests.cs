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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Dasher.Tests
{
    public sealed class SerialiserTests
    {
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

        [Fact]
        public void HandlesDecimal()
        {
            var after = RoundTrip(new UserScoreDecimal("Bob", 123.456m));

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123.456m, after.Score);
        }

        [Fact]
        public void HandlesDateTime()
        {
            var dateTime = new DateTime(2015, 12, 25);

            var after = RoundTrip(new WithDateTimeProperty(dateTime));

            Assert.Equal(dateTime, after.Date);
        }

        [Fact]
        public void HandlesTimeSpan()
        {
            var timeSpan = new TimeSpan(12345678);

            var after = RoundTrip(new WithTimeSpanProperty(timeSpan));

            Assert.Equal(timeSpan, after.Time);
        }

        [Fact]
        public void HandlesIntPtr()
        {
            var timeSpan = new IntPtr(12345678);

            var after = RoundTrip(new WithIntPtrProperty(timeSpan));

            Assert.Equal(timeSpan, after.IntPtr);
        }

        [Fact]
        public void HandlesVersion()
        {
            var version = new Version("1.2.3");

            var after = RoundTrip(new WithVersionProperty(version));

            Assert.Equal(version, after.Version);

            // check null version works ok
            RoundTrip(new WithVersionProperty(null));
        }

        [Fact]
        public void HandlesGuid()
        {
            var guid = new Guid();

            var after = RoundTrip(new WithGuidProperty(guid));

            Assert.Equal(guid, after.Guid);
        }

        [Fact]
        public void HandlesNullableValueTypes()
        {
            var after = RoundTrip(new WithNullableProperties(null, null, null, null));

            Assert.Null(after.Int);
            Assert.Null(after.Double);
            Assert.Null(after.DateTime);
            Assert.Null(after.Decimal);

            after = RoundTrip(new WithNullableProperties(123, 2.3d, DateTime.Today, 12.3m));

            Assert.Equal(123, after.Int);
            Assert.Equal(2.3d, after.Double);
            Assert.Equal(DateTime.Today, after.DateTime);
            Assert.Equal(12.3m, after.Decimal);
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
            var after = RoundTrip(new UserScoreWrapper(1.0, new UserScore("Bob", 123)));

            Assert.Equal(1.0, after.Weight);
            Assert.Equal("Bob", after.UserScore.Name);
            Assert.Equal(123, after.UserScore.Score);
        }

        [Fact]
        public void HandlesBinary()
        {
            var after = RoundTrip(new WithBinary(Enumerable.Range(0, byte.MaxValue).Select(i => (byte)i).ToArray()));

            Assert.Equal(
                Enumerable.Range(0, byte.MaxValue).Select(i => (byte)i).ToArray(),
                after.Bytes);
        }

        [Fact]
        public void HandlesList()
        {
            var after = RoundTrip(new UserScoreList("Bob", new[] {1, 2, 3, 4}));

            Assert.Equal("Bob", after.Name);
            Assert.Equal(new[] {1, 2, 3, 4}, after.Scores);
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
        public void HandlesNullList()
        {
            var after = RoundTrip(new UserScoreList("Bob", null));

            Assert.Equal("Bob", after.Name);
            Assert.Null(after.Scores);
        }

        [Fact]
        public void HandlesEnum()
        {
            var after = RoundTrip(new WithEnumProperty(TestEnum.Bar));

            Assert.Equal(TestEnum.Bar, after.TestEnum);
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
        public void HandlesGenericType()
        {
            var after = RoundTrip(new GenericWrapper<string>("Hello"));

            Assert.Equal("Hello", after.Content);
        }

        [Fact]
        public void HandlesComplexGenericType()
        {
            var after = RoundTrip(new GenericWrapper<UserScore>(new UserScore("Bob", 123)));

            Assert.Equal("Bob", after.Content.Name);
            Assert.Equal(123, after.Content.Score);
        }

        [Fact]
        public void HandlesTuple()
        {
            var after = RoundTrip(new TupleWrapper<int, string, bool?>(Tuple.Create(1, "Bob", (bool?)true)));

            Assert.Equal(1, after.Item.Item1);
            Assert.Equal("Bob", after.Item.Item2);
            Assert.Equal(true, after.Item.Item3);
        }

        [Fact]
        public void HandlesDictionary()
        {
            var after = RoundTrip(new DictionaryWrapper<int, string>(
                new Dictionary<int, string> {{1, "Hello"}, {2, "World"}}));

            Assert.Equal(2, after.Item.Count);
            Assert.Equal("Hello", after.Item[1]);
            Assert.Equal("World", after.Item[2]);

            var after2 = RoundTrip(new DictionaryWrapper<int, bool?>(
                new Dictionary<int, bool?> {{1, true}, {2, false}, {3, null} }));

            Assert.Equal(3, after2.Item.Count);
            Assert.Equal(true, after2.Item[1]);
            Assert.Equal(false, after2.Item[2]);
            Assert.Equal(null, after2.Item[3]);
        }

        [Fact]
        public void HandlesClassWrappingCustomStruct()
        {
            var after = RoundTrip(new StructWrapper(new UserScoreStruct("Foo", 123)));

            Assert.Equal("Foo", after.Struct.Name);
            Assert.Equal(123, after.Struct.Score);
        }

        #region Test helpers

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