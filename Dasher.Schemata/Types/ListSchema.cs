using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dasher.Schemata.Utils;

namespace Dasher.Schemata.Types
{
    internal sealed class ListWriteSchema : ByValueSchema, IWriteSchema
    {
        public static bool CanProcess(Type type) => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>);

        public IWriteSchema ItemSchema { get; }

        public ListWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyList<int>)}<>.", nameof(type));
            ItemSchema = schemaCollection.GetOrAddWriteSchema(type.GetGenericArguments().Single());
        }

        public ListWriteSchema(IWriteSchema itemSchema)
        {
            ItemSchema = itemSchema;
        }

        public override bool Equals(Schema other)
        {
            var o = other as ListWriteSchema;
            return o != null && ((Schema)o.ItemSchema).Equals((Schema)ItemSchema);
        }

        protected override int ComputeHashCode() => unchecked((int)0xA4A76926 ^ ItemSchema.GetHashCode());

        internal override IEnumerable<Schema> Children => new[] { (Schema)ItemSchema };

        internal override string MarkupValue => $"{{list {ItemSchema.ToReferenceString()}}}";
    }

    internal sealed class ListReadSchema : ByValueSchema, IReadSchema
    {
        public static bool CanProcess(Type type) => ListWriteSchema.CanProcess(type);

        private IReadSchema ItemSchema { get; }

        public ListReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyList<int>)}<>.", nameof(type));
            ItemSchema = schemaCollection.GetOrAddReadSchema(type.GetGenericArguments().Single());
        }

        public ListReadSchema(IReadSchema itemSchema)
        {
            ItemSchema = itemSchema;
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool strict)
        {
            var ws = writeSchema as ListWriteSchema;
            return ws != null && ItemSchema.CanReadFrom(ws.ItemSchema, strict);
        }

        public override bool Equals(Schema other) => (other as ListReadSchema)?.ItemSchema.Equals(ItemSchema) ?? false;

        protected override int ComputeHashCode() => unchecked((int)0x9ABCF854 ^ ItemSchema.GetHashCode());

        internal override IEnumerable<Schema> Children => new[] { (Schema)ItemSchema };

        internal override string MarkupValue => $"{{list {ItemSchema.ToReferenceString()}}}";
    }
}