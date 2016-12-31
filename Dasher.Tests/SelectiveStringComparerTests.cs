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
    public class SelectiveStringComparerTests
    {
        [Fact]
        public void Comparisons()
        {
            var cmp = new SelectiveStringComparer();

            Assert.True(cmp.Equals("A\rB", "AB"));
            Assert.True(cmp.Equals("AB", "A\rB"));
            Assert.True(cmp.Equals("A\n\rB", "A\r\nB"));
            Assert.True(cmp.Equals("\r\nAB\r\n", "AB"));
            Assert.True(cmp.Equals("\r\nAB\r\n", "A\r\n\rB"));

            Assert.False(cmp.Equals("\r\nAB\r\n", "ABB"));
            Assert.False(cmp.Equals("\r\nAB\r\n", "AAB"));
            Assert.False(cmp.Equals("\r\nAB\r\n", "BA"));
        }
    }
}