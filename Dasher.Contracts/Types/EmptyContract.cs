using System.Collections.Generic;
using Dasher.Contracts.Utils;

namespace Dasher.Contracts.Types
{
    internal sealed class EmptyContract : ByValueContract, IWriteContract, IReadContract
    {
        public bool CanReadFrom(IWriteContract writeContract, bool strict) => writeContract is EmptyContract || !strict;

        public override bool Equals(Contract other) => other is EmptyContract;

        internal override IEnumerable<Contract> Children => EmptyArray<Contract>.Instance;

        internal override string MarkupValue => "{empty}";

        protected override int ComputeHashCode() => MarkupValue.GetHashCode();

        IWriteContract IWriteContract.CopyTo(ContractCollection collection) => collection.Intern(this);
        IReadContract IReadContract.CopyTo(ContractCollection collection) => collection.Intern(this);
    }
}