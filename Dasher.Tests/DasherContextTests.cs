using System;
using Xunit;

// ReSharper disable EmptyConstructor
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local

namespace Dasher.Tests
{
    public sealed class DasherContextTests
    {
        private class Invalid1 { }
        private class Invalid2 { public Invalid2() {} }
        private class Invalid3 { private Invalid3(int i) {} public int I { get; } }

        private class Valid1 { public Valid1(int i) {} public int I { get; } }
        private struct Valid2 { public Valid2(int i, float f) { I = i; } public int I { get; } }

        [Fact]
        public void IsValidTopLevelType()
        {
            var context = new DasherContext();

            Assert.False(context.IsValidTopLevelType(typeof(string)));
            Assert.False(context.IsValidTopLevelType(typeof(int)));
            Assert.False(context.IsValidTopLevelType(typeof(int?)));
            Assert.False(context.IsValidTopLevelType(typeof(Tuple<int, float>)));
            Assert.False(context.IsValidTopLevelType(typeof(TimeSpan)));
            Assert.False(context.IsValidTopLevelType(typeof(DateTime)));
            Assert.False(context.IsValidTopLevelType(typeof(Guid)));
            Assert.False(context.IsValidTopLevelType(typeof(Version)));
            Assert.False(context.IsValidTopLevelType(typeof(Invalid1)));
            Assert.False(context.IsValidTopLevelType(typeof(decimal)));
            Assert.False(context.IsValidTopLevelType(typeof(Union<int, string>)));

            Assert.True(context.IsValidTopLevelType(typeof(Valid1)));
            Assert.True(context.IsValidTopLevelType(typeof(Valid2)));
            Assert.True(context.IsValidTopLevelType(typeof(Union<Valid1, Valid2>)));
            Assert.True(context.IsValidTopLevelType(typeof(Empty)));
            Assert.True(context.IsValidTopLevelType(typeof(Union<Empty, Valid1>)));
            Assert.True(context.IsValidTopLevelType(typeof(Valid2?)));
        }
    }
}