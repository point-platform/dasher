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

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedTypeParameter
// ReSharper disable MemberCanBePrivate.Global

namespace Dasher.Tests
{
    internal class Foo<T>
    {
        internal struct Bar { }
        internal struct Baz<U> { }
    }

    public class UnionEncodingTests
    {
        public struct NestedType { }
        public struct NestedType<T> { }

        [Fact]
        public void TypeNames()
        {
            Assert.Equal("String", UnionEncoding.GetTypeName(typeof(string)));
            Assert.Equal("Int32", UnionEncoding.GetTypeName(typeof(int)));
            Assert.Equal("Version", UnionEncoding.GetTypeName(typeof(Version)));
            Assert.Equal("Guid", UnionEncoding.GetTypeName(typeof(Guid)));

            Assert.Equal("Union<Int32,String>", UnionEncoding.GetTypeName(typeof(Union<int, string>)));

            Assert.Equal("Dasher.Tests.ValueWrapper<String>", UnionEncoding.GetTypeName(typeof(ValueWrapper<string>)));

            Assert.Equal("Dasher.Tests.UnionEncodingTests", UnionEncoding.GetTypeName(typeof(UnionEncodingTests)));

            Assert.Equal("Dasher.Tests.UnionEncodingTests+NestedType", UnionEncoding.GetTypeName(typeof(NestedType)));
            Assert.Equal("Dasher.Tests.UnionEncodingTests+NestedType<Int32>", UnionEncoding.GetTypeName(typeof(NestedType<int>)));
            Assert.Equal("Dasher.Tests.Foo<Int32>+Bar", UnionEncoding.GetTypeName(typeof(Foo<int>.Bar)));
            Assert.Equal("Dasher.Tests.Foo<Int32>+Baz<String>", UnionEncoding.GetTypeName(typeof(Foo<int>.Baz<string>)));

            Assert.Equal("[String]", UnionEncoding.GetTypeName(typeof(IReadOnlyList<string>)));

            Assert.Equal("(Int32=>Boolean)", UnionEncoding.GetTypeName(typeof(IReadOnlyDictionary<int, bool>)));

            Assert.Equal("(Int32=>[Boolean])", UnionEncoding.GetTypeName(typeof(IReadOnlyDictionary<int, IReadOnlyList<bool>>)));
            Assert.Equal("[(Int32=>Boolean)]", UnionEncoding.GetTypeName(typeof(IReadOnlyList<IReadOnlyDictionary<int, bool>>)));
        }
    }
}