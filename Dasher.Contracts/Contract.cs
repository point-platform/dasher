using System;
using System.Collections.Generic;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace Dasher.Contracts
{
    public interface IWriteContract
    {
        /// <summary>
        /// Creates a deep copy of this contract within <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        IWriteContract CopyTo(ContractCollection collection);
    }

    public interface IReadContract
    {
        bool CanReadFrom(IWriteContract writeContract, bool strict);

        /// <summary>
        /// Creates a deep copy of this contract within <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        IReadContract CopyTo(ContractCollection collection);
    }

    public abstract class Contract
    {
        internal abstract IEnumerable<Contract> Children { get; }

        public override bool Equals(object obj)
        {
            var other = obj as Contract;
            return other != null && Equals(other);
        }

        public abstract bool Equals(Contract other);

        public override int GetHashCode() => ComputeHashCode();

        protected abstract int ComputeHashCode();
    }

    /// <summary>For complex, union and enum.</summary>
    public abstract class ByRefContract : Contract
    {
        private string _id;

        [CanBeNull]
        internal string Id
        {
            get { return _id; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Must be non-blank string.");
                _id = value;
            }
        }

        internal abstract XElement ToXml();
        public override string ToString() => Id ?? GetType().Name;
    }

    /// <summary>For primitive, nullable, list, dictionary, tuple, empty.</summary>
    public abstract class ByValueContract : Contract
    {
        internal abstract string MarkupValue { get; }
        public override string ToString() => MarkupValue;
    }
}