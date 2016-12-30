using System;
using System.Collections.Generic;
using System.Linq;
using Dasher.Contracts.Utils;

namespace Dasher.Contracts.Types
{
    internal sealed class PrimitiveContract : ByValueContract, IWriteContract, IReadContract
    {
        private static readonly Dictionary<Type, string> _nameByType;
        private static readonly Dictionary<string, Type> _typeByName;

        static PrimitiveContract()
        {
            _nameByType = new Dictionary<Type, string>
            {
                {typeof(byte), "Byte"},
                {typeof(sbyte), "SByte"},
                {typeof(short), "Int16"},
                {typeof(ushort), "UInt16"},
                {typeof(int), "Int32"},
                {typeof(uint), "UInt32"},
                {typeof(long), "Int64"},
                {typeof(ulong), "UInt64"},
                {typeof(float), "Single"},
                {typeof(double), "Double"},
                {typeof(bool), "Boolean"},
                {typeof(string), "String"},
                {typeof(byte[]), "ByteArray"},
                {typeof(decimal), "Decimal"},
                {typeof(DateTime), "DateTime"},
                {typeof(DateTimeOffset), "DateTimeOffset"},
                {typeof(TimeSpan), "TimeSpan"},
                {typeof(IntPtr), "IntPtr"},
                {typeof(Guid), "Guid"},
                {typeof(Version), "Version"}
            };

            _typeByName = _nameByType.ToDictionary(p => p.Value, p => p.Key);
        }

        public static bool CanProcess(Type type) => _nameByType.ContainsKey(type);

        private string TypeName { get; }

        public PrimitiveContract(Type type)
        {
            string name;
            if (!_nameByType.TryGetValue(type, out name))
                throw new ArgumentException($"Type {type} is not a supported primitive.", nameof(type));
            TypeName = name;
        }

        public PrimitiveContract(string typeName)
        {
            if (!_typeByName.ContainsKey(typeName))
                throw new ContractParseException($"Invalid primitive contract name \"{typeName}\".");
            TypeName = typeName;
        }

        public bool CanReadFrom(IWriteContract writeContract, bool strict) => Equals(writeContract);

        public override bool Equals(Contract other)
        {
            var contract = other as PrimitiveContract;
            return contract != null && contract.TypeName == TypeName;
        }

        protected override int ComputeHashCode() => TypeName.GetHashCode();

        internal override IEnumerable<Contract> Children => EmptyArray<Contract>.Instance;

        internal override string MarkupValue => TypeName;

        IWriteContract IWriteContract.CopyTo(ContractCollection collection) => collection.Intern(this);
        IReadContract IReadContract.CopyTo(ContractCollection collection) => collection.Intern(this);
    }
}