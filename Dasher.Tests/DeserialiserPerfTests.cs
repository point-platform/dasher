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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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

        [Fact(Skip = "For informational purposes, with nothing to assert")]
        public void DeserialisationPerf()
        {
#if DEBUG
            Assert.True(false, "Performance comparison must be performed on a release build.");
#endif

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

            TestUtils.CleanUpForPerfTest();

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

            TestUtils.CleanUpForPerfTest();

            sw.Restart();

            for (var i = 0; i < timedLoopCount; i++)
            {
                stream.Position = 0;
                cliSer.Unpack(stream);
            }

            var cliMs = sw.Elapsed.TotalMilliseconds;

            ////

            _output.WriteLine($"{nameof(dasherMs)}={dasherMs} {nameof(cliMs)}={cliMs}");
            Assert.True(dasherMs < cliMs, $"{nameof(dasherMs)}={dasherMs} should be less than {nameof(cliMs)}={cliMs}");
        }

        [Fact(Skip = "For informational purposes, with nothing to assert")]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void ConstructionPerf()
        {
#if DEBUG
            Assert.True(false, "Performance comparison must be performed on a release build.");
#endif

            const int iterations = 100;

            // Warm up the deserialiser, with a different type
            new Deserialiser<RecurringTree>();

            ////////////////////////////////////////////////////////////////////////////

            Stopwatch buildTime;
            {
                TestUtils.CleanUpForPerfTest();

                buildTime = Stopwatch.StartNew();

                for (var i = 0; i < iterations; i++)
                    new Deserialiser<ClassWithAllDefaults>();

                buildTime.Stop();
            }

            ////////////////////////////////////////////////////////////////////////////

            Stopwatch buildWithSharedContextTime;
            {
                TestUtils.CleanUpForPerfTest();

                buildWithSharedContextTime = Stopwatch.StartNew();

                var context = new DasherContext();

                for (var i = 0; i < iterations; i++)
                    new Deserialiser<ClassWithAllDefaults>(context: context);

                buildWithSharedContextTime.Stop();
            }

            ////////////////////////////////////////////////////////////////////////////

            Stopwatch buildAndUseTime;
            {
                var stream = new MemoryStream();
                new Packer(stream).PackMapHeader(0);
                var bytes = stream.ToArray();

                var builtDeserialisers = Enumerable.Range(0, iterations).Select(i => new Deserialiser<ClassWithAllDefaults>()).ToList();

                builtDeserialisers[0].Deserialise(bytes);

                TestUtils.CleanUpForPerfTest();

                buildAndUseTime = Stopwatch.StartNew();

                for (var i = 0; i < iterations; i++)
                    builtDeserialisers[i].Deserialise(bytes);

                buildAndUseTime.Stop();
            }

            ////////////////////////////////////////////////////////////////////////////

            Stopwatch buildAndUseWithSharedContextTime;
            {
                var stream = new MemoryStream();
                new Packer(stream).PackMapHeader(0);
                var bytes = stream.ToArray();

                var builtDeserialisers = Enumerable.Range(0, iterations).Select(i => new Deserialiser<ClassWithAllDefaults>()).ToList();

                builtDeserialisers[0].Deserialise(bytes);

                TestUtils.CleanUpForPerfTest();

                buildAndUseWithSharedContextTime = Stopwatch.StartNew();

                for (var i = 0; i < iterations; i++)
                    builtDeserialisers[i].Deserialise(bytes);

                buildAndUseWithSharedContextTime.Stop();
            }

            ////////////////////////////////////////////////////////////////////////////

            _output.WriteLine($"{iterations} constructions in {buildTime.Elapsed.TotalMilliseconds} ms");
            _output.WriteLine($"{iterations} constructions with shared context in {buildWithSharedContextTime.Elapsed.TotalMilliseconds} ms");
            _output.WriteLine($"{iterations} deserialisations in {buildAndUseTime.Elapsed.TotalMilliseconds} ms");
            _output.WriteLine($"{iterations} deserialisations with shared context in {buildAndUseWithSharedContextTime.Elapsed.TotalMilliseconds} ms");
        }
    }
}