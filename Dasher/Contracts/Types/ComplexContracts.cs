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
    internal sealed class ComplexWriteContract : ByRefContract, IWriteContract
    {
        public struct Field
        {
            public string Name { get; }
            public IWriteContract Contract { get; }

            public Field(string name, IWriteContract contract)
            {
                Name = name;
                Contract = contract;
            }
        }

        public IReadOnlyList<Field> Fields { get; }

        public ComplexWriteContract(Type type, ContractCollection contractCollection)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase);
            Fields = properties.Select(p => new Field(p.Name, contractCollection.GetOrAddWriteContract(p.PropertyType))).ToArray();
            if (!Fields.Any())
                throw new ArgumentException($"Type {type} must have at least one public instance property.", nameof(type));
        }

        private ComplexWriteContract(IReadOnlyList<Field> fields)
        {
            Fields = fields;
        }

        public ComplexWriteContract(XElement element, Func<string, IWriteContract> resolveContract, ICollection<Action> bindActions)
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

        public override bool Equals(Contract other)
        {
            return (other as ComplexWriteContract)?.Fields.SequenceEqual(Fields,
                       (a, b) => a.Name == b.Name && a.Contract.Equals(b.Contract))
                   ?? false;
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

        public IWriteContract CopyTo(ContractCollection collection)
        {
            return collection.GetOrCreate(this, () => new ComplexWriteContract(Fields.Select(f => new Field(f.Name, f.Contract.CopyTo(collection))).ToList()));
        }
    }

    internal sealed class ComplexReadContract : ByRefContract, IReadContract
    {
        private struct Field
        {
            public string Name { get; }
            public IReadContract Contract { get; }
            public bool IsRequired { get; }

            public Field(string name, IReadContract contract, bool isRequired)
            {
                Name = name;
                Contract = contract;
                IsRequired = isRequired;
            }
        }

        private IReadOnlyList<Field> Fields { get; }

        public ComplexReadContract(Type type, ContractCollection contractCollection)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            if (constructors.Length != 1)
                throw new ArgumentException($"Type {type} have a single constructor.", nameof(type));
            var parameters = constructors[0].GetParameters();
            if (parameters.Length == 0)
                throw new ArgumentException($"Constructor for type {type} must have at least one argument.", nameof(type));
            Fields = parameters
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .Select(p => new Field(p.Name, contractCollection.GetOrAddReadContract(p.ParameterType), isRequired: !p.HasDefaultValue))
                .ToList();
        }

        private ComplexReadContract(IReadOnlyList<Field> fields)
        {
            Fields = fields;
        }

        public ComplexReadContract(XElement element, Func<string, IReadContract> resolveContract, ICollection<Action> bindActions)
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
                    bool isRequired;
                    if (!bool.TryParse(isRequiredStr, out isRequired))
                        throw new ContractParseException($"\"{element.Name}\" element must have a boolean \"{nameof(Field.IsRequired)}\" attribute.");

                    fields.Add(new Field(name, resolveContract(contract), isRequired));
                }
            });

            Fields = fields;
        }

        public bool CanReadFrom(IWriteContract writeContract, bool strict)
        {
            // TODO write EmptyContract test for this case and several others... (eg. tuple, union, ...)
            if (writeContract is EmptyContract)
                return true;

            var ws = writeContract as ComplexWriteContract;
            if (ws == null)
                return false;
            var readFields = Fields;
            var writeFields = ws.Fields;

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
                    if (!rf.Contract.CanReadFrom(wf.Contract, strict))
                        return false;

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
            return (other as ComplexReadContract)?.Fields.SequenceEqual(Fields,
                       (a, b) => a.Name == b.Name && a.IsRequired == b.IsRequired && a.Contract.Equals(b.Contract))
                   ?? false;
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