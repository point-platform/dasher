using System;

namespace MsgPack.Strict
{
    public sealed class StrictDeserialisationException : Exception
    {
        public Type TargetType { get; }

        public StrictDeserialisationException(string message, Type targetType)
            : base(message)
        {
            TargetType = targetType;
        }
    }
}