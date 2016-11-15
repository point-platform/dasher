using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dasher.Schemata.Utils;

namespace Dasher.Schemata.Types
{
    internal sealed class TupleReadSchema : ByValueSchema, IReadSchema
    {
        public static bool CanProcess(Type type) => TupleWriteSchema.CanProcess(type);

        private IReadOnlyList<IReadSchema> Items { get; }

        public TupleReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!TupleWriteSchema.CanProcess(type))
                throw new ArgumentException($"Type {type} is not a supported tuple type.", nameof(type));

            Items = type.GetGenericArguments().Select(schemaCollection.GetOrAddReadSchema).ToList();
        }

        public TupleReadSchema(IReadOnlyList<IReadSchema> items)
        {
            Items = items;
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool strict)
        {
            var that = writeSchema as TupleWriteSchema;

            return that?.Items.Count == Items.Count
                   && !Items.Where((rs, i) => !rs.CanReadFrom(that.Items[i], strict)).Any();
        }

        public override bool Equals(Schema other) => (other as TupleReadSchema)?.Items.SequenceEqual(Items) ?? false;

        protected override int ComputeHashCode()
        {
            unchecked
            {
                var hash = 0;
                foreach (var item in Items)
                {
                    hash <<= 5;
                    hash ^= item.GetHashCode();
                }
                return hash;
            }
        }

        internal override IEnumerable<Schema> Children => Items.Cast<Schema>();

        internal override string MarkupValue
        {
            get { return $"{{tuple {string.Join(" ", Items.Select(i => i.ToReferenceString()))}}}"; }
        }
    }

    internal sealed class TupleWriteSchema : ByValueSchema, IWriteSchema
    {
        public static bool CanProcess(Type type)
        {
            if (!type.GetTypeInfo().IsGenericType)
                return false;
            if (!type.IsConstructedGenericType)
                return false;

            var genType = type.GetGenericTypeDefinition();

            return genType == typeof(Tuple<>) ||
                   genType == typeof(Tuple<,>) ||
                   genType == typeof(Tuple<,,>) ||
                   genType == typeof(Tuple<,,,>) ||
                   genType == typeof(Tuple<,,,,>) ||
                   genType == typeof(Tuple<,,,,,>) ||
                   genType == typeof(Tuple<,,,,,,>) ||
                   genType == typeof(Tuple<,,,,,,,>) ||
                   genType == typeof(Tuple<,,,,,,,>);
        }

        public IReadOnlyList<IWriteSchema> Items { get; }

        public TupleWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} is not a supported tuple type.", nameof(type));

            Items = type.GetGenericArguments().Select(schemaCollection.GetOrAddWriteSchema).ToList();
        }

        public TupleWriteSchema(IReadOnlyList<IWriteSchema> items)
        {
            Items = items;
        }

        public override bool Equals(Schema other) => (other as TupleWriteSchema)?.Items.SequenceEqual(Items) ?? false;

        protected override int ComputeHashCode()
        {
            unchecked
            {
                var hash = 0;
                foreach (var item in Items)
                {
                    hash <<= 5;
                    hash ^= item.GetHashCode();
                }
                return hash;
            }
        }

        internal override IEnumerable<Schema> Children => Items.Cast<Schema>();

        internal override string MarkupValue
        {
            get { return $"{{tuple {string.Join(" ", Items.Select(i => i.ToReferenceString()))}}}"; }
        }
    }
}