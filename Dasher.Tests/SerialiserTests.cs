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
using System.IO;
using Xunit;

namespace Dasher.Tests
{
    public sealed class SerialiserTests
    {
        [Fact]
        public void DisallowsPrimitiveTypes()
        {
            var exception = Assert.Throws<SerialisationException>(() => new Serialiser<int>());
            Assert.Equal("Cannot serialise type \"System.Int32\": Top level types must be complex to support future versioning.", exception.Message);
        }

        [Fact]
        public void DisallowsObject()
        {
            var exception = Assert.Throws<SerialisationException>(() => new Serialiser<object>());
            Assert.Equal($"Cannot serialise type \"System.Object\": Complex type provider requires constructor to have at least one argument. Use \"{typeof(Empty).FullName}\" to model an empty type.", exception.Message);
        }

        [Fact]
        public void DisallowsTypeWithNoProperties()
        {
            var exception = Assert.Throws<SerialisationException>(() => new Serialiser<NoProperties>());
            Assert.Equal($"Cannot serialise type \"Dasher.Tests.NoProperties\": Complex type provider requires constructor to have at least one argument. Use \"{typeof(Empty).FullName}\" to model an empty type.", exception.Message);
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
        public void ThrowsIfNullStream()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new Serialiser<UserScore>().Serialise((Stream)null, new UserScore("Doug", 100)));

            Assert.Equal("stream", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => new Serialiser(typeof(UserScore)).Serialise((Stream)null, new UserScore("Doug", 100)));

            Assert.Equal("stream", ex.ParamName);
        }

        [Fact]
        public void ThrowsIfNullPacker()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new Serialiser<UserScore>().Serialise((Packer)null, new UserScore("Doug", 100)));

            Assert.Equal("packer", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => new Serialiser(typeof(UserScore)).Serialise((Packer)null, new UserScore("Doug", 100)));

            Assert.Equal("packer", ex.ParamName);
        }
    }
}