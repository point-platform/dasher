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
    internal sealed class EnumContract : ByRefContract, IWriteContract, IReadContract
    {
        public static bool CanProcess(Type type) => type.GetTypeInfo().IsEnum;

        private HashSet<string> MemberNames { get; }

        public EnumContract(Type type)
        {
            if (!CanProcess(type))
                throw new ArgumentException("Must be an enum.", nameof(type));
            MemberNames = new HashSet<string>(Enum.GetNames(type), StringComparer.OrdinalIgnoreCase);
        }

        public EnumContract(XContainer element)
        {
            MemberNames = new HashSet<string>(element.Elements("Member").Select(e => e.Attribute("Name").Value));
        }

        public bool CanReadFrom(IWriteContract writeContract, bool strict)
        {
            var that = writeContract as EnumContract;
            if (that == null)
                return false;
            return strict
                ? MemberNames.SetEquals(that.MemberNames)
                : MemberNames.IsSupersetOf(that.MemberNames);
        }

        internal override IEnumerable<Contract> Children => EmptyArray<Contract>.Instance;

        public override bool Equals(Contract other) => other is EnumContract e && MemberNames.SetEquals(e.MemberNames);

        protected override int ComputeHashCode()
        {
            unchecked
            {
                var hash = 0;
                foreach (var memberName in MemberNames)
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
                MemberNames.Select(m => new XElement("Member", new XAttribute("Name", m))));
        }

        IReadContract IReadContract.CopyTo(ContractCollection collection) => collection.Intern(this);
        IWriteContract IWriteContract.CopyTo(ContractCollection collection) => collection.Intern(this);
    }
}