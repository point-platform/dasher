using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dasher.Schemata.Utils;
using Dasher.TypeProviders;

namespace Dasher.Schemata.Types
{
    internal sealed class UnionWriteSchema : ByRefSchema, IWriteSchema
    {
        public static bool CanProcess(Type type) => Union.IsUnionType(type);

        public struct Member
        {
            public string Id { get; }
            public IWriteSchema Schema { get; }

            public Member(string id, IWriteSchema schema)
            {
                Id = id;
                Schema = schema;
            }
        }

        public IReadOnlyList<Member> Members { get; }

        public UnionWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be a union.", nameof(type));
            Members = Union.GetTypes(type)
                .Select(t => new Member(UnionEncoding.GetTypeName(t), schemaCollection.GetOrAddWriteSchema(t)))
                .OrderBy(m => m.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private UnionWriteSchema(IReadOnlyList<Member> members)
        {
            Members = members;
        }

        public UnionWriteSchema(XElement element, Func<string, IWriteSchema> resolveSchema, ICollection<Action> bindActions)
        {
            var members = new List<Member>();

            bindActions.Add(() =>
            {
                foreach (var field in element.Elements(nameof(Member)))
                {
                    var id = field.Attribute(nameof(Member.Id))?.Value;
                    var schema = field.Attribute(nameof(Member.Schema))?.Value;

                    if (string.IsNullOrWhiteSpace(id))
                        throw new SchemaParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Member.Id)}\" attribute.");
                    if (string.IsNullOrWhiteSpace(schema))
                        throw new SchemaParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Member.Schema)}\" attribute.");

                    members.Add(new Member(id, resolveSchema(schema)));
                }
            });

            Members = members;
        }

        public override bool Equals(Schema other)
        {
            var o = other as UnionWriteSchema;
            return o != null && o.Members.SequenceEqual(Members, (a, b) => a.Id == b.Id && a.Schema.Equals(b.Schema));
        }

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
                    hash ^= member.Schema.GetHashCode();
                }
                return hash;
            }
        }

        internal override IEnumerable<Schema> Children => Members.Select(m => m.Schema).Cast<Schema>();

        internal override XElement ToXml()
        {
            if (Id == null)
                throw new InvalidOperationException("\"Id\" property cannot be null.");
            return new XElement("UnionWrite",
                new XAttribute("Id", Id),
                Members.Select(m => new XElement("Member",
                    new XAttribute("Id", m.Id),
                    new XAttribute("Schema", m.Schema.ToReferenceString()))));
        }

        public IWriteSchema CopyTo(SchemaCollection collection)
        {
            return collection.GetOrCreate(this, () => new UnionWriteSchema(Members.Select(m => new Member(m.Id, m.Schema.CopyTo(collection))).ToList()));
        }
    }

    internal sealed class UnionReadSchema : ByRefSchema, IReadSchema
    {
        public static bool CanProcess(Type type) => UnionWriteSchema.CanProcess(type);

        private struct Member
        {
            public string Id { get; }
            public IReadSchema Schema { get; }

            public Member(string id, IReadSchema schema)
            {
                Id = id;
                Schema = schema;
            }
        }

        private IReadOnlyList<Member> Members { get; }

        public UnionReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be a union.", nameof(type));
            Members = Union.GetTypes(type)
                .Select(t => new Member(UnionEncoding.GetTypeName(t), schemaCollection.GetOrAddReadSchema(t)))
                .OrderBy(m => m.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private UnionReadSchema(IReadOnlyList<Member> members)
        {
            Members = members;
        }

        public UnionReadSchema(XElement element, Func<string, IReadSchema> resolveSchema, ICollection<Action> bindActions)
        {
            var members = new List<Member>();

            bindActions.Add(() =>
            {
                foreach (var field in element.Elements(nameof(Member)))
                {
                    var id = field.Attribute(nameof(Member.Id))?.Value;
                    var schema = field.Attribute(nameof(Member.Schema))?.Value;

                    if (string.IsNullOrWhiteSpace(id))
                        throw new SchemaParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Member.Id)}\" attribute.");
                    if (string.IsNullOrWhiteSpace(schema))
                        throw new SchemaParseException($"\"{element.Name}\" element must have a non-empty \"{nameof(Member.Schema)}\" attribute.");

                    members.Add(new Member(id, resolveSchema(schema)));
                }
            });

            Members = members;
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool strict)
        {
            // TODO write EmptySchema test for this case
            if (writeSchema is EmptySchema)
                return true;

            var ws = writeSchema as UnionWriteSchema;

            if (ws == null)
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
                    if (!rm.Schema.CanReadFrom(wm.Schema, strict))
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

        public override bool Equals(Schema other)
        {
            var o = other as UnionReadSchema;
            return o != null && o.Members.SequenceEqual(Members, (a, b) => a.Id == b.Id && a.Schema.Equals(b.Schema));
        }

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
                    hash ^= member.Schema.GetHashCode();
                }
                return hash;
            }
        }

        internal override IEnumerable<Schema> Children => Members.Select(m => m.Schema).Cast<Schema>();

        internal override XElement ToXml()
        {
            if (Id == null)
                throw new InvalidOperationException("\"Id\" property cannot be null.");
            return new XElement("UnionRead",
                new XAttribute("Id", Id),
                Members.Select(m => new XElement("Member",
                    new XAttribute("Id", m.Id),
                    new XAttribute("Schema", m.Schema.ToReferenceString()))));
        }

        public IReadSchema CopyTo(SchemaCollection collection)
        {
            return collection.GetOrCreate(this, () => new UnionReadSchema(Members.Select(m => new Member(m.Id, m.Schema.CopyTo(collection))).ToList()));
        }
    }
}