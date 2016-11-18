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
using MsgPack;
using Xunit;

namespace Dasher.Tests
{
    /// <summary>
    /// Verifies that values of various types are correctly serialised and deserialised.
    /// </summary>
    public sealed class TypeSupportTests
    {
        [Fact]
        public void SupportsNestedInt()
        {
            TestNested(12345678, packer => packer.Pack(12345678));
        }

        [Fact]
        public void SupportsNestedNullableInt()
        {
            TestNested((int?)12345678, packer => packer.Pack(12345678));
            TestNested((int?)null, packer => packer.PackNull());
        }

        [Fact]
        public void SupportsNestedDouble()
        {
            TestNested(2.3d, packer => packer.Pack(2.3d));
        }

        [Fact]
        public void SupportsNestedNullableDouble()
        {
            TestNested((double?)2.3d, packer => packer.Pack(2.3d));
            TestNested((double?)null, packer => packer.PackNull());
        }

        [Fact]
        public void SupportsNestedChar()
        {
            TestNested('a', packer => packer.Pack("a"));
            TestNested(' ', packer => packer.Pack(" "));
            TestNested('\0', packer => packer.Pack("\0"));
        }

        [Fact]
        public void SupportsNestedNullableChar()
        {
            TestNested((char?)'a', packer => packer.Pack("a"));
            TestNested((char?)' ', packer => packer.Pack(" "));
            TestNested((char?)'\0', packer => packer.Pack("\0"));
            TestNested((char?)null, packer => packer.PackNull());
        }

        [Fact]
        public void SupportsNestedDecimal()
        {
            TestNested(123.4567m, packer => packer.Pack("123.4567"));
        }

        [Fact]
        public void SupportsNestedNullableDecimal()
        {
            TestNested((decimal?)123.4567m, packer => packer.Pack("123.4567"));
            TestNested((decimal?)null, packer => packer.PackNull());
        }

        [Fact]
        public void SupportsNestedDateTime()
        {
            Action<DateTime> test = dateTime =>
            {
                var after = TestNested(dateTime, packer => packer.Pack(dateTime.ToBinary()));

                Assert.Equal(dateTime, after);
                Assert.Equal(dateTime.Kind, after.Kind);
            };

            test(new DateTime(2015, 12, 25));
            test(DateTime.SpecifyKind(new DateTime(2015, 12, 25), DateTimeKind.Local));
            test(DateTime.SpecifyKind(new DateTime(2015, 12, 25), DateTimeKind.Unspecified));
            test(DateTime.SpecifyKind(new DateTime(2015, 12, 25), DateTimeKind.Utc));
            test(DateTime.MinValue);
            test(DateTime.MaxValue);
            test(DateTime.Now);
            test(DateTime.UtcNow);
        }

        [Fact]
        public void SupportsNestedDateTimeOffset()
        {
            Action<DateTimeOffset> test = dto =>
            {
                var after = TestNested(dto, packer => packer.PackArrayHeader(2)
                    .Pack(dto.DateTime.ToBinary())
                    .Pack((short)dto.Offset.TotalMinutes));

                Assert.Equal(dto, after);
                Assert.Equal(dto.Offset, after.Offset);
                Assert.Equal(dto.DateTime.Kind, after.DateTime.Kind);
                Assert.True(dto.EqualsExact(after));
            };

            var offsets = new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromHours(1),
                TimeSpan.FromHours(-1),
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(-10),
                TimeSpan.FromMinutes(90),
                TimeSpan.FromMinutes(-90)
            };

            foreach (var offset in offsets)
                test(new DateTimeOffset(new DateTime(2015, 12, 25), offset));

            test(DateTimeOffset.MinValue);
            test(DateTimeOffset.MaxValue);
            test(DateTimeOffset.Now);
            test(DateTimeOffset.UtcNow);
        }

        [Fact]
        public void SupportsNestedTimeSpan()
        {
            var timeSpan = TimeSpan.FromSeconds(1234.5678);
            TestNested(timeSpan, packer => packer.Pack(timeSpan.Ticks));
        }

        [Fact]
        public void SupportsNestedIntPtr()
        {
            var intPtr = new IntPtr(12345678);
            TestNested(intPtr, packer => packer.Pack(intPtr.ToInt64()));
        }

