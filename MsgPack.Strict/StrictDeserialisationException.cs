using System;

namespace MsgPack.Strict
{
    public sealed class StrictDeserialisationException : Exception
    {
        public Type TargetType { get; }

        public StrictDeserialisationException(Type targetType, string message)
            : base(message)
        {
            TargetType = targetType;
        }
    }
}