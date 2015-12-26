using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgPack.Strict.SchemaGenerator
{
    public sealed class SchemaGenerationException : Exception
    {
        public Type TargetType { get; }

        public SchemaGenerationException(string message, Type targetType)
            : base(message)
        {
            TargetType = targetType;
        }
    }
}
