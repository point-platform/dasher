using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Dasher.Schemata.Utils;

namespace Dasher.Schemata.Types
{
    internal sealed class ComplexWriteSchema : ByRefSchema, IWriteSchema
    {
        public struct Field
        {
            public string Name { get; }
            public IWriteSchema Schema { get; }

            public Field(string name, IWriteSchema schema)
            {
                Name = name;
                Schema = schema;
            }
        }

        public IReadOnlyList<Field> Fields { get; }

        public ComplexWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase);
            if (!properties.Any())
                throw new ArgumentException($"Type {type} must have at least one public instance property.", nameof(type));
            Fields = properties.Select(p => new Field(p.Name, schemaCollection.GetOrAddWriteSchema(p.PropertyType))).ToArray();
        }

        private ComplexWriteSchema(IReadOnlyList<Field> fields)
        {
            Fields = fields;
        }

        public ComplexWriteSchema(XElement element, Func<string, IWriteSchema> resolveSchema, ICollection<Action> bindActions)
        {
            var fields = new List<Field>();

            bindActions.Add(() =>
            {
                foreach (var field in element.Elements(nameof(Field)))
                {
                    var name = field.Attribute(nameof(Field.Name))?.Value;
                    var schema = field.Attribute(nameof(Field.Schema))?.Value;

                    if (string.IsNullOrWhiteSpace(name))
                        throw new SchemaParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Field.Name)}\" attribute.");
                    if (string.IsNullOrWhiteSpace(schema))
                        throw new SchemaParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Field.Schema)}\" attribute.");

                    fields.Add(new Field(name, resolveSchema(schema)));
                }
            });

            Fields = fields;
        }

        public override bool Equals(Schema other)
        {
            return (other as ComplexWriteSchema)?.Fields.SequenceEqual(Fields,
                       (a, b) => a.Name == b.Name && a.Schema.Equals(b.Schema))
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
                    hash ^= field.Schema.GetHashCode();
                }
                return hash;
            }
        }

        internal override IEnumerable<Schema> Children => Fields.Select(f => f.Schema).Cast<Schema>();

        internal override XElement ToXml()
        {
            if (Id == null)
                throw new InvalidOperationException("\"Id\" property cannot be null.");
            return new XElement("ComplexWrite",
                new XAttribute("Id", Id),
                Fields.Select(f => new XElement("Field",
                    new XAttribute("Name", f.Name),
                    new XAttribute("Schema", f.Schema.ToReferenceString()))));
        }

        public IWriteSchema CopyTo(SchemaCollection collection)
        {
            return collection.GetOrCreate(this, () => new ComplexWriteSchema(Fields.Select(f => new Field(f.Name, f.Schema.CopyTo(collection))).ToList()));
        }
    }

    internal sealed class ComplexReadSchema : ByRefSchema, IReadSchema
    {
        private struct Field
        {
            public string Name { get; }
            public IReadSchema Schema { get; }
            public bool IsRequired { get; }

            public Field(string name, IReadSchema schema, bool isRequired)
            {
                Name = name;
                Schema = schema;
                IsRequired = isRequired;
            }
        }

        private IReadOnlyList<Field> Fields { get; }

        public ComplexReadSchema(Type type, SchemaCollection schemaCollection)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            if (constructors.Length != 1)
                throw new ArgumentException($"Type {type} have a single constructor.", nameof(type));
            var parameters = constructors[0].GetParameters();
            if (parameters.Length == 0)
                throw new ArgumentException($"Constructor for type {type} must have at least one argument.", nameof(type));
            Fields = parameters
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .Select(p => new Field(p.Name, schemaCollection.GetOrAddReadSchema(p.ParameterType), isRequired: !p.HasDefaultValue))
                .ToList();
        }

        private ComplexReadSchema(IReadOnlyList<Field> fields)
        {
            Fields = fields;
        }

        public ComplexReadSchema(XElement element, Func<string, IReadSchema> resolveSchema, ICollection<Action> bindActions)
        {
            var fields = new List<Field>();

            bindActions.Add(() =>
            {
                foreach (var field in element.Elements(nameof(Field)))
                {
                    var name = field.Attribute(nameof(Field.Name))?.Value;
                    var schema = field.Attribute(nameof(Field.Schema))?.Value;
                    var isRequiredStr = field.Attribute(nameof(Field.IsRequired))?.Value;

                    if (string.IsNullOrWhiteSpace(name))
                        throw new SchemaParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Field.Name)}\" attribute.");
                    if (string.IsNullOrWhiteSpace(schema))
                        throw new SchemaParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Field.Schema)}\" attribute.");
                    bool isRequired;
                    if (!bool.TryParse(isRequiredStr, out isRequired))
                        throw new SchemaParseException($"\"{element.Name}\" element must have a boolean \"{nameof(Field.IsRequired)}\" attribute.");

                    fields.Add(new Field(name, resolveSchema(schema), isRequired));
                }
            });

            Fields = fields;
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool strict)
        {
            // TODO write EmptySchema test for this case and several others... (eg. tuple, union, ...)
            if (writeSchema is EmptySchema)
                return true;

            var ws = writeSchema as ComplexWriteSchema;
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
                    if (!rf.Schema.CanReadFrom(wf.Schema, strict))
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

        public override bool Equals(Schema other)
        {
            return (other as ComplexReadSchema)?.Fields.SequenceEqual(Fields,
                       (a, b) => a.Name == b.Name && a.IsRequired == b.IsRequired && a.Schema.Equals(b.Schema))
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
                    hash ^= field.Schema.GetHashCode();
                    hash <<= 1;
                    hash |= field.IsRequired.GetHashCode();
                }
                return hash;
            }
        }

        internal override IEnumerable<Schema> Children => Fields.Select(f => f.Schema).Cast<Schema>();

        internal override XElement ToXml()
        {
            if (Id == null)
                throw new InvalidOperationException("\"Id\" property cannot be null.");
            return new XElement("ComplexRead",
                new XAttribute("Id", Id),
                Fields.Select(f => new XElement(nameof(Field),
                    new XAttribute(nameof(Field.Name), f.Name),
                    new XAttribute(nameof(Field.Schema), f.Schema.ToReferenceString()),
                    new XAttribute(nameof(Field.IsRequired), f.IsRequired))));
        }

        public IReadSchema CopyTo(SchemaCollection collection)
        {
            return collection.GetOrCreate(this, () => new ComplexReadSchema(Fields.Select(f => new Field(f.Name, f.Schema.CopyTo(collection), f.IsRequired)).ToList()));
        }
    }
}