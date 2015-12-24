using System;

namespace Dasher
{
    public sealed class DeserialisationException : Exception
    {
        public Type TargetType { get; }

        public DeserialisationException(string message, Type targetType)
            : base(message)
        {
            TargetType = targetType;
        }
    }
}