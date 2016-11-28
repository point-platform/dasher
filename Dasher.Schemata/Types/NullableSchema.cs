using System;
using System.Collections.Generic;
using Dasher.Schemata.Utils;

namespace Dasher.Schemata.Types
{
    internal sealed class NullableWriteSchema : ByValueSchema, IWriteSchema
    {
        public static bool CanProcess(Type type) => Nullable.GetUnderlyingType(type) != null;

        public IWriteSchema Inner { get; }

        public NullableWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be nullable.", nameof(type));
            Inner = schemaCollection.GetOrAddWriteSchema(Nullable.GetUnderlyingType(type));
        }

        public NullableWriteSchema(IWriteSchema inner)
        {
            Inner = inner;
        }

        public override bool Equals(Schema other)
        {
            var o = other as NullableWriteSchema;
            return o != null && ((Schema)o.Inner).Equals((Schema)Inner);
        }

        protected override int ComputeHashCode() => unchecked(0x3731AFBB ^ Inner.GetHashCode());

        internal override IEnumerable<Schema> Children => new[] { (Schema)Inner };

        internal override string MarkupValue => $"{{nullable {Inner.ToReferenceString()}}}";

        public IWriteSchema CopyTo(SchemaCollection collection)
        {
            return collection.GetOrCreate(this, () => new NullableWriteSchema(Inner.CopyTo(collection)));
        }
    }

    internal sealed class NullableReadSchema : ByValueSchema, IReadSchema
    {
        public static bool CanProcess(Type type) => NullableWriteSchema.CanProcess(type);

        private IReadSchema Inner { get; }

        public NullableReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be nullable.", nameof(type));
            Inner = schemaCollection.GetOrAddReadSchema(Nullable.GetUnderlyingType(type));
        }

        public NullableReadSchema(IReadSchema inner)
        {
            Inner = inner;
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool strict)
        {
            var ws = writeSchema as NullableWriteSchema;

            if (ws != null)
                return Inner.CanReadFrom(ws.Inner, strict);

            if (strict)
                return false;

            return Inner.CanReadFrom(writeSchema, strict);
        }

        public override bool Equals(Schema other)
        {
            var o = other as NullableReadSchema;
            return o != null && ((Schema)o.Inner).Equals((Schema)Inner);
        }

        protected override int ComputeHashCode() => unchecked(0x563D4345 ^ Inner.GetHashCode());

        internal override IEnumerable<Schema> Children => new[] { (Schema)Inner };

        internal override string MarkupValue => $"{{nullable {Inner.ToReferenceString()}}}";

        public IReadSchema CopyTo(SchemaCollection collection)
        {
            return collection.GetOrCreate(this, () => new NullableReadSchema(Inner.CopyTo(collection)));
        }
    }
}