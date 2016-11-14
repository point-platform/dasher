using System;

namespace Dasher.Schema
{
    public sealed class SchemaParseException : Exception
    {
        public SchemaParseException(string message) : base(message)
        {
        }
    }
}