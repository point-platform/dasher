using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MsgPack;
using Xunit;

namespace Dasher.Tests
{
    public sealed class PackerPerfTests
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

            Assert.True(inChunksTime < oneByOneTime);
        }

//        [Fact]
        public void PackerPerf()
        {
            var s = new MemoryStream();

            var thisPacker = new Packer(s);
            var thisUnsafePacker = new UnsafePacker(s);
            var thatPacker = MsgPack.Packer.Create(s);

//            var str = new string('a', 256);
//            var bytes = new byte[256];

            const int loopCount = 1024 * 1024;

            Action thisBytePack = () =>
            {
                s.Position = 0;
                thisPacker.Pack(false);
                thisPacker.Pack(true);
                thisPacker.Pack((byte)1);
                thisPacker.Pack((sbyte)-1);
                thisPacker.Pack(1.1f);
                thisPacker.Pack(1.1d);
                thisPacker.Pack((short)1234);
                thisPacker.Pack((ushort)1234);
                thisPacker.Pack((int)1234);
                thisPacker.Pack((uint)1234);
                thisPacker.Pack((long)1234);
                thisPacker.Pack((ulong)1234);
                thisPacker.Pack("Hello World");
//                thisPacker.Pack(str);
//                thisPacker.Pack(bytes);
            };

            Action thisUnsafePack = () =>
            {
                s.Position = 0;
                thisUnsafePacker.Pack(false);
                thisUnsafePacker.Pack(true);
                thisUnsafePacker.Pack((byte)1);
                thisUnsafePacker.Pack((sbyte)-1);
                thisUnsafePacker.Pack(1.1f);
                thisUnsafePacker.Pack(1.1d);
                thisUnsafePacker.Pack((short)1234);
                thisUnsafePacker.Pack((ushort)1234);
                thisUnsafePacker.Pack((int)1234);
                thisUnsafePacker.Pack((uint)1234);
                thisUnsafePacker.Pack((long)1234);
                thisUnsafePacker.Pack((ulong)1234);
                thisUnsafePacker.Pack("Hello World");
//                thisUnsafePacker.Pack(str);
//                thisUnsafePacker.Pack(bytes);
                thisUnsafePacker.Flush();
            };

            Action thatPack = () =>
            {
                s.Position = 0;
                thatPacker.Pack(false);
                thatPacker.Pack(true);
                thatPacker.Pack((byte)1);
                thatPacker.Pack((sbyte)-1);
                thatPacker.Pack(1.1f);
                thatPacker.Pack(1.1d);
                thatPacker.Pack((short)1234);
                thatPacker.Pack((ushort)1234);
                thatPacker.Pack((int)1234);
                thatPacker.Pack((uint)1234);
                thatPacker.Pack((long)1234);
                thatPacker.Pack((ulong)1234);
                thatPacker.Pack("Hello World");
//                thatPacker.Pack(str);
//                thatPacker.Pack(bytes);
            };

            for (var i = 0; i < 10; i++)
            {
                thisBytePack();
                thisUnsafePack();
                thatPack();
            }

            var sw = Stopwatch.StartNew();

            for (var i = 0; i < loopCount; i++)
                thisBytePack();

            var thisBytePackTime = sw.Elapsed.TotalMilliseconds;

            sw.Restart();

            for (var i = 0; i < loopCount; i++)
                thisUnsafePack();

            var thisUnsafeTime = sw.Elapsed.TotalMilliseconds;

            sw.Restart();

            for (var i = 0; i < loopCount; i++)
                thatPack();

            var thatPackTime = sw.Elapsed.TotalMilliseconds;

            Console.Out.WriteLine($"thisBytePackTime={thisBytePackTime}, thisUnsafeTime={thisUnsafeTime}, thatPackTime={thatPackTime}");

            Assert.True(thisUnsafeTime < thisBytePackTime);
            Assert.True(thisUnsafeTime < thatPackTime);
        }
    }
}