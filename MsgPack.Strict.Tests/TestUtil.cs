using System;
using System.IO;

namespace MsgPack.Strict.Tests
{
    public static class TestUtil
    {
        public static byte[] PackBytes(Action<MsgPackPacker> packAction)
        {
            var stream = new MemoryStream();
            var packer = MsgPackPacker.Create(stream);
            packAction(packer);
            stream.Position = 0;
            return stream.GetBuffer();
        }
    }
}