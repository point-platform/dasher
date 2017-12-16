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
using System.Linq;
using System.Reflection;

namespace Dasher.Contracts.Types
{
    /// <summary>
    /// Contract to use when reading a tuple of values of known types.
    /// </summary>
    public sealed class TupleReadContract : ByValueContract, IReadContract
    {
        internal static bool CanProcess(Type type) => TupleWriteContract.CanProcess(type);

        /// <summary>
        /// The contract of each type in the tuple, in the order they appear.
        /// </summary>
        public IReadOnlyList<IReadContract> Items { get; }

        internal TupleReadContract(Type type, ContractCollection contractCollection)
        {
            if (!TupleWriteContract.CanProcess(type))
                throw new ArgumentException($"Type {type} is not a supported tuple type.", nameof(type));

            Items = type.GetGenericArguments().Select(contractCollection.GetOrAddReadContract).ToList();
        }

        internal TupleReadContract(IReadOnlyList<IReadContract> items) => Items = items;

        /// <inheritdoc />
        public bool CanReadFrom(IWriteContract writeContract, bool strict)
        {
            var that = writeContract as TupleWriteContract;

            return that?.Items.Count == Items.Count
                   && !Items.Where((rs, i) => !rs.CanReadFrom(that.Items[i], strict)).Any();
        }

        /// <inheritdoc />
        public override bool Equals(Contract other) => other is TupleReadContract o && o.Items.SequenceEqual(Items);

        /// <inheritdoc />
        protected override int ComputeHashCode()
        {
            unchecked
            {
                var hash = 0;
                foreach (var item in Items)
                {
                    hash <<= 5;
                    hash ^= item.GetHashCode();
                }
                return hash;
            }
        }

        internal override IEnumerable<Contract> Children => Items.Cast<Contract>();

        internal override string MarkupValue => $"{{tuple {string.Join(" ", Items.Select(i => i.ToReferenceString()))}}}";

        /// <inheritdoc />
        public IReadContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new TupleReadContract(Items.Select(i => i.CopyTo(collection)).ToList()));
        }
    }

    /// <summary>
    /// Contract to use when reading a tuple of values of known types.
    /// </summary>
    public sealed class TupleWriteContract : ByValueContract, IWriteContract
    {
        internal static bool CanProcess(Type type)
        {
            if (!type.GetTypeInfo().IsGenericType)
                return false;
            if (!type.IsConstructedGenericType)
                return false;

            var genType = type.GetGenericTypeDefinition();

            return genType == typeof(Tuple<>) ||
                   genType == typeof(Tuple<,>) ||
                   genType == typeof(Tuple<,,>) ||
                   genType == typeof(Tuple<,,,>) ||
                   genType == typeof(Tuple<,,,,>) ||
                   genType == typeof(Tuple<,,,,,>) ||
                   genType == typeof(Tuple<,,,,,,>) ||
                   genType == typeof(Tuple<,,,,,,,>) ||
                   genType == typeof(Tuple<,,,,,,,>);
        }

        /// <summary>
        /// The contract of each type in the tuple, in the order they appear.
        /// </summary>
        public IReadOnlyList<IWriteContract> Items { get; }

        internal TupleWriteContract(Type type, ContractCollection contractCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} is not a supported tuple type.", nameof(type));

            Items = type.GetGenericArguments().Select(contractCollection.GetOrAddWriteContract).ToList();
        }

        internal TupleWriteContract(IReadOnlyList<IWriteContract> items) => Items = items;

        /// <inheritdoc />
        public override bool Equals(Contract other) => other is TupleWriteContract o && o.Items.SequenceEqual(Items);

        /// <inheritdoc />
        protected override int ComputeHashCode()
        {
            unchecked
            {
                var hash = 0;
                foreach (var item in Items)
                {
                    hash <<= 5;
                    hash ^= item.GetHashCode();
                }
                return hash;
            }
        }

        internal override IEnumerable<Contract> Children => Items.Cast<Contract>();

        internal override string MarkupValue => $"{{tuple {string.Join(" ", Items.Select(i => i.ToReferenceString()))}}}";

        /// <inheritdoc />
        public IWriteContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new TupleWriteContract(Items.Select(i => i.CopyTo(collection)).ToList()));
        }
    }
}
