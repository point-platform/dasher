using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Dasher;
using Dasher.TypeProviders;
using JetBrains.Annotations;

namespace SchemaComparisons
{
    // TODO list and dictionary
    // TODO support recursive types

    public sealed class SchemaCollection
    {
        public IWriteSchema GetWriteSchema(Type type)
        {
            if (type == typeof(EmptyMessage))
                return new EmptySchema();
            if (PrimitiveSchema.CanProcess(type))
                return new PrimitiveSchema(type);
            if (EnumSchema.CanProcess(type))
                return new EnumSchema(type);

            if (TupleWriteSchema.CanProcess(type))
                return new TupleWriteSchema(type, this);
            if (NullableWriteSchema.CanProcess(type))
                return new NullableWriteSchema(type, this);
            if (ListWriteSchema.CanProcess(type))
                return new ListWriteSchema(type, this);
            if (DictionaryWriteSchema.CanProcess(type))
                return new DictionaryWriteSchema(type, this);
            if (UnionWriteSchema.CanProcess(type))
                return new UnionWriteSchema(type, this);

            return new ComplexWriteSchema(type, this);
        }

        public IReadSchema GetReadSchema(Type type)
        {
            if (type == typeof(EmptyMessage))
                return new EmptySchema();
            if (PrimitiveSchema.CanProcess(type))
                return new PrimitiveSchema(type);
            if (EnumSchema.CanProcess(type))
                return new EnumSchema(type);

            if (TupleReadSchema.CanProcess(type))
                return new TupleReadSchema(type, this);
            if (NullableReadSchema.CanProcess(type))
                return new NullableReadSchema(type, this);
            if (ListReadSchema.CanProcess(type))
                return new ListReadSchema(type, this);
            if (DictionaryReadSchema.CanProcess(type))
                return new DictionaryReadSchema(type, this);
            if (UnionReadSchema.CanProcess(type))
                return new UnionReadSchema(type, this);

            return new ComplexReadSchema(type, this);
        }
    }

    [SuppressMessage("ReSharper", "ConvertToStaticClass")]
    public sealed class EmptyMessage
    {
        private EmptyMessage() { }
    }

    public interface IWriteSchema { }

    public interface IReadSchema
    {
        bool CanReadFrom([NotNull] IWriteSchema writeSchema, bool allowWideningConversion);
    }

    internal sealed class EnumSchema : IWriteSchema, IReadSchema
    {
        public static bool CanProcess(Type type) => type.IsEnum;

        private HashSet<string> MemberNames { get; }

        public EnumSchema(Type type)
        {
            if (!CanProcess(type))
                throw new ArgumentException("Must be an enum.", nameof(type));
            MemberNames = new HashSet<string>(Enum.GetNames(type), StringComparer.OrdinalIgnoreCase);
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool allowWideningConversion)
        {
            var that = writeSchema as EnumSchema;
            if (that == null)
                return false;
            return allowWideningConversion
                ? MemberNames.IsSupersetOf(that.MemberNames)
                : MemberNames.SetEquals(that.MemberNames);
        }
    }

    internal sealed class EmptySchema : IWriteSchema, IReadSchema
    {
        public bool CanReadFrom(IWriteSchema writeSchema, bool allowWideningConversion)
        {
            return writeSchema is EmptySchema || allowWideningConversion;
        }
    }

    internal sealed class PrimitiveSchema : IWriteSchema, IReadSchema
    {
        private static readonly Dictionary<Type, string> NameByType = new Dictionary<Type, string>
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

        public static bool CanProcess(Type type) => NameByType.ContainsKey(type);

        private string TypeName { get; }

        public PrimitiveSchema(Type type)
        {
            string name;
            if (!NameByType.TryGetValue(type, out name))
                throw new ArgumentException($"Type {type} is not a supported primitive.", nameof(type));
            TypeName = name;
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool allowWideningConversion)
        {
            var ws = writeSchema as PrimitiveSchema;
            return ws != null && ws.TypeName == TypeName;
        }
    }

    #region Tuple

    internal sealed class TupleWriteSchema : IWriteSchema
    {
        public static bool CanProcess(Type type)
        {
            if (!type.IsGenericType)
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

            Items = type.GetGenericArguments().Select(schemaCollection.GetWriteSchema).ToList();
        }
    }

    internal sealed class TupleReadSchema : IReadSchema
    {
        public static bool CanProcess(Type type) => TupleWriteSchema.CanProcess(type);

        private IReadOnlyList<IReadSchema> Items { get; }

        public TupleReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!TupleWriteSchema.CanProcess(type))
                throw new ArgumentException($"Type {type} is not a supported tuple type.", nameof(type));

