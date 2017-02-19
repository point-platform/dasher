#region License
//
// Dasher
//
// Copyright 2015-2017 Drew Noakes
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

using Dasher.Utils;
using Xunit;

// ReSharper disable UnusedParameter.Local

namespace Dasher.Tests
{
    public sealed class NumericStringComparerTests
    {
        [Fact]
        public void OrdersCorrectly()
        {
            AssertEqual("", "");
            AssertEqual(null, null);
            AssertEqual("Hello", "Hello");
            AssertEqual("Hello123", "Hello123");
            AssertEqual("123", "123");
            AssertEqual("123Hello", "123Hello");

            AssertOrdered("", "Hello");
            AssertOrdered(null, "Hello");
            AssertOrdered("Hello", "Hello1");
            AssertOrdered("Hello123", "Hello124");
            AssertOrdered("Hello123", "Hello133");
            AssertOrdered("Hello123", "Hello223");
            AssertOrdered("123", "124");
            AssertOrdered("123", "133");
            AssertOrdered("123", "223");
            AssertOrdered("123", "1234");
            AssertOrdered("123", "2345");
            AssertOrdered("0", "1");
            AssertOrdered("123Hello", "124Hello");
            AssertOrdered("123Hello", "133Hello");
            AssertOrdered("123Hello", "223Hello");
            AssertOrdered("123Hello", "1234Hello");
        }

        private static void AssertEqual(string x, string y)
        {
            Assert.Equal(0, NumericStringComparer.Default.Compare(x, y));
            Assert.Equal(0, NumericStringComparer.Default.Compare(y, x));
        }

        private static void AssertOrdered(string x, string y)
        {
            Assert.Equal(-1, NumericStringComparer.Default.Compare(x, y));
            Assert.Equal( 1, NumericStringComparer.Default.Compare(y, x));
        }
    }
}