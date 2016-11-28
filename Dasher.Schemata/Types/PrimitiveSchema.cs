using System;
using System.Collections.Generic;
using System.Linq;
using Dasher.Schemata.Utils;

namespace Dasher.Schemata.Types
{
    internal sealed class PrimitiveSchema : ByValueSchema, IWriteSchema, IReadSchema
    {
        private static readonly Dictionary<Type, string> _nameByType;
        private static readonly Dictionary<string, Type> _typeByName;

        static PrimitiveSchema()
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
                {typeof(string), "String"},
                {typeof(byte[]), "ByteArray"},
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

        public PrimitiveSchema(Type type)
        {
            string name;
            if (!_nameByType.TryGetValue(type, out name))
                throw new ArgumentException($"Type {type} is not a supported primitive.", nameof(type));
            TypeName = name;
        }

        public PrimitiveSchema(string typeName)
        {
            if (!_typeByName.ContainsKey(typeName))
                throw new SchemaParseException($"Invalid primitive schema name \"{typeName}\".");
            TypeName = typeName;
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool strict) => Equals(writeSchema);

        public override bool Equals(Schema other)
        {
            var schema = other as PrimitiveSchema;
            return schema != null && schema.TypeName == TypeName;
        }

        protected override int ComputeHashCode() => TypeName.GetHashCode();

        internal override IEnumerable<Schema> Children => EmptyArray<Schema>.Instance;

        internal override string MarkupValue => TypeName;

        IWriteSchema IWriteSchema.CopyTo(SchemaCollection collection) => collection.Intern(this);
        IReadSchema IReadSchema.CopyTo(SchemaCollection collection) => collection.Intern(this);
    }
}