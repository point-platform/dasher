using System;
using System.Collections.Generic;
using System.Reflection;
using Dasher.Schemata.Utils;

namespace Dasher.Schemata.Types
{
    internal sealed class DictionaryWriteSchema : ByValueSchema, IWriteSchema
    {
        public static bool CanProcess(Type type) => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>);

        public IWriteSchema KeySchema { get; }
        public IWriteSchema ValueSchema { get; }

        public DictionaryWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyDictionary<int, int>)}<>.", nameof(type));
            KeySchema = schemaCollection.GetOrAddWriteSchema(type.GetGenericArguments()[0]);
            ValueSchema = schemaCollection.GetOrAddWriteSchema(type.GetGenericArguments()[1]);
        }

        public DictionaryWriteSchema(IWriteSchema keySchema, IWriteSchema valueSchema)
        {
            KeySchema = keySchema;
            ValueSchema = valueSchema;
        }

        public override bool Equals(Schema other)
        {
            var o = other as DictionaryWriteSchema;
            return o != null && o.KeySchema.Equals(KeySchema) && o.ValueSchema.Equals(ValueSchema);
        }

        protected override int ComputeHashCode()
        {
            unchecked
            {
                var hash = KeySchema.GetHashCode();
                hash <<= 5;
                hash ^= ValueSchema.GetHashCode();
                return hash;
            }
        }

        internal override IEnumerable<Schema> Children => new[] { (Schema)KeySchema, (Schema)ValueSchema };

        internal override string MarkupValue => $"{{dictionary {KeySchema.ToReferenceString()} {ValueSchema.ToReferenceString()}}}";
    }

    internal sealed class DictionaryReadSchema : ByValueSchema, IReadSchema
    {
        public static bool CanProcess(Type type) => DictionaryWriteSchema.CanProcess(type);

        private IReadSchema KeySchema { get; }
        private IReadSchema ValueSchema { get; }

        public DictionaryReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyDictionary<int, int>)}<>.", nameof(type));
            KeySchema = schemaCollection.GetOrAddReadSchema(type.GetGenericArguments()[0]);
            ValueSchema = schemaCollection.GetOrAddReadSchema(type.GetGenericArguments()[1]);
        }

        public DictionaryReadSchema(IReadSchema keySchema, IReadSchema valueSchema)
        {
            KeySchema = keySchema;
            ValueSchema = valueSchema;
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool strict)
        {
            var ws = writeSchema as DictionaryWriteSchema;
            if (ws == null)
                return false;
            return KeySchema.CanReadFrom(ws.KeySchema, strict) &&
                   ValueSchema.CanReadFrom(ws.ValueSchema, strict);
        }

        public override bool Equals(Schema other)
        {
            var o = other as DictionaryReadSchema;
            return o != null && o.KeySchema.Equals(KeySchema) && o.ValueSchema.Equals(ValueSchema);
        }

        protected override int ComputeHashCode()
        {
            unchecked
            {
                var hash = KeySchema.GetHashCode();
                hash <<= 5;
                hash ^= ValueSchema.GetHashCode();
                return hash;
            }
        }

        internal override IEnumerable<Schema> Children => new[] { (Schema)KeySchema, (Schema)ValueSchema };

        internal override string MarkupValue => $"{{dictionary {KeySchema.ToReferenceString()} {ValueSchema.ToReferenceString()}}}";
    }
}