using System;

namespace Dasher.Schemata
{
    public sealed class SchemaParseException : Exception
    {
        public SchemaParseException(string message) : base(message)
        {
        }
    }
}