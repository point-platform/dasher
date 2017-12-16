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
using System.Reflection;

namespace Dasher.Contracts.Types
{
    internal sealed class DictionaryWriteContract : ByValueContract, IWriteContract
    {
        public static bool CanProcess(Type type) => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>);

        public IWriteContract KeyContract { get; }
        public IWriteContract ValueContract { get; }

        public DictionaryWriteContract(Type type, ContractCollection contractCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyDictionary<int, int>)}<,>.", nameof(type));
            KeyContract = contractCollection.GetOrAddWriteContract(type.GetGenericArguments()[0]);
            ValueContract = contractCollection.GetOrAddWriteContract(type.GetGenericArguments()[1]);
        }

        public DictionaryWriteContract(IWriteContract keyContract, IWriteContract valueContract)
        {
            KeyContract = keyContract;
            ValueContract = valueContract;
        }

        public override bool Equals(Contract other)
        {
            return other is DictionaryWriteContract o && o.KeyContract.Equals(KeyContract) && o.ValueContract.Equals(ValueContract);
        }

        protected override int ComputeHashCode()
        {
            unchecked
            {
                var hash = KeyContract.GetHashCode();
                hash <<= 5;
                hash ^= ValueContract.GetHashCode();
                return hash;
            }
        }

        internal override IEnumerable<Contract> Children => new[] { (Contract)KeyContract, (Contract)ValueContract };

        internal override string MarkupValue => $"{{dictionary {KeyContract.ToReferenceString()} {ValueContract.ToReferenceString()}}}";

        public IWriteContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new DictionaryWriteContract(KeyContract.CopyTo(collection), ValueContract.CopyTo(collection)));
        }
    }

    internal sealed class DictionaryReadContract : ByValueContract, IReadContract
    {
        public static bool CanProcess(Type type) => DictionaryWriteContract.CanProcess(type);

        private IReadContract KeyContract { get; }
        private IReadContract ValueContract { get; }

        public DictionaryReadContract(Type type, ContractCollection contractCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyDictionary<int, int>)}<,>.", nameof(type));
            KeyContract = contractCollection.GetOrAddReadContract(type.GetGenericArguments()[0]);
            ValueContract = contractCollection.GetOrAddReadContract(type.GetGenericArguments()[1]);
        }

        public DictionaryReadContract(IReadContract keyContract, IReadContract valueContract)
        {
            KeyContract = keyContract;
            ValueContract = valueContract;
        }

        public bool CanReadFrom(IWriteContract writeContract, bool strict)
        {
            return writeContract is DictionaryWriteContract ws &&
                   KeyContract.CanReadFrom(ws.KeyContract, strict) &&
                   ValueContract.CanReadFrom(ws.ValueContract, strict);
        }

        public override bool Equals(Contract other)
        {
            return other is DictionaryReadContract o && o.KeyContract.Equals(KeyContract) && o.ValueContract.Equals(ValueContract);
        }

        protected override int ComputeHashCode()
        {
            unchecked
            {
                var hash = KeyContract.GetHashCode();
                hash <<= 5;
                hash ^= ValueContract.GetHashCode();
                return hash;
            }
        }

        internal override IEnumerable<Contract> Children => new[] { (Contract)KeyContract, (Contract)ValueContract };

        internal override string MarkupValue => $"{{dictionary {KeyContract.ToReferenceString()} {ValueContract.ToReferenceString()}}}";

        public IReadContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new DictionaryReadContract(KeyContract.CopyTo(collection), ValueContract.CopyTo(collection)));
        }
    }
}
