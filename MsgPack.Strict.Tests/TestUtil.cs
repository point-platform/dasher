using System;
using System.IO;

namespace MsgPack.Strict.Tests
{
    public static class TestUtil
    {
        public static byte[] PackBytes(Action<Packer> packAction)
        {
            var stream = new MemoryStream();
            var packer = Packer.Create(stream, PackerCompatibilityOptions.None);
            packAction(packer);
            stream.Position = 0;
            return stream.GetBuffer();
        }
    }
}