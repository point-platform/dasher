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

namespace Dasher.Contracts.Types
{
    /// <summary>
    /// Contract to use when writing a nullable value.
    /// </summary>
    public sealed class NullableWriteContract : ByValueContract, IWriteContract
    {
        internal static bool CanProcess(Type type) => Nullable.GetUnderlyingType(type) != null;

        /// <summary>
        /// The contract to use when writing the inner type.
        /// </summary>
        public IWriteContract Inner { get; }

        internal NullableWriteContract(Type type, ContractCollection contractCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be nullable.", nameof(type));
            Inner = contractCollection.GetOrAddWriteContract(Nullable.GetUnderlyingType(type));
        }

        internal NullableWriteContract(IWriteContract inner) => Inner = inner;

        /// <inheritdoc />
        public override bool Equals(Contract other)
        {
            return other is NullableWriteContract o && ((Contract)o.Inner).Equals((Contract)Inner);
        }

        /// <inheritdoc />
        protected override int ComputeHashCode() => unchecked(0x3731AFBB ^ Inner.GetHashCode());

        internal override IEnumerable<Contract> Children => new[] { (Contract)Inner };

        internal override string MarkupValue => $"{{nullable {Inner.ToReferenceString()}}}";

        /// <inheritdoc />
        public IWriteContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new NullableWriteContract(Inner.CopyTo(collection)));
        }
    }

    /// <summary>
    /// Contract to use when reading a nullable value.
    /// </summary>
    public sealed class NullableReadContract : ByValueContract, IReadContract
    {
        internal static bool CanProcess(Type type) => NullableWriteContract.CanProcess(type);

        /// <summary>
        /// The contract to use when reading the inner type.
        /// </summary>
        public IReadContract Inner { get; }

        internal NullableReadContract(Type type, ContractCollection contractCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be nullable.", nameof(type));
            Inner = contractCollection.GetOrAddReadContract(Nullable.GetUnderlyingType(type));
        }

        internal NullableReadContract(IReadContract inner) => Inner = inner;

        /// <inheritdoc />
        public bool CanReadFrom(IWriteContract writeContract, bool strict)
        {
            if (writeContract is NullableWriteContract ws)
                return Inner.CanReadFrom(ws.Inner, strict);

            if (strict)
                return false;

            return Inner.CanReadFrom(writeContract, strict);
        }

        /// <inheritdoc />
        public override bool Equals(Contract other)
        {
            return other is NullableReadContract o && ((Contract)o.Inner).Equals((Contract)Inner);
        }

        /// <inheritdoc />
        protected override int ComputeHashCode() => unchecked(0x563D4345 ^ Inner.GetHashCode());

        internal override IEnumerable<Contract> Children => new[] { (Contract)Inner };

        internal override string MarkupValue => $"{{nullable {Inner.ToReferenceString()}}}";

        /// <inheritdoc />
        public IReadContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new NullableReadContract(Inner.CopyTo(collection)));
        }
    }
}
