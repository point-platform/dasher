using System;

namespace Dasher.Contracts
{
    public sealed class ContractParseException : Exception
    {
        public ContractParseException(string message) : base(message)
        {
        }
    }
}