        [Fact]
        public void SupportsNestedVersion()
        {
            var version = new Version("1.2.3");
            TestNested(version, packer => packer.Pack(version.ToString()));
            TestNested((Version)null, packer => packer.PackNull());
        }

        [Fact]
        public void SupportsNestedGuid()
        {
            var guid = Guid.NewGuid();
            TestNested(guid, packer => packer.Pack(guid.ToByteArray()));
        }

        [Fact]
        public void SupportsNestedEnum()
        {
            TestNested(TestEnum.Bar, packer => packer.Pack("Bar"));
        }

        [Fact]
        public void SupportsNestedReadOnlyList()
        {
            TestNested<IReadOnlyList<int>>(new[] {1, 2, 3}, packer => packer.PackArrayHeader(3).Pack(1).Pack(2).Pack(3));
            TestNested<IReadOnlyList<int>>(new int[0], packer => packer.PackArrayHeader(0));
            TestNested<IReadOnlyList<int>>(null, packer => packer.PackNull());
        }

        [Fact]
        public void SupportsNestedReadOnlyListOfReadOnlyList()
        {
            TestNested<IReadOnlyList<IReadOnlyList<int>>>(
                new[] { new[] { 1, 2, 3 }, new[] { 4, 5, 6 } },
                packer => packer.PackArrayHeader(2)
                    .PackArrayHeader(3).Pack(1).Pack(2).Pack(3)
                    .PackArrayHeader(3).Pack(4).Pack(5).Pack(6),
                actual =>
                {
                    Assert.Equal(2, actual.Count);
                    Assert.Equal(new[] { 1, 2, 3 }, actual[0]);
                    Assert.Equal(new[] { 4, 5, 6 }, actual[1]);
                });
            TestNested<IReadOnlyList<IReadOnlyList<int>>>(null, packer => packer.PackNull());
        }

        [Fact]
        public void SupportsNestedReadOnlyDictionary()
        {
            TestNested<IReadOnlyDictionary<int, string>>(null, packer => packer.PackNull());
            TestNested<IReadOnlyDictionary<int, string>>(new Dictionary<int, string>(), packer => packer.PackMapHeader(0));

            TestNested<IReadOnlyDictionary<int, string>>(
                new Dictionary<int, string> {{1, "Hello"}, {2, "World"}},
                packer => packer.PackMapHeader(2)
                    .Pack(1).Pack("Hello")
                    .Pack(2).Pack("World"));

            TestNested<IReadOnlyDictionary<int, bool?>>(
                new Dictionary<int, bool?> { { 1, true }, { 2, false }, {3, null} },
                packer => packer.PackMapHeader(3)
                    .Pack(1).Pack(true)
                    .Pack(2).Pack(false)
                    .Pack(3).PackNull());
        }

        [Fact]
        public void SupportsNestedByteArray()
        {
            TestNested(new byte[] {1, 2, 3, 4}, packer => packer.PackBinary(new byte[] {1, 2, 3, 4}));
        }

        [Fact]
        public void SupportsNestedTuple2()
        {
            TestNested(Tuple.Create(1, "Hello"), packer => packer.PackArrayHeader(2).Pack(1).Pack("Hello"));
        }

        [Fact]
        public void SupportsNestedTuple3()
        {
            TestNested(Tuple.Create(1, "Hello", true), packer => packer.PackArrayHeader(3).Pack(1).Pack("Hello").Pack(true));
        }

        [Fact]
        public void SupportsNestedUnion()
        {
            TestNested(Union<int, double>.Create(123), packer => packer.PackArrayHeader(2).Pack("Int32").Pack(123));
            TestNested(Union<int, double>.Create(123.0), packer => packer.PackArrayHeader(2).Pack("Double").Pack(123.0));
            TestNested(Union<int, string>.Create("Hello"), packer => packer.PackArrayHeader(2).Pack("String").Pack("Hello"));
            TestNested(Union<int, string>.Create(null), packer => packer.PackArrayHeader(2).Pack("String").PackNull());
            TestNested((Union<int, string>)null, packer => packer.PackNull());
        }

