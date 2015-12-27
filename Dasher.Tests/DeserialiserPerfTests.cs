using System;
using System.Diagnostics;
using System.IO;
using MsgPack;
using MsgPack.Serialization;
using Xunit;

namespace Dasher.Tests
{
    public sealed class DeserialiserPerfTests
    {
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