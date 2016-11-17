using System;
using System.Diagnostics.CodeAnalysis;

namespace Dasher
{
    [SuppressMessage("ReSharper", "ConvertToStaticClass")]
    public sealed class Empty
    {
        private Empty()
        {
            throw new NotSupportedException("Not for instantiation.");
        }
    }
}