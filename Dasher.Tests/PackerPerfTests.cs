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

        [Fact(Skip = "Performance test")]
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

        [Fact(Skip = "Performance test")]
        public void PackerPerf()
        {
            var s = new MemoryStream();

            var dasherPacker = new Packer(s);
            var msgPackCliPacker = MsgPack.Packer.Create(s);

//            var str = new string('a', 256);
//            var bytes = new byte[256];

            const int loopCount = 1024 * 1024;

            Action dasherPack = () =>
            {
                s.Position = 0;
                dasherPacker.Pack(false);
                dasherPacker.Pack(true);
                dasherPacker.Pack((byte)1);
                dasherPacker.Pack((sbyte)-1);
                dasherPacker.Pack(1.1f);
                dasherPacker.Pack(1.1d);
                dasherPacker.Pack((short)1234);
                dasherPacker.Pack((ushort)1234);
                dasherPacker.Pack((int)1234);
                dasherPacker.Pack((uint)1234);
                dasherPacker.Pack((long)1234);
                dasherPacker.Pack((ulong)1234);
                dasherPacker.Pack("Hello World");
//                dasherPacker.Pack(str);
//                dasherPacker.Pack(bytes);
                dasherPacker.Flush();
            };

            Action msgPackCliPack = () =>
            {
                s.Position = 0;
                msgPackCliPacker.Pack(false);
                msgPackCliPacker.Pack(true);
                msgPackCliPacker.Pack((byte)1);
                msgPackCliPacker.Pack((sbyte)-1);
                msgPackCliPacker.Pack(1.1f);
                msgPackCliPacker.Pack(1.1d);
                msgPackCliPacker.Pack((short)1234);
                msgPackCliPacker.Pack((ushort)1234);
                msgPackCliPacker.Pack((int)1234);
                msgPackCliPacker.Pack((uint)1234);
                msgPackCliPacker.Pack((long)1234);
                msgPackCliPacker.Pack((ulong)1234);
                msgPackCliPacker.Pack("Hello World");
//                msgPackCliPacker.Pack(str);
//                msgPackCliPacker.Pack(bytes);
            };

            for (var i = 0; i < 10; i++)
            {
                dasherPack();
                msgPackCliPack();
            }

            var sw = Stopwatch.StartNew();

            for (var i = 0; i < loopCount; i++)
                dasherPack();

            var unsafePackTime = sw.Elapsed.TotalMilliseconds;

            sw.Restart();

            for (var i = 0; i < loopCount; i++)
                msgPackCliPack();

            var msgPackCliPackTime = sw.Elapsed.TotalMilliseconds;

            TestOutput.WriteLine($"{nameof(unsafePackTime)}={unsafePackTime}, {nameof(msgPackCliPackTime)}={msgPackCliPackTime}");
        }
    }
}