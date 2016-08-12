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
using Dasher.TypeProviders;
using Xunit;

namespace Dasher.Tests
{
    public class UnionProviderTests
    {
        [Fact]
        public void TypeNames()
        {
            Assert.Equal("String", UnionProvider.GetTypeName(typeof(string)));
            Assert.Equal("Int32", UnionProvider.GetTypeName(typeof(int)));
            Assert.Equal("Version", UnionProvider.GetTypeName(typeof(Version)));
            Assert.Equal("Guid", UnionProvider.GetTypeName(typeof(Guid)));

            Assert.Equal("Union<Int32,String>", UnionProvider.GetTypeName(typeof(Union<int, string>)));

            Assert.Equal("Dasher.Tests.ValueWrapper<String>", UnionProvider.GetTypeName(typeof(ValueWrapper<string>)));

            Assert.Equal("Dasher.Tests.UnionProviderTests", UnionProvider.GetTypeName(typeof(UnionProviderTests)));

            Assert.Equal("[String]", UnionProvider.GetTypeName(typeof(IReadOnlyList<string>)));

            Assert.Equal("(Int32=>Boolean)", UnionProvider.GetTypeName(typeof(IReadOnlyDictionary<int, bool>)));

            Assert.Equal("(Int32=>[Boolean])", UnionProvider.GetTypeName(typeof(IReadOnlyDictionary<int, IReadOnlyList<bool>>)));
            Assert.Equal("[(Int32=>Boolean)]", UnionProvider.GetTypeName(typeof(IReadOnlyList<IReadOnlyDictionary<int, bool>>)));
        }
    }
}