        [Fact]
        public void SupportsNestedStruct()
        {
            TestNested(new UserScoreStruct("Foo", 123), packer => packer.PackMapHeader(2).Pack("Name").Pack("Foo").Pack("Score").Pack(123));
        }

        [Fact]
        public void SupportsNestedClass()
        {
            TestNested(new UserScore("Foo", 123), packer => packer.PackMapHeader(2).Pack("Name").Pack("Foo").Pack("Score").Pack(123));
        }

        [Fact]
        public void SupportsNestedEmpty()
        {
            TestNested((Empty)null, packer => packer.PackMapHeader(0));
        }


        [Fact]
        public void SupportsTopLevelClass()
        {
            TestTopLevel(new UserScore("Foo", 123), packer => packer.PackMapHeader(2).Pack("Name").Pack("Foo").Pack("Score").Pack(123));
            TestTopLevel((UserScore)null, packer => packer.PackNull());
        }

        [Fact]
        public void SupportsTopLevelStruct()
        {
            TestTopLevel(new UserScoreStruct("Foo", 123), packer => packer.PackMapHeader(2).Pack("Name").Pack("Foo").Pack("Score").Pack(123));
        }

        [Fact]
        public void SupportsTopLevelNullableStruct()
        {
            TestTopLevel<UserScoreStruct?>(new UserScoreStruct("Foo", 123), packer => packer.PackMapHeader(2).Pack("Name").Pack("Foo").Pack("Score").Pack(123));
            TestTopLevel<UserScoreStruct?>(null, packer => packer.PackNull());
        }

        [Fact]
        public void SupportsTopLevelUnion()
        {
            // Top level unions are allowed, so long as each type within the union meets the requirements of a top-level type

            TestTopLevel(
                Union<UserScore, UserScoreStruct>.Create(new UserScore("Bob", 123)),
                packer => packer.PackArrayHeader(2).Pack("Dasher.Tests.UserScore").PackMapHeader(2).Pack("Name").Pack("Bob").Pack("Score").Pack(123));

            TestTopLevel(
                Union<UserScore, UserScoreStruct>.Create(new UserScoreStruct("Bob", 123)),
                packer => packer.PackArrayHeader(2).Pack("Dasher.Tests.UserScoreStruct").PackMapHeader(2).Pack("Name").Pack("Bob").Pack("Score").Pack(123));

            TestTopLevel(
                Union<UserScore, UserScoreStruct>.Create(null),
                packer => packer.PackArrayHeader(2).Pack("Dasher.Tests.UserScore").PackNull());

            TestTopLevel((Union<UserScore, UserScoreStruct>)null, packer => packer.PackNull());
        }

        [Fact]
        public void SupportsTopLevelEmpty()
        {
            TestTopLevel(
                (Empty)null,
                packer => packer.PackMapHeader(0));
        }

        #region Helpers

        private static T TestTopLevel<T>(T value, Action<MsgPack.Packer> packAction, Action<T> customEvaluator = null)
        {
            byte[] msgPackCliBytes;
            using (var stream = new MemoryStream())
            using (var packer = MsgPack.Packer.Create(stream, PackerCompatibilityOptions.None))
            {
                packAction(packer);
                stream.Position = 0;
                msgPackCliBytes = stream.ToArray();
            }

            var deserialisedValue = new Deserialiser<T>().Deserialise(msgPackCliBytes);

            if (customEvaluator != null)
                customEvaluator(value);
            else
                Assert.Equal(value, deserialisedValue);

            var dasherBytes = new Serialiser<T>().Serialise(value);

            Assert.Equal(msgPackCliBytes.Length, dasherBytes.Length);
            Assert.Equal(msgPackCliBytes, dasherBytes);

            return deserialisedValue;
        }

        private static T TestNested<T>(T value, Action<MsgPack.Packer> packAction, Action<T> customEvaluator = null)
        {
            return TestTopLevel(
                new ValueWrapper<T>(value),
                packer =>
                {
                    packer.PackMapHeader(1).Pack(nameof(ValueWrapper<T>.Value));
                    packAction(packer);
                },
                actual =>
                {
                    if (customEvaluator != null)
                        customEvaluator(actual.Value);
                    else
                        Assert.Equal(value, actual.Value);
                })
                .Value;
        }

        #endregion
    }
}