            Items = type.GetGenericArguments().Select(schemaCollection.GetReadSchema).ToList();
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool allowWideningConversion)
        {
            var that = writeSchema as TupleWriteSchema;

            return that?.Items.Count == Items.Count
                   && !Items.Where((rs, i) => !rs.CanReadFrom(that.Items[i], allowWideningConversion)).Any();
        }
    }

    #endregion

    #region Complex

    internal sealed class ComplexWriteSchema : IWriteSchema
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
            Fields = properties.Select(p => new Field(p.Name, schemaCollection.GetWriteSchema(p.PropertyType))).ToArray();
        }
    }

    internal sealed class ComplexReadSchema : IReadSchema
    {
        public struct Field
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

        public IReadOnlyList<Field> Fields { get; }

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
                .Select(p => new Field(p.Name, schemaCollection.GetReadSchema(p.ParameterType), isRequired: !p.HasDefaultValue))
                .ToList();
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool allowWideningConversion)
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
                    if (!rf.Schema.CanReadFrom(wf.Schema, allowWideningConversion))
                        return false;

                    // step both forwards
                    ir++;
                    iw++;
                }
                else if (cmp > 0)
                {
                    // write field comes before read field -- write type contains an extra field
                    if (!allowWideningConversion)
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

            if (iw != writeFields.Count && !allowWideningConversion)
                return false;

            return true;
        }
    }

    #endregion

    #region Nullable

    internal sealed class NullableWriteSchema : IWriteSchema
    {
        public static bool CanProcess(Type type) => Nullable.GetUnderlyingType(type) != null;

        [NotNull]
        public IWriteSchema Inner { get; }

        public NullableWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be nullable.", nameof(type));
            Inner = schemaCollection.GetWriteSchema(Nullable.GetUnderlyingType(type));
        }
    }

    internal sealed class NullableReadSchema : IReadSchema
    {
        public static bool CanProcess(Type type) => NullableWriteSchema.CanProcess(type);

        [NotNull]
        public IReadSchema Inner { get; }

        public NullableReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be nullable.", nameof(type));
            Inner = schemaCollection.GetReadSchema(Nullable.GetUnderlyingType(type));
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool allowWideningConversion)
        {
            // TODO only accept non-nullable writer if allowWideningConversion?
            // If the writer was nullable, unwrap its inner schema
            var schema = (writeSchema as NullableWriteSchema)?.Inner ?? writeSchema;

            return Inner.CanReadFrom(schema, allowWideningConversion);
        }
    }

    #endregion

    #region IReadOnlyList

    internal sealed class ListWriteSchema : IWriteSchema
    {
        public static bool CanProcess(Type type) => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>);

        [NotNull]
        public IWriteSchema ItemSchema { get; }

        public ListWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyList<int>)}<>.", nameof(type));
            ItemSchema = schemaCollection.GetWriteSchema(type.GetGenericArguments().Single());
        }
    }

    internal sealed class ListReadSchema : IReadSchema
    {
        public static bool CanProcess(Type type) => ListWriteSchema.CanProcess(type);

        [NotNull]
        public IReadSchema ItemSchema { get; }

        public ListReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyList<int>)}<>.", nameof(type));
            ItemSchema = schemaCollection.GetReadSchema(type.GetGenericArguments().Single());
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool allowWideningConversion)
        {
            var ws = writeSchema as ListWriteSchema;
            if (ws == null)
                return false;
            return ItemSchema.CanReadFrom(ws.ItemSchema, allowWideningConversion);
        }
    }

    #endregion

    #region IReadOnlyDictionary

    internal sealed class DictionaryWriteSchema : IWriteSchema
    {
        public static bool CanProcess(Type type) => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>);

        [NotNull] public IWriteSchema KeySchema { get; }
        [NotNull] public IWriteSchema ValueSchema { get; }

        public DictionaryWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyDictionary<int,int>)}<>.", nameof(type));
            KeySchema = schemaCollection.GetWriteSchema(type.GetGenericArguments()[0]);
            ValueSchema = schemaCollection.GetWriteSchema(type.GetGenericArguments()[1]);
        }
    }

    internal sealed class DictionaryReadSchema : IReadSchema
    {
        public static bool CanProcess(Type type) => DictionaryWriteSchema.CanProcess(type);

        [NotNull] public IReadSchema KeySchema { get; }
        [NotNull] public IReadSchema ValueSchema { get; }

        public DictionaryReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyDictionary<int, int>)}<>.", nameof(type));
            KeySchema = schemaCollection.GetReadSchema(type.GetGenericArguments()[0]);
            ValueSchema = schemaCollection.GetReadSchema(type.GetGenericArguments()[1]);
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool allowWideningConversion)
        {
            var ws = writeSchema as DictionaryWriteSchema;
            if (ws == null)
                return false;
            return KeySchema.CanReadFrom(ws.KeySchema, allowWideningConversion)
                   && ValueSchema.CanReadFrom(ws.ValueSchema, allowWideningConversion);
        }
    }

    #endregion

    #region Union

    internal sealed class UnionWriteSchema : IWriteSchema
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

        [NotNull]
        public IReadOnlyList<Member> Members { get; }

        public UnionWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be a union.", nameof(type));
            Members = Union.GetTypes(type)
                .Select(t => new Member(UnionEncoding.GetTypeName(t), schemaCollection.GetWriteSchema(t)))
                .OrderBy(m => m.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    internal sealed class UnionReadSchema : IReadSchema
    {
        public static bool CanProcess(Type type) => UnionWriteSchema.CanProcess(type);

        public struct Member
        {
            public string Id { get; }
            public IReadSchema Schema { get; }

            public Member(string id, IReadSchema schema)
            {
                Id = id;
                Schema = schema;
            }
        }

        [NotNull]
        public IReadOnlyList<Member> Members { get; }

        public UnionReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be a union.", nameof(type));
            Members = Union.GetTypes(type)
                .Select(t => new Member(UnionEncoding.GetTypeName(t), schemaCollection.GetReadSchema(t)))
                .OrderBy(m => m.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool allowWideningConversion)
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
                    if (!rm.Schema.CanReadFrom(wm.Schema, allowWideningConversion))
                        return false;

                    // step both forwards
                    ir++;
                    iw++;
                }
                else if (cmp < 0)
                {
                    // read member comes before write member -- read type contains an extra member
                    if (!allowWideningConversion)
                        return false;
                    // skip the read member only
                    iw++;
                }
                else
                {
                    return false;
                }
            }

            if (ir != readMembers.Count && !allowWideningConversion)
                return false;

            return true;
        }
    }

    #endregion
}
