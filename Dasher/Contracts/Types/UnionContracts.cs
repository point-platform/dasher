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
using System.Xml.Linq;
using Dasher.TypeProviders;
using Dasher.Utils;

namespace Dasher.Contracts.Types
{
    /// <summary>
    /// Contract to use when writing a value having union type.
    /// </summary>
    public sealed class UnionWriteContract : ByRefContract, IWriteContract
    {
        internal static bool CanProcess(Type type) => Union.IsUnionType(type);

        /// <summary>
        /// Details of a member type within a union, for writing.
        /// </summary>
        public struct Member
        {
            /// <summary>
            /// The ID used in serialised format that identifies this member type.
            /// </summary>
            public string Id { get; }

            /// <summary>
            /// The contract to use when writing this member type's value.
            /// </summary>
            public IWriteContract Contract { get; }

            internal Member(string id, IWriteContract contract)
            {
                Id = id;
                Contract = contract;
            }
        }

        /// <summary>
        /// The members that comprise this union type.
        /// </summary>
        public IReadOnlyList<Member> Members { get; }

        internal UnionWriteContract(Type type, ContractCollection contractCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be a union.", nameof(type));
            Members = Union.GetTypes(type)
                .Select(t => new Member(UnionEncoding.GetTypeName(t), contractCollection.GetOrAddWriteContract(t)))
                .OrderBy(m => m.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private UnionWriteContract(IReadOnlyList<Member> members) => Members = members;

        internal UnionWriteContract(XElement element, Func<string, IWriteContract> resolveContract, ICollection<Action> bindActions)
        {
            var members = new List<Member>();

            bindActions.Add(() =>
            {
                foreach (var field in element.Elements(nameof(Member)))
                {
                    var id = field.Attribute(nameof(Member.Id))?.Value;
                    var contract = field.Attribute(nameof(Member.Contract))?.Value;

                    if (string.IsNullOrWhiteSpace(id))
                        throw new ContractParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Member.Id)}\" attribute.");
                    if (string.IsNullOrWhiteSpace(contract))
                        throw new ContractParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Member.Contract)}\" attribute.");

