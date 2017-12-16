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
    /// Contract to use when writing complex types.
    /// </summary>
    public sealed class ComplexWriteContract : ByRefContract, IWriteContract
    {
        /// <summary>
        /// Details of a complex type's field, for writing.
        /// </summary>
        public struct Field
        {
            /// <summary>
            /// Name of the field.
            /// </summary>
            /// <remarks>
            /// Names are case in-sensitive.
            /// </remarks>
            public string Name { get; }

            /// <summary>
            /// The contract to use when writing this field's value.
            /// </summary>
            public IWriteContract Contract { get; }

            internal Field(string name, IWriteContract contract)
            {
                Name = name;
                Contract = contract;
            }
        }

        /// <summary>
        /// The fields that comprise this complex type.
        /// </summary>
        public IReadOnlyList<Field> Fields { get; }

        internal ComplexWriteContract(Type type, ContractCollection contractCollection)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase);
            Fields = properties.Select(p => new Field(p.Name,
                p.PropertyType == type
                    ? this
                    : contractCollection.GetOrAddWriteContract(p.PropertyType))).ToArray();
            if (!Fields.Any())
                throw new ArgumentException($"Type {type} must have at least one public instance property.", nameof(type));
        }

        private ComplexWriteContract(IReadOnlyList<Field> fields) => Fields = fields;

        internal ComplexWriteContract(XElement element, Func<string, IWriteContract> resolveContract, ICollection<Action> bindActions)
        {
            var fields = new List<Field>();

            bindActions.Add(() =>
            {
                foreach (var field in element.Elements(nameof(Field)))
                {
                    var name = field.Attribute(nameof(Field.Name))?.Value;
                    var contract = field.Attribute(nameof(Field.Contract))?.Value;

                    if (string.IsNullOrWhiteSpace(name))
                        throw new ContractParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Field.Name)}\" attribute.");
                    if (string.IsNullOrWhiteSpace(contract))
                        throw new ContractParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Field.Contract)}\" attribute.");

                    fields.Add(new Field(name, resolveContract(contract)));
                }
            });

            Fields = fields;
        }

        /// <inheritdoc />
        public override bool Equals(Contract other)
        {
            return other is ComplexWriteContract cwc &&
                   cwc.Fields.SequenceEqual(Fields, (a, b) => a.Name == b.Name && a.Contract.Equals(b.Contract));
        }

        /// <inheritdoc />
        protected override int ComputeHashCode()
        {
            unchecked
            {
                var hash = 0;
                foreach (var field in Fields)
                {
                    hash <<= 5;
                    hash ^= field.Name.GetHashCode();
                    hash <<= 3;
                    hash ^= field.Contract.GetHashCode();
                }

                return hash;
            }
        }

        internal override IEnumerable<Contract> Children => Fields.Select(f => f.Contract).Cast<Contract>();

        internal override XElement ToXml()
        {
            if (Id == null)
                throw new InvalidOperationException($"\"{nameof(Id)}\" property cannot be null.");
            return new XElement("ComplexWrite",
                new XAttribute("Id", Id),
                Fields.Select(f => new XElement("Field",
                    new XAttribute("Name", f.Name),
                    new XAttribute("Contract", f.Contract.ToReferenceString()))));
        }

        /// <inheritdoc />
        public IWriteContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new ComplexWriteContract(Fields.Select(f => new Field(f.Name, f.Contract.CopyTo(collection))).ToList()));
        }
    }

    /// <summary>
    /// Contract to use when reading complex types.
    /// </summary>
    internal sealed class ComplexReadContract : ByRefContract, IReadContract
    {
        /// <summary>
        /// Details of a complex type's field, for writing.
        /// </summary>
        public struct Field
        {
            /// <summary>
            /// Name of the field.
            /// </summary>
            /// <remarks>
            /// Names are case in-sensitive.
            /// </remarks>
            public string Name { get; }

            /// <summary>
            /// The contract to use when reading this field's value.
            /// </summary>
            public IReadContract Contract { get; }

            /// <summary>
            /// Whether the field is required when reading this complex type.
            /// </summary>
            public bool IsRequired { get; }

            internal Field(string name, IReadContract contract, bool isRequired)
            {
                Name = name;
                Contract = contract;
                IsRequired = isRequired;
            }
        }

        /// <summary>
        /// The fields that comprise this complex type.
        /// </summary>
        public IReadOnlyList<Field> Fields { get; }

        internal ComplexReadContract(Type type, ContractCollection contractCollection)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            if (constructors.Length != 1)
                throw new ArgumentException($"Type {type} must have one constructor, not {constructors.Length}.", nameof(type));
            var parameters = constructors[0].GetParameters();
            if (parameters.Length == 0)
                throw new ArgumentException($"Constructor for type {type} must have at least one argument.", nameof(type));
            Fields = parameters
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .Select(p => new Field(
                    p.Name,
                    p.ParameterType == type
                        ? this
                        : contractCollection.GetOrAddReadContract(p.ParameterType),
                    isRequired: !p.HasDefaultValue))
                .ToList();
        }

        private ComplexReadContract(IReadOnlyList<Field> fields) => Fields = fields;

        internal ComplexReadContract(XElement element, Func<string, IReadContract> resolveContract, ICollection<Action> bindActions)
        {
            var fields = new List<Field>();

            bindActions.Add(() =>
            {
                foreach (var field in element.Elements(nameof(Field)))
                {
                    var name = field.Attribute(nameof(Field.Name))?.Value;
                    var contract = field.Attribute(nameof(Field.Contract))?.Value;
                    var isRequiredStr = field.Attribute(nameof(Field.IsRequired))?.Value;

                    if (string.IsNullOrWhiteSpace(name))
                        throw new ContractParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Field.Name)}\" attribute.");
                    if (string.IsNullOrWhiteSpace(contract))
                        throw new ContractParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Field.Contract)}\" attribute.");
                    if (!bool.TryParse(isRequiredStr, out bool isRequired))
                        throw new ContractParseException($"\"{element.Name}\" element must have a boolean \"{nameof(Field.IsRequired)}\" attribute.");

                    fields.Add(new Field(name, resolveContract(contract), isRequired));
                }
            });

            Fields = fields;
        }

        public bool CanReadFrom(IWriteContract writeContract, bool strict)
        {
            var isEmpty = writeContract is EmptyContract;
            if (strict && isEmpty)
                return false;
            var ws = writeContract as ComplexWriteContract;
            if (ws == null && !isEmpty)
                return false;
            var readFields = Fields;
            var writeFields = isEmpty ? EmptyArray<ComplexWriteContract.Field>.Instance : ws.Fields;

            var ir = 0;
            var iw = 0;

            while (ir < readFields.Count)
            {
                var rf = readFields[ir];

                // skip non-required read fields at the end of the message
                if (iw == writeFields.Count)
                {
                    if (rf.IsRequired)
                        return false;

                    ir++;
                    continue;
                }

                var wf = writeFields[iw];

                var cmp = StringComparer.OrdinalIgnoreCase.Compare(rf.Name, wf.Name);

                if (cmp == 0)
                {
                    // match

                    // prevent single-level recursive type causing stack overflow
                    if (!Equals(rf.Contract, this) && !Equals(wf.Contract, writeContract))
                    {
                        if (!rf.Contract.CanReadFrom(wf.Contract, strict))
                            return false;
                    }

                    // step both forwards
                    ir++;
                    iw++;
                }
                else if (cmp > 0)
                {
                    // write field comes before read field -- write type contains an extra field
                    if (strict)
                        return false;
                    // skip the write field only
                    iw++;
                }
                else
                {
                    // write field missing
                    if (rf.IsRequired)
                        return false;
                    ir++;
                }
            }

            if (iw != writeFields.Count && strict)
                return false;

            return true;
        }

        public override bool Equals(Contract other)
        {
            return other is ComplexReadContract crc &&
                   crc.Fields.SequenceEqual(Fields,
                       (a, b) => a.Name == b.Name && a.IsRequired == b.IsRequired && a.Contract.Equals(b.Contract));
        }

        protected override int ComputeHashCode()
        {
            unchecked
            {
                var hash = 0;
                foreach (var field in Fields)
                {
                    hash <<= 5;
                    hash ^= field.Name.GetHashCode();
                    hash <<= 3;
                    hash ^= field.Contract.GetHashCode();
                    hash <<= 1;
                    hash |= field.IsRequired.GetHashCode();
                }

                return hash;
            }
        }

        internal override IEnumerable<Contract> Children => Fields.Select(f => f.Contract).Cast<Contract>();

        internal override XElement ToXml()
        {
            if (Id == null)
                throw new InvalidOperationException("\"Id\" property cannot be null.");
            return new XElement("ComplexRead",
                new XAttribute("Id", Id),
                Fields.Select(f => new XElement(nameof(Field),
                    new XAttribute(nameof(Field.Name), f.Name),
                    new XAttribute(nameof(Field.Contract), f.Contract.ToReferenceString()),
                    new XAttribute(nameof(Field.IsRequired), f.IsRequired))));
        }

        public IReadContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new ComplexReadContract(Fields.Select(f => new Field(f.Name, f.Contract.CopyTo(collection), f.IsRequired)).ToList()));
        }
    }
}
