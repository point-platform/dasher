using System;

namespace Dasher
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class UnionTagAttribute : Attribute
    {
        public string Tag { get; }

        public UnionTagAttribute(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentException("Cannot be null or whitespace.", nameof(tag));

            Tag = tag;
        }
    }
}