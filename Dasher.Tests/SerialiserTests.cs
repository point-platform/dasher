#region License
//
// Dasher
//
// Copyright 2015 Drew Noakes
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
            // ReSharper disable once ObjectCreationAsStatement
            new Serialiser<Recurring>();
        }

        [Fact]
        public void HandlesRecurringTreeType()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Serialiser<RecurringTree>();
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