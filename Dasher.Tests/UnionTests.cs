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
using Xunit;

namespace Dasher.Tests
{
    public class UnionTests
    {
        [Fact]
        public void MatchingWorks()
        {
            Union<int, string, double> union1 = 123;
            Union<int, string, double> union2 = "Hello";
            Union<int, string, double> union3 = 1234.5;

            union1.Match(
                i => Assert.Equal(123, i),
                _ => { throw new Exception(); },
                _ => { throw new Exception(); });

            union2.Match(
                _ => { throw new Exception(); },
                s => Assert.Equal("Hello", s),
                _ => { throw new Exception(); });

            union3.Match(
                _ => { throw new Exception(); },
                _ => { throw new Exception(); },
                d => Assert.Equal(1234.5, d));
        }

        [Fact]
        public void Collections()
        {
            var array = new Union<string, int>[] {1, "Hello"};
            Assert.Equal(1, array[0]);
            Assert.Equal("Hello", array[1]);
        }

        [Fact]
        public void TryCreate()
        {
            object o = 123;
            Union<int, double> union;
            Assert.True(Union<int, double>.TryCreate(o, out union));

            Assert.NotNull(union);
            Assert.Equal(123, union);

            Assert.False(Union<int, double>.TryCreate("Wrong type", out union));
            Assert.Null(union);
        }

        [Fact]
        public void Create()
        {
            Union<int, double>.Create(123).Match(
                i => Assert.Equal(123, i),
                d => { throw new Exception("Value should not be a double"); });

            Union<int, double>.Create(123.0).Match(
                i => { throw new Exception("Value should not be an int"); },
                d => Assert.Equal(123.0, d));
        }

        [Fact]
        public void Equals()
        {
            Assert.Equal(1, (Union<int, double>)1);
            Assert.Equal(1.0, (Union<int, double>)1.0);

            Assert.Equal(
                Union<int, double>.Create(1),
                Union<int, double>.Create(1));

            Assert.Equal(
                Union<int, string>.Create(null),
                Union<int, string>.Create(null));
        }

        [Fact]
        public void ToStringTest()
        {
            Assert.Equal("1",     Union<int, string>.Create(1).ToString());
            Assert.Equal("Hello", Union<int, string>.Create("Hello").ToString());
        }

        [Fact]
        public void GetHashCodeTest()
        {
            Assert.Equal(1.GetHashCode(), Union<int, double>.Create(1).GetHashCode());
            Assert.Equal(1.0.GetHashCode(), Union<int, double>.Create(1.0).GetHashCode());
        }

        [Fact]
        public void NullValues()
        {
            var str = Union<string, Version>.Create((string)null);
            var ver = Union<string, Version>.Create((Version)null);

            Assert.True(str.Equals(null));
            Assert.True(ver.Equals(null));

            Assert.Equal(null, str.ToString());
            Assert.Equal(null, ver.ToString());

            Assert.Equal(0, str.GetHashCode());
            Assert.Equal(0, ver.GetHashCode());

            Assert.Equal(typeof(string), str.Type);
            Assert.Equal(typeof(Version), ver.Type);

            Assert.Null(str.Value);
            Assert.Null(ver.Value);

            Assert.Equal(
                Union<int, string>.Create(null),
                Union<int, string>.Create(null));
        }

        [Fact]
        public void Type()
        {
            Assert.Equal(typeof(int), Union<int, double>.Create(1).Type);
            Assert.Equal(typeof(double), Union<int, double>.Create(1.0).Type);
        }

        [Fact]
        public void ExplicitCastToValue()
        {
            Assert.Equal(1, (int)Union<int, double>.Create(1));
            Assert.Equal(1.0, (double)Union<int, double>.Create(1.0));

            Assert.Throws<InvalidCastException>(() => (double)Union<int, double>.Create(1));
            Assert.Throws<InvalidCastException>(() => (int)Union<int, double>.Create(1.0));
        }
    }
}
