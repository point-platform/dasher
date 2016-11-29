using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Dasher.Schemata.Utils;

namespace Dasher.Schemata.Types
{
    internal sealed class EnumSchema : ByRefSchema, IWriteSchema, IReadSchema
    {
        public static bool CanProcess(Type type) => type.GetTypeInfo().IsEnum;

        private HashSet<string> MemberNames { get; }

        public EnumSchema(Type type)
        {
            if (!CanProcess(type))
                throw new ArgumentException("Must be an enum.", nameof(type));
            MemberNames = new HashSet<string>(Enum.GetNames(type), StringComparer.OrdinalIgnoreCase);
        }

        public EnumSchema(XContainer element)
        {
            MemberNames = new HashSet<string>(element.Elements().Select(e => e.Name.LocalName));
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool strict)
        {
            var that = writeSchema as EnumSchema;
            if (that == null)
                return false;
            return strict
                ? MemberNames.SetEquals(that.MemberNames)
                : MemberNames.IsSupersetOf(that.MemberNames);
        }

        internal override IEnumerable<Schema> Children => EmptyArray<Schema>.Instance;

        public override bool Equals(Schema other)
        {
            var e = other as EnumSchema;
            return e != null && MemberNames.SetEquals(e.MemberNames);
        }

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
                MemberNames.Select(m => new XElement(m)));
        }

        IReadSchema IReadSchema.CopyTo(SchemaCollection collection) => collection.Intern(this);
        IWriteSchema IWriteSchema.CopyTo(SchemaCollection collection) => collection.Intern(this);
    }
}