using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dasher.Contracts.Utils;

namespace Dasher.Contracts.Types
{
    internal sealed class ListWriteContract : ByValueContract, IWriteContract
    {
        public static bool CanProcess(Type type) => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>);

        public IWriteContract ItemContract { get; }

        public ListWriteContract(Type type, ContractCollection contractCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyList<int>)}<>.", nameof(type));
            ItemContract = contractCollection.GetOrAddWriteContract(type.GetGenericArguments().Single());
        }

        public ListWriteContract(IWriteContract itemContract)
        {
            ItemContract = itemContract;
        }

        public override bool Equals(Contract other)
        {
            var o = other as ListWriteContract;
            return o != null && ((Contract)o.ItemContract).Equals((Contract)ItemContract);
        }

        protected override int ComputeHashCode() => unchecked((int)0xA4A76926 ^ ItemContract.GetHashCode());

        internal override IEnumerable<Contract> Children => new[] { (Contract)ItemContract };

        internal override string MarkupValue => $"{{list {ItemContract.ToReferenceString()}}}";

        public IWriteContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new ListWriteContract(ItemContract.CopyTo(collection)));
        }
    }

    internal sealed class ListReadContract : ByValueContract, IReadContract
    {
        public static bool CanProcess(Type type) => ListWriteContract.CanProcess(type);

        private IReadContract ItemContract { get; }

        public ListReadContract(Type type, ContractCollection contractCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyList<int>)}<>.", nameof(type));
            ItemContract = contractCollection.GetOrAddReadContract(type.GetGenericArguments().Single());
        }

        public ListReadContract(IReadContract itemContract)
        {
            ItemContract = itemContract;
        }

        public bool CanReadFrom(IWriteContract writeContract, bool strict)
        {
            var ws = writeContract as ListWriteContract;
            return ws != null && ItemContract.CanReadFrom(ws.ItemContract, strict);
        }

        public override bool Equals(Contract other) => (other as ListReadContract)?.ItemContract.Equals(ItemContract) ?? false;

        protected override int ComputeHashCode() => unchecked((int)0x9ABCF854 ^ ItemContract.GetHashCode());

        internal override IEnumerable<Contract> Children => new[] { (Contract)ItemContract };

        internal override string MarkupValue => $"{{list {ItemContract.ToReferenceString()}}}";

        public IReadContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new ListReadContract(ItemContract.CopyTo(collection)));
        }
    }
}