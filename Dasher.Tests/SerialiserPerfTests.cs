using System;
using System.Diagnostics;
using System.IO;
using MsgPack;
using MsgPack.Serialization;
using Xunit;

namespace Dasher.Tests
{
    public sealed class SerialiserPerfTests
    {
        [Fact]
        public void SerialisationPerf()
        {
            var dasherSer = new Serialiser<UserScore>();
            var dasherPacker = new UnsafePacker(Stream.Null);

            var cliSer = MessagePackSerializer.Get<UserScore>(new SerializationContext
            {
                CompatibilityOptions =
                {
                    PackerCompatibilityOptions = PackerCompatibilityOptions.None
                },
                SerializationMethod = SerializationMethod.Map
            });
            var cliPacker = MsgPack.Packer.Create(Stream.Null, PackerCompatibilityOptions.None);

            var obj = new UserScore("Bob", 1234);

            const int warmUpLoopCount = 1000;
            const int timedLoopCount = 10 * 1000 * 1000;

            var sw = Stopwatch.StartNew();

            ////

            for (var i = 0; i < warmUpLoopCount; i++)
                cliSer.PackTo(cliPacker, obj);

            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete(1000);
            GC.WaitForPendingFinalizers();

            sw.Restart();

            for (var i = 0; i < timedLoopCount; i++)
                cliSer.PackTo(cliPacker, obj);

            var cliMs = sw.Elapsed.TotalMilliseconds;

            ////

            for (var i = 0; i < warmUpLoopCount; i++)
            {
                dasherSer.Serialise(dasherPacker, obj);
                dasherPacker.Flush();
            }

            GC.Collect(2, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete(1000);
            GC.WaitForPendingFinalizers();

            sw.Restart();

            for (var i = 0; i < timedLoopCount; i++)
            {
                dasherSer.Serialise(dasherPacker, obj);
                dasherPacker.Flush();
            }

            var dasherMs = sw.Elapsed.TotalMilliseconds;

            ////

#if DEBUG
            Assert.True(false, "Performance comparison must be performed on a release build.");
#endif

            // serialisation performance is on par with MsgPack.Cli. should always be within 10%.
            Assert.True(dasherMs < cliMs * 1.1, $"{nameof(dasherMs)}={dasherMs} {nameof(cliMs)}={cliMs}");
        }
    }
}