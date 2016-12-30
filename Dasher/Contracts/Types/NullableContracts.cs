#region License
//
// Dasher
//
// Copyright 2015-2016 Drew Noakes
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
    internal sealed class NullableWriteContract : ByValueContract, IWriteContract
    {
        public static bool CanProcess(Type type) => Nullable.GetUnderlyingType(type) != null;

        public IWriteContract Inner { get; }

        public NullableWriteContract(Type type, ContractCollection contractCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be nullable.", nameof(type));
            Inner = contractCollection.GetOrAddWriteContract(Nullable.GetUnderlyingType(type));
        }

        public NullableWriteContract(IWriteContract inner)
        {
            Inner = inner;
        }

        public override bool Equals(Contract other)
        {
            var o = other as NullableWriteContract;
            return o != null && ((Contract)o.Inner).Equals((Contract)Inner);
        }

        protected override int ComputeHashCode() => unchecked(0x3731AFBB ^ Inner.GetHashCode());

        internal override IEnumerable<Contract> Children => new[] { (Contract)Inner };

        internal override string MarkupValue => $"{{nullable {Inner.ToReferenceString()}}}";

        public IWriteContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new NullableWriteContract(Inner.CopyTo(collection)));
        }
    }

    internal sealed class NullableReadContract : ByValueContract, IReadContract
    {
        public static bool CanProcess(Type type) => NullableWriteContract.CanProcess(type);

        private IReadContract Inner { get; }

        public NullableReadContract(Type type, ContractCollection contractCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be nullable.", nameof(type));
            Inner = contractCollection.GetOrAddReadContract(Nullable.GetUnderlyingType(type));
        }

        public NullableReadContract(IReadContract inner)
        {
            Inner = inner;
        }

        public bool CanReadFrom(IWriteContract writeContract, bool strict)
        {
            var ws = writeContract as NullableWriteContract;

            if (ws != null)
                return Inner.CanReadFrom(ws.Inner, strict);

            if (strict)
                return false;

            return Inner.CanReadFrom(writeContract, strict);
        }

        public override bool Equals(Contract other)
        {
            var o = other as NullableReadContract;
            return o != null && ((Contract)o.Inner).Equals((Contract)Inner);
        }

        protected override int ComputeHashCode() => unchecked(0x563D4345 ^ Inner.GetHashCode());

        internal override IEnumerable<Contract> Children => new[] { (Contract)Inner };

        internal override string MarkupValue => $"{{nullable {Inner.ToReferenceString()}}}";

        public IReadContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new NullableReadContract(Inner.CopyTo(collection)));
        }
    }
}