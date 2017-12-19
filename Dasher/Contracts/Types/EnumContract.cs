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
using System.Xml.Linq;
using Dasher.Utils;

namespace Dasher.Contracts.Types
{
    /// <summary>
    /// Contract to use when reading or writing enum values.
    /// </summary>
    public sealed class EnumContract : ByRefContract, IWriteContract, IReadContract
    {
        internal static bool CanProcess(Type type) => type.GetTypeInfo().IsEnum;

        private readonly HashSet<string> _memberNames;

        /// <summary>
        /// The set of member names present in this contract.
        /// </summary>
#if NET45
        public IEnumerable<string> MemberNames => _memberNames;
#else
        public IReadOnlyCollection<string> MemberNames => _memberNames;
#endif

        internal EnumContract(Type type)
        {
            if (!CanProcess(type))
                throw new ArgumentException("Must be an enum.", nameof(type));
            _memberNames = new HashSet<string>(Enum.GetNames(type), StringComparer.OrdinalIgnoreCase);
        }

        internal EnumContract(XContainer element)
        {
            _memberNames = new HashSet<string>(element.Elements("Member").Select(e => e.Attribute("Name").Value));
        }

        /// <inheritdoc />
        public bool CanReadFrom(IWriteContract writeContract, bool strict)
        {
            if (!(writeContract is EnumContract that))
                return false;

            return strict
                ? _memberNames.SetEquals(that.MemberNames)
                : _memberNames.IsSupersetOf(that.MemberNames);
        }

        /// <inheritdoc />
        public override IEnumerable<Contract> Children => EmptyArray<Contract>.Instance;

        /// <inheritdoc />
        public override bool Equals(Contract other) => other is EnumContract e && _memberNames.SetEquals(e._memberNames);

        /// <inheritdoc />
        protected override int ComputeHashCode()
        {
            unchecked
            {
                var hash = 0;
                foreach (var memberName in _memberNames)
                {
                    hash <<= 5;
                    hash ^= memberName.GetHashCode();
                }
                return hash;
            }
        }

        internal override XElement ToXml()
        {
            if (Id == null)
                throw new InvalidOperationException("\"Id\" property cannot be null.");
            return new XElement("Enum",
                new XAttribute("Id", Id),
                _memberNames.Select(m => new XElement("Member", new XAttribute("Name", m))));
        }

        IReadContract IReadContract.CopyTo(ContractCollection collection) => collection.Intern(this);
        IWriteContract IWriteContract.CopyTo(ContractCollection collection) => collection.Intern(this);
    }
}
