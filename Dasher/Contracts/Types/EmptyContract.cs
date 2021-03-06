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

using System.Collections.Generic;
using Dasher.Utils;

namespace Dasher.Contracts.Types
{
    /// <summary>
    /// Contract to use when reading or writing the <see cref="Empty"/> value.
    /// </summary>
    public sealed class EmptyContract : ByValueContract, IWriteContract, IReadContract
    {
        internal EmptyContract() {}

        /// <inheritdoc />
        public bool CanReadFrom(IWriteContract writeContract, bool strict) => writeContract is EmptyContract || !strict;

        /// <inheritdoc />
        public override bool Equals(Contract other) => other is EmptyContract;

        /// <inheritdoc />
        public override IEnumerable<Contract> Children => EmptyArray<Contract>.Instance;

        /// <inheritdoc />
        public override string MarkupValue => "{empty}";

        /// <inheritdoc />
        protected override int ComputeHashCode() => MarkupValue.GetHashCode();

        IWriteContract IWriteContract.CopyTo(ContractCollection collection) => collection.Intern(this);
        IReadContract IReadContract.CopyTo(ContractCollection collection) => collection.Intern(this);
    }
}
