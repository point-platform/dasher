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
using System.Diagnostics;
using System.IO;
using MsgPack;
using MsgPack.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace Dasher.Tests
{
    public sealed class DeserialiserPerfTests
    {
        private readonly ITestOutputHelper _output;

        public DeserialiserPerfTests(ITestOutputHelper output)
        {
            _output = output;
        }

//        [Fact]
        public void DeserialisationPerf()
        {
            var stream = new MemoryStream();
            new Serialiser<UserScore>().Serialise(stream, new UserScore("Bob", 12345));

            var dasherDeser = new Deserialiser<UserScore>();

            var cliSer = MessagePackSerializer.Get<UserScore>(new SerializationContext
            {
                CompatibilityOptions =
                {
                    PackerCompatibilityOptions = PackerCompatibilityOptions.None
                },
                SerializationMethod = SerializationMethod.Map
            });

            const int warmUpLoopCount = 1000;
            const int timedLoopCount = 100 * 1000;

            var sw = Stopwatch.StartNew();

            ////

            for (var i = 0; i < warmUpLoopCount; i++)
            {
                stream.Position = 0;
                dasherDeser.Deserialise(stream);
            }

            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete(1000);
            GC.WaitForPendingFinalizers();

            sw.Restart();

            for (var i = 0; i < timedLoopCount; i++)
            {
                stream.Position = 0;
                dasherDeser.Deserialise(stream);
            }

            var dasherMs = sw.Elapsed.TotalMilliseconds;

            ////

            for (var i = 0; i < warmUpLoopCount; i++)
            {
                stream.Position = 0;
                cliSer.Unpack(stream);
            }

            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete(1000);
            GC.WaitForPendingFinalizers();

            sw.Restart();

            for (var i = 0; i < timedLoopCount; i++)
            {
                stream.Position = 0;
                cliSer.Unpack(stream);
            }

            var cliMs = sw.Elapsed.TotalMilliseconds;

            ////

#if DEBUG
            Assert.True(false, "Performance comparison must be performed on a release build.");
#endif

            _output.WriteLine($"{nameof(dasherMs)}={dasherMs} {nameof(cliMs)}={cliMs}");
            Assert.True(dasherMs < cliMs, $"{nameof(dasherMs)}={dasherMs} should be less than {nameof(cliMs)}={cliMs}");
        }

//        [Fact]
        public void ConstructionPerf()
        {
            var stream = new MemoryStream();
            new Packer(stream).PackMapHeader(0);
            var bytes = stream.GetBuffer();
            var sw = Stopwatch.StartNew();

            for (var i = 0; i < 100; i++)
                new Deserialiser<TestDefaultParams>().Deserialise(bytes);

            sw.Stop();

            Assert.True(false, $"{sw.Elapsed.TotalMilliseconds}");
        }
    }
}