                    members.Add(new Member(id, resolveContract(contract)));
                }
            });

            Members = members;
        }

        /// <inheritdoc />
        public override bool Equals(Contract other)
        {
            return other is UnionWriteContract o && o.Members.SequenceEqual(Members, (a, b) => a.Id == b.Id && a.Contract.Equals(b.Contract));
        }

        /// <inheritdoc />
        protected override int ComputeHashCode()
        {
            unchecked
            {
                var hash = 0;
                foreach (var member in Members)
                {
                    hash <<= 5;
                    hash ^= member.Id.GetHashCode();
                    hash <<= 3;
                    hash ^= member.Contract.GetHashCode();
                }
                return hash;
            }
        }

        internal override IEnumerable<Contract> Children => Members.Select(m => m.Contract).Cast<Contract>();

        internal override XElement ToXml()
        {
            if (Id == null)
                throw new InvalidOperationException("\"Id\" property cannot be null.");
            return new XElement("UnionWrite",
                new XAttribute("Id", Id),
                Members.Select(m => new XElement("Member",
                    new XAttribute("Id", m.Id),
                    new XAttribute("Contract", m.Contract.ToReferenceString()))));
        }

        /// <inheritdoc />
        public IWriteContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new UnionWriteContract(Members.Select(m => new Member(m.Id, m.Contract.CopyTo(collection))).ToList()));
        }
    }

    /// <summary>
    /// Contract to use when reading a value having union type.
    /// </summary>
    public sealed class UnionReadContract : ByRefContract, IReadContract
    {
        internal static bool CanProcess(Type type) => UnionWriteContract.CanProcess(type);

        /// <summary>
        /// Details of a member type within a union, for reading.
        /// </summary>
        public struct Member
        {
            /// <summary>
            /// The ID used in serialised format that identifies this member type.
            /// </summary>
            public string Id { get; }

            /// <summary>
            /// The contract to use when reading this member type's value.
            /// </summary>
            public IReadContract Contract { get; }

            internal Member(string id, IReadContract contract)
            {
                Id = id;
                Contract = contract;
            }
        }

        private IReadOnlyList<Member> Members { get; }

        internal UnionReadContract(Type type, ContractCollection contractCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be a union.", nameof(type));
            Members = Union.GetTypes(type)
                .Select(t => new Member(UnionEncoding.GetTypeName(t), contractCollection.GetOrAddReadContract(t)))
                .OrderBy(m => m.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private UnionReadContract(IReadOnlyList<Member> members) => Members = members;

        internal UnionReadContract(XElement element, Func<string, IReadContract> resolveContract, ICollection<Action> bindActions)
        {
            var members = new List<Member>();

            bindActions.Add(() =>
            {
                foreach (var field in element.Elements(nameof(Member)))
                {
                    var id = field.Attribute(nameof(Member.Id))?.Value;
                    var contract = field.Attribute(nameof(Member.Contract))?.Value;

                    if (string.IsNullOrWhiteSpace(id))
                        throw new ContractParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Member.Id)}\" attribute.");
                    if (string.IsNullOrWhiteSpace(contract))
                        throw new ContractParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Member.Contract)}\" attribute.");

                    members.Add(new Member(id, resolveContract(contract)));
                }
            });

            Members = members;
        }

        /// <inheritdoc />
        public bool CanReadFrom(IWriteContract writeContract, bool strict)
        {
            // TODO write EmptyContract test for this case
            if (writeContract is EmptyContract)
                return true;

            if (!(writeContract is UnionWriteContract ws))
                return false;

            var readMembers = Members;
            var writeMembers = ws.Members;

            var ir = 0;
            var iw = 0;

            while (iw < writeMembers.Count)
            {
                if (ir == readMembers.Count)
                    return false;

                var rm = readMembers[ir];
                var wm = writeMembers[iw];

                var cmp = StringComparer.OrdinalIgnoreCase.Compare(rm.Id, wm.Id);

                if (cmp == 0)
                {
                    // match
                    if (!rm.Contract.CanReadFrom(wm.Contract, strict))
                        return false;

                    // step both forwards
                    ir++;
                    iw++;
                }
                else if (cmp < 0)
                {
                    // read member comes before write member -- read type contains an extra member
                    if (strict)
                        return false;
                    // skip the read member only
                    iw++;
                }
                else
                {
                    return false;
                }
            }

            if (ir != readMembers.Count && strict)
                return false;

            return true;
        }

        /// <inheritdoc />
        public override bool Equals(Contract other)
        {
            return other is UnionReadContract o && o.Members.SequenceEqual(Members, (a, b) => a.Id == b.Id && a.Contract.Equals(b.Contract));
        }

        /// <inheritdoc />
        protected override int ComputeHashCode()
        {
            unchecked
            {
                var hash = 0;
                foreach (var member in Members)
                {
                    hash <<= 5;
                    hash ^= member.Id.GetHashCode();
                    hash <<= 3;
                    hash ^= member.Contract.GetHashCode();
                }
                return hash;
            }
        }

        internal override IEnumerable<Contract> Children => Members.Select(m => m.Contract).Cast<Contract>();

        internal override XElement ToXml()
        {
            if (Id == null)
                throw new InvalidOperationException("\"Id\" property cannot be null.");
            return new XElement("UnionRead",
                new XAttribute("Id", Id),
                Members.Select(m => new XElement("Member",
                    new XAttribute("Id", m.Id),
                    new XAttribute("Contract", m.Contract.ToReferenceString()))));
        }

        /// <inheritdoc />
        public IReadContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new UnionReadContract(Members.Select(m => new Member(m.Id, m.Contract.CopyTo(collection))).ToList()));
        }
    }
}
