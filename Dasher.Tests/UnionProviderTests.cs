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

            Assert.Equal("Union<Int32,String>", UnionProvider.GetTypeName(typeof(Union<int, string>))); ;

            Assert.Equal("Dasher.Tests.ValueWrapper<String>", UnionProvider.GetTypeName(typeof(ValueWrapper<string>)));

            Assert.Equal("Dasher.Tests.UnionProviderTests", UnionProvider.GetTypeName(typeof(UnionProviderTests)));

            Assert.Equal("[String]", UnionProvider.GetTypeName(typeof(IReadOnlyList<string>)));

            Assert.Equal("(Int32=>Boolean)", UnionProvider.GetTypeName(typeof(IReadOnlyDictionary<int, bool>)));

            Assert.Equal("(Int32=>[Boolean])", UnionProvider.GetTypeName(typeof(IReadOnlyDictionary<int, IReadOnlyList<bool>>)));
            Assert.Equal("[(Int32=>Boolean)]", UnionProvider.GetTypeName(typeof(IReadOnlyList<IReadOnlyDictionary<int, bool>>)));
        }
    }
}