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
using Dasher.Utils;

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
                {typeof(char), "Char"},
                {typeof(string), "String"},
                {typeof(byte[]), "ByteArray"},
                {typeof(ArraySegment<byte>), "ByteArraySegment"},
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
            if (!_nameByType.TryGetValue(type, out string name))
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

        public override bool Equals(Contract other) => other is PrimitiveContract contract && contract.TypeName == TypeName;

        protected override int ComputeHashCode() => TypeName.GetHashCode();

        internal override IEnumerable<Contract> Children => EmptyArray<Contract>.Instance;

        internal override string MarkupValue => TypeName;

        IWriteContract IWriteContract.CopyTo(ContractCollection collection) => collection.Intern(this);
        IReadContract IReadContract.CopyTo(ContractCollection collection) => collection.Intern(this);
    }
}