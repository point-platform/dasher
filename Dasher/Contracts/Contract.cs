#region License
//
// Dasher
//
// Copyright 2015-2017 Drew Noakes
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
// More information about this project is available at:
//
//    https://github.com/drewnoakes/dasher
//
#endregion

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace Dasher.Contracts
{
    /// <summary>
    /// Defines a contract that represents how data is serialised.
    /// </summary>
    public interface IWriteContract
    {
        /// <summary>
        /// Creates a deep copy of this contract within <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        IWriteContract CopyTo(ContractCollection collection);
    }

    /// <summary>
    /// Defines a contract that represents how data is deserialised.
    /// </summary>
    public interface IReadContract
    {
        /// <summary>
        /// Compute whether data written according to <paramref name="writeContract"/> may be
        /// read under this contract.
        /// </summary>
        /// <param name="writeContract">The contract to test compatibility against.</param>
        /// <param name="strict">Whether the comparison allows any leniency or not.</param>
        /// <returns><c>true</c> if the contracts are compatible, otherwise <c>false</c>.</returns>
        bool CanReadFrom(IWriteContract writeContract, bool strict);

        /// <summary>
        /// Creates a deep copy of this contract within <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        IReadContract CopyTo(ContractCollection collection);
    }

    /// <summary>
    /// Base class for all contracts.
    /// </summary>
    /// <remarks>
    /// Additionally, contract classes must derive from either <see cref="ByRefContract"/> or
    /// <see cref="ByValueContract"/> and implement one or both of <see cref="IWriteContract"/>
    /// and <see cref="IReadContract"/>.
    /// </remarks>
    public abstract class Contract
    {
        internal abstract IEnumerable<Contract> Children { get; }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Contract other && Equals(other);

        /// <summary>
        /// Determines whether the specified contract is equal to this one.
        /// Equality means that they have identical specifications in every way.
        /// </summary>
        /// <param name="other">The contract to compare against.</param>
        /// <returns><c>true</c> if contracts are equal, otherwise <c>false</c>.</returns>
        public abstract bool Equals(Contract other);

        /// <inheritdoc />
        public sealed override int GetHashCode() => ComputeHashCode();

        /// <summary>
        /// Computes a hash code for the contract.
        /// </summary>
        /// <returns>The hash code for this contract.</returns>
        protected abstract int ComputeHashCode();
    }

    /// <summary>
    /// Base class of contracts that are annotated by reference.
    /// </summary>
    /// <remarks>For complex, union and enum.</remarks>
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

        /// <inheritdoc />
        public override string ToString() => Id ?? GetType().Name;
    }

    /// <summary>
    /// Base class of contracts that are annotated by value.
    /// </summary>
    /// <remarks>For primitive, nullable, list, dictionary, tuple, empty.</remarks>
    public abstract class ByValueContract : Contract
    {
        internal abstract string MarkupValue { get; }

        /// <inheritdoc />
        public override string ToString() => MarkupValue;
    }
}