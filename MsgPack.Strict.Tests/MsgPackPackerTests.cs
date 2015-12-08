using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace MsgPack.Strict.Tests
{
    public class MsgPackPackerTests
    {
        [Fact]
        public void StreamWritePerfTest()
        {
            const int bufferSize = 1024;
            const int chunkCount = 1024;

            var s = new MemoryStream(bufferSize * chunkCount);

            s.WriteByte(1);

            var sw = Stopwatch.StartNew();

            for (var i = 0; i < bufferSize * chunkCount; i++)
                s.WriteByte((byte)i);

            var oneByOneTime = sw.Elapsed.TotalMilliseconds;

            s.Position = 0;

            var buffer = Enumerable.Range(0, bufferSize).Select(i => (byte)i).ToArray();

            sw.Restart();

            for (var i = 0; i < chunkCount; i++)
            {
                for (int j = 0; j < buffer.Length; j++)
                    buffer[j] = (byte)j;
                s.Write(buffer, 0, bufferSize);
            }

            var inChunksTime = sw.Elapsed.TotalMilliseconds;

            Console.Out.WriteLine("oneByOneTime = {0}", oneByOneTime);
            Console.Out.WriteLine("inChunksTime = {0}", inChunksTime);
        }

        [Fact]
        public void PacksInt()
        {
            var stream = new MemoryStream();
            var packer = new MsgPackPacker(stream);
            var unpacker = Unpacker.Create(stream);

            for (int i = -60000; i < 60000; i++)
            {
                packer.Pack(i);

                stream.Position = 0;

                int result;
                Assert.True(unpacker.ReadInt32(out result));
                Assert.Equal(i, result);
            }
        }
    }
}