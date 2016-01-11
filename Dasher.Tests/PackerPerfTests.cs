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
using System.Diagnostics;
using System.IO;
using System.Linq;
using MsgPack;
using Xunit;
using Xunit.Abstractions;

namespace Dasher.Tests
{
    public sealed class PackerPerfTests
    {
        private ITestOutputHelper TestOutput { get; }

        public PackerPerfTests(ITestOutputHelper testOutput)
        {
            TestOutput = testOutput;
        }

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

            TestOutput.WriteLine("oneByOneTime = {0}", oneByOneTime);
            TestOutput.WriteLine("inChunksTime = {0}", inChunksTime);

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

            TestOutput.WriteLine($"thisBytePackTime={thisBytePackTime}, thisUnsafeTime={thisUnsafeTime}, thatPackTime={thatPackTime}");

            Assert.True(thisUnsafeTime < thisBytePackTime);
            Assert.True(thisUnsafeTime < thatPackTime);
        }
    }
}