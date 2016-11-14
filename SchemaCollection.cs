using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Dasher.TypeProviders;

namespace Dasher.Schema
{
    // TODO test XML writing
    // TODO test individual IEquatable implementations
    // TODO test all new interface methods
    // TODO implement FromXml

    // TODO support recursive types

    public sealed class SchemaCollection
    {
        public XElement ToXml()
        {
            var i = 0;
            foreach (var schema in _schema.OfType<IByRefSchema>())
                schema.Id = $"Schema{i++}";

            return new XElement("Schema",
                _schema.OfType<IByRefSchema>().Select(s => s.ToXml()));
        }

        public static SchemaCollection FromXml(IEnumerable<XElement> elements)
        {
            // TODO parse XML and build _schema collection
            throw new NotImplementedException();
        }

        private readonly List<ISchema> _schema = new List<ISchema>();

        public IWriteSchema GetWriteSchema(Type type)
        {
            if (type == typeof(EmptyMessage))
                return Intern(new EmptySchema());
            if (PrimitiveSchema.CanProcess(type))
                return Intern(new PrimitiveSchema(type));
            if (EnumSchema.CanProcess(type))
                return Intern(new EnumSchema(type));

            if (TupleWriteSchema.CanProcess(type))
                return Intern(new TupleWriteSchema(type, this));
            if (NullableWriteSchema.CanProcess(type))
                return Intern(new NullableWriteSchema(type, this));
            if (ListWriteSchema.CanProcess(type))
                return Intern(new ListWriteSchema(type, this));
            if (DictionaryWriteSchema.CanProcess(type))
                return Intern(new DictionaryWriteSchema(type, this));
            if (UnionWriteSchema.CanProcess(type))
                return Intern(new UnionWriteSchema(type, this));

            return Intern(new ComplexWriteSchema(type, this));
        }

        public IReadSchema GetReadSchema(Type type)
        {
            if (type == typeof(EmptyMessage))
                return Intern(new EmptySchema());
            if (PrimitiveSchema.CanProcess(type))
                return Intern(new PrimitiveSchema(type));
            if (EnumSchema.CanProcess(type))
                return Intern(new EnumSchema(type));

            if (TupleReadSchema.CanProcess(type))
                return Intern(new TupleReadSchema(type, this));
            if (NullableReadSchema.CanProcess(type))
                return Intern(new NullableReadSchema(type, this));
            if (ListReadSchema.CanProcess(type))
                return Intern(new ListReadSchema(type, this));
            if (DictionaryReadSchema.CanProcess(type))
                return Intern(new DictionaryReadSchema(type, this));
            if (UnionReadSchema.CanProcess(type))
                return Intern(new UnionReadSchema(type, this));

            return Intern(new ComplexReadSchema(type, this));
        }

        private T Intern<T>(T schema) where T : ISchema
        {
            Debug.Assert(schema is IByRefSchema || schema is IByValueSchema, "schema is IByRefSchema || schema is IByValueSchema");

            // TODO can we improve on a linear scan? create one list per type? multivaluedict on typeof(T)?
            foreach (var existing in _schema)
            {
                if (existing.Equals(schema))
                    return (T)existing;
            }
            _schema.Add(schema);
            return schema;
        }
    }

    internal static class EmptyArray<T>
    {
        public static T[] Instance { get; } = new T[0];
    }

    [SuppressMessage("ReSharper", "ConvertToStaticClass")]
    public sealed class EmptyMessage
    {
        private EmptyMessage() { }
    }

    internal interface IByRefSchema : ISchema // for complex, union and enum
    {
        string Id { get; set; }
        XElement ToXml();
    }

    internal interface IByValueSchema : ISchema // for primitive, nullable, list, dictionary, tuple, empty
    {
        string MarkupValue { get; }
    }

    internal interface ISchema : IEquatable<ISchema>
    {
        IEnumerable<ISchema> Children { get; }
    }

    public interface IWriteSchema
    { }

    public interface IReadSchema
    {
        bool CanReadFrom(IWriteSchema writeSchema, bool strict);
    }

    internal sealed class EnumSchema : IWriteSchema, IReadSchema, IByRefSchema
    {
        public static bool CanProcess(Type type) => type.IsEnum;

        private HashSet<string> MemberNames { get; }

        public EnumSchema(Type type)
        {
            if (!CanProcess(type))
                throw new ArgumentException("Must be an enum.", nameof(type));
            MemberNames = new HashSet<string>(Enum.GetNames(type), StringComparer.OrdinalIgnoreCase);
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

        IEnumerable<ISchema> ISchema.Children => EmptyArray<ISchema>.Instance;

        bool IEquatable<ISchema>.Equals(ISchema other)
        {
            var e = other as EnumSchema;
            return e != null && MemberNames.SetEquals(e.MemberNames);
        }

        string IByRefSchema.Id { get; set; }

        XElement IByRefSchema.ToXml()
        {
            return new XElement("Enum",
                new XAttribute("Id", ((IByRefSchema)this).Id),
                MemberNames.Select(m => new XElement(m)));
        }
    }

    internal sealed class EmptySchema : IWriteSchema, IReadSchema, IByValueSchema
    {
        public bool CanReadFrom(IWriteSchema writeSchema, bool strict) => writeSchema is EmptySchema || !strict;

        bool IEquatable<ISchema>.Equals(ISchema other) => other is EmptySchema;

        IEnumerable<ISchema> ISchema.Children => EmptyArray<ISchema>.Instance;

        string IByValueSchema.MarkupValue => "{empty}";
    }

    internal sealed class PrimitiveSchema : IWriteSchema, IReadSchema, IByValueSchema
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

        public bool CanReadFrom(IWriteSchema writeSchema, bool strict)
        {
            var ws = writeSchema as PrimitiveSchema;
            return ws != null && ws.TypeName == TypeName;
        }

        bool IEquatable<ISchema>.Equals(ISchema other)
        {
            var schema = other as PrimitiveSchema;
            return schema != null && schema.TypeName == TypeName;
        }

        IEnumerable<ISchema> ISchema.Children => EmptyArray<ISchema>.Instance;

        string IByValueSchema.MarkupValue => TypeName;
    }

    internal static class EnumerableExtensions
    {
        public static bool SequenceEqual<T>(this IEnumerable<T> a, IEnumerable<T> b, Func<T, T, bool> comparer)
        {
            if (typeof(ICollection).IsAssignableFrom(typeof(T)))
            {
                if (((ICollection)a).Count != ((ICollection)b).Count)
                    return false;
            }

            using (var ae = a.GetEnumerator())
            using (var be = b.GetEnumerator())
            {
                while (ae.MoveNext())
                {
                    var moved = be.MoveNext();
                    Debug.Assert(moved);
                    if (!comparer(ae.Current, be.Current))
                        return false;
                }

                Debug.Assert(!be.MoveNext());
                return true;
            }
        }
    }

    internal static class SchemaExtensions
    {
        public static string ToReferenceString(this IWriteSchema schema) => ToReferenceStringInternal(schema);

        public static string ToReferenceString(this IReadSchema schema) => ToReferenceStringInternal(schema);

        private static string ToReferenceStringInternal(object schema)
        {
            var byRefSchema = schema as IByRefSchema;
            return byRefSchema != null
                ? '#' + byRefSchema.Id
                : ((IByValueSchema)schema).MarkupValue;
        }
    }

    #region Tuple

    internal sealed class TupleWriteSchema : IWriteSchema, IByValueSchema
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

        bool IEquatable<ISchema>.Equals(ISchema other)
        {
            var s = other as TupleWriteSchema;
            return s != null && Items.SequenceEqual(s.Items, (a, b) => ((IEquatable<ISchema>)a).Equals((ISchema)b));
        }

        IEnumerable<ISchema> ISchema.Children => Items.Cast<ISchema>();

        string IByValueSchema.MarkupValue
        {
            get { return $"{{tuple {string.Join(" ", Items.Select(i => i.ToReferenceString()))}}}"; }
        }
    }

    internal sealed class TupleReadSchema : IReadSchema, IByValueSchema
    {
        public static bool CanProcess(Type type) => TupleWriteSchema.CanProcess(type);

        private IReadOnlyList<IReadSchema> Items { get; }

        public TupleReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!TupleWriteSchema.CanProcess(type))
                throw new ArgumentException($"Type {type} is not a supported tuple type.", nameof(type));

            Items = type.GetGenericArguments().Select(schemaCollection.GetReadSchema).ToList();
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool strict)
        {
            var that = writeSchema as TupleWriteSchema;

            return that?.Items.Count == Items.Count
                   && !Items.Where((rs, i) => !rs.CanReadFrom(that.Items[i], strict)).Any();
        }

        bool IEquatable<ISchema>.Equals(ISchema other)
        {
            var s = other as TupleReadSchema;
            return s != null && Items.SequenceEqual(s.Items, (a, b) => ((IEquatable<ISchema>)a).Equals((ISchema)b));
        }

        IEnumerable<ISchema> ISchema.Children => Items.Cast<ISchema>();

        string IByValueSchema.MarkupValue
        {
            get { return $"{{tuple {string.Join(" ", Items.Select(i => i.ToReferenceString()))}}}"; }
        }
    }

    #endregion

    #region Complex

    internal sealed class ComplexWriteSchema : IWriteSchema, IByRefSchema
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

        bool IEquatable<ISchema>.Equals(ISchema other)
        {
            var s = other as ComplexWriteSchema;
            return s != null && Fields.SequenceEqual(s.Fields, (a, b) => ((IEquatable<ISchema>)a.Schema).Equals(b.Schema));
        }

        IEnumerable<ISchema> ISchema.Children => Fields.Select(f => f.Schema).Cast<ISchema>();

        string IByRefSchema.Id { get; set; }

        XElement IByRefSchema.ToXml()
        {
            return new XElement("ComplexWrite",
                new XAttribute("Id", ((IByRefSchema)this).Id),
                Fields.Select(f => new XElement("Field",
                    new XAttribute("Name", f.Name),
                    new XAttribute("Schema", f.Schema.ToReferenceString()))));
        }
    }

    internal sealed class ComplexReadSchema : IReadSchema, IByRefSchema
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

        bool IEquatable<ISchema>.Equals(ISchema other)
        {
            var s = other as ComplexReadSchema;
            return s != null && Fields.SequenceEqual(s.Fields, (a, b) => ((IEquatable<ISchema>)a.Schema).Equals(b.Schema));
        }

        IEnumerable<ISchema> ISchema.Children => Fields.Select(f => f.Schema).Cast<ISchema>();

        string IByRefSchema.Id { get; set; }

        XElement IByRefSchema.ToXml()
        {
            return new XElement("ComplexRead",
                new XAttribute("Id", ((IByRefSchema)this).Id),
                Fields.Select(f => new XElement("Field",
                    new XAttribute("Name", f.Name),
                    new XAttribute("Schema", f.Schema.ToReferenceString()),
                    new XAttribute("IsRequired", f.IsRequired))));
        }
    }

    #endregion

    [Flags]
    enum CompatabilityLevel
    {
        Strict,
        AllowExtraFieldsOnComplex,
        AllowFewerMembersInEnum,
        AllowFewerMembersInUnion,
        AllowWideningIntegralTypes,
        AllowWideningFloatingPointTypes,
        AllowMakingNullable,
        Lenient // = AllowExtraFieldsOnComplex | AllowFewerMembersInEnum | AllowFewerMembersInUnion | AllowLosslessTypeConversion
    }

    #region Nullable

    internal sealed class NullableWriteSchema : IWriteSchema, IByValueSchema
    {
        public static bool CanProcess(Type type) => Nullable.GetUnderlyingType(type) != null;

        public IWriteSchema Inner { get; }

        public NullableWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be nullable.", nameof(type));
            Inner = schemaCollection.GetWriteSchema(Nullable.GetUnderlyingType(type));
        }

        bool IEquatable<ISchema>.Equals(ISchema other)
        {
            var o = other as NullableWriteSchema;
            return o != null && ((ISchema)o.Inner).Equals((ISchema)Inner);
        }

        IEnumerable<ISchema> ISchema.Children => new[] {(ISchema)Inner};

        string IByValueSchema.MarkupValue => $"{{nullable {Inner.ToReferenceString()}}}";
    }

    internal sealed class NullableReadSchema : IReadSchema, IByValueSchema
    {
        public static bool CanProcess(Type type) => NullableWriteSchema.CanProcess(type);

        public IReadSchema Inner { get; }

        public NullableReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be nullable.", nameof(type));
            Inner = schemaCollection.GetReadSchema(Nullable.GetUnderlyingType(type));
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

        bool IEquatable<ISchema>.Equals(ISchema other)
        {
            var o = other as NullableReadSchema;
            return o != null && ((ISchema)o.Inner).Equals((ISchema)Inner);
        }

        IEnumerable<ISchema> ISchema.Children => new[] {(ISchema)Inner};

        string IByValueSchema.MarkupValue => $"{{nullable {Inner.ToReferenceString()}}}";
    }

    #endregion

    #region IReadOnlyList

    internal sealed class ListWriteSchema : IWriteSchema, IByValueSchema
    {
        public static bool CanProcess(Type type) => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>);

        public IWriteSchema ItemSchema { get; }

        public ListWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyList<int>)}<>.", nameof(type));
            ItemSchema = schemaCollection.GetWriteSchema(type.GetGenericArguments().Single());
        }

        bool IEquatable<ISchema>.Equals(ISchema other)
        {
            var o = other as ListWriteSchema;
            return o != null && ((ISchema)o.ItemSchema).Equals((ISchema)ItemSchema);
        }

        IEnumerable<ISchema> ISchema.Children => new[] {(ISchema)ItemSchema};

        string IByValueSchema.MarkupValue => $"{{list {ItemSchema.ToReferenceString()}}}";
    }

    internal sealed class ListReadSchema : IReadSchema, IByValueSchema
    {
        public static bool CanProcess(Type type) => ListWriteSchema.CanProcess(type);

        public IReadSchema ItemSchema { get; }

        public ListReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyList<int>)}<>.", nameof(type));
            ItemSchema = schemaCollection.GetReadSchema(type.GetGenericArguments().Single());
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool strict)
        {
            var ws = writeSchema as ListWriteSchema;
            if (ws == null)
                return false;
            return ItemSchema.CanReadFrom(ws.ItemSchema, strict);
        }

        bool IEquatable<ISchema>.Equals(ISchema other)
        {
            var o = other as ListReadSchema;
            return o != null && ((ISchema)o.ItemSchema).Equals((ISchema)ItemSchema);
        }

        IEnumerable<ISchema> ISchema.Children => new[] {(ISchema)ItemSchema};

        string IByValueSchema.MarkupValue => $"{{list {ItemSchema.ToReferenceString()}}}";
    }

    #endregion

    #region IReadOnlyDictionary

    internal sealed class DictionaryWriteSchema : IWriteSchema, IByValueSchema
    {
        public static bool CanProcess(Type type) => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>);

        public IWriteSchema KeySchema { get; }
        public IWriteSchema ValueSchema { get; }

        public DictionaryWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyDictionary<int,int>)}<>.", nameof(type));
            KeySchema = schemaCollection.GetWriteSchema(type.GetGenericArguments()[0]);
            ValueSchema = schemaCollection.GetWriteSchema(type.GetGenericArguments()[1]);
        }

        bool IEquatable<ISchema>.Equals(ISchema other)
        {
            var o = other as DictionaryWriteSchema;
            return o != null && ((ISchema)o.KeySchema).Equals((ISchema)KeySchema) && ((ISchema)o.ValueSchema).Equals((ISchema)ValueSchema);
        }

        IEnumerable<ISchema> ISchema.Children => new[] {(ISchema)KeySchema, (ISchema)ValueSchema};

        string IByValueSchema.MarkupValue => $"{{dictionary {KeySchema.ToReferenceString()} {ValueSchema.ToReferenceString()}}}";
    }

    internal sealed class DictionaryReadSchema : IReadSchema, IByValueSchema
    {
        public static bool CanProcess(Type type) => DictionaryWriteSchema.CanProcess(type);

        public IReadSchema KeySchema { get; }
        public IReadSchema ValueSchema { get; }

        public DictionaryReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyDictionary<int, int>)}<>.", nameof(type));
            KeySchema = schemaCollection.GetReadSchema(type.GetGenericArguments()[0]);
            ValueSchema = schemaCollection.GetReadSchema(type.GetGenericArguments()[1]);
        }

        public bool CanReadFrom(IWriteSchema writeSchema, bool strict)
        {
            var ws = writeSchema as DictionaryWriteSchema;
            if (ws == null)
                return false;
            return KeySchema.CanReadFrom(ws.KeySchema, strict) &&
                   ValueSchema.CanReadFrom(ws.ValueSchema, strict);
        }

        bool IEquatable<ISchema>.Equals(ISchema other)
        {
            var o = other as DictionaryReadSchema;
            return o != null && ((ISchema)o.KeySchema).Equals((ISchema)KeySchema) && ((ISchema)o.ValueSchema).Equals((ISchema)ValueSchema);
        }

        IEnumerable<ISchema> ISchema.Children => new[] {(ISchema)KeySchema, (ISchema)ValueSchema};

        string IByValueSchema.MarkupValue => $"{{dictionary {KeySchema.ToReferenceString()} {ValueSchema.ToReferenceString()}}}";
    }

    #endregion

    #region Union

    internal sealed class UnionWriteSchema : IWriteSchema, IByRefSchema
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
                .Select(t => new Member(UnionEncoding.GetTypeName(t), schemaCollection.GetWriteSchema(t)))
                .OrderBy(m => m.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        bool IEquatable<ISchema>.Equals(ISchema other)
        {
            var o = other as UnionWriteSchema;
            return o != null && o.Members.SequenceEqual(Members, (a, b) => a.Id == b.Id && ((ISchema)a.Schema).Equals((ISchema)b.Schema));
        }

        IEnumerable<ISchema> ISchema.Children => Members.Select(m => m.Schema).Cast<ISchema>();

        string IByRefSchema.Id { get; set; }

        XElement IByRefSchema.ToXml()
        {
            return new XElement("Union",
                new XAttribute("Id", ((IByRefSchema)this).Id),
                Members.Select(m => new XElement("Member",
                    new XAttribute("Id", m.Id),
                    new XAttribute("Schema", m.Schema.ToReferenceString()))));
        }
    }

    internal sealed class UnionReadSchema : IReadSchema, IByRefSchema
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

        bool IEquatable<ISchema>.Equals(ISchema other)
        {
            var o = other as UnionReadSchema;
            return o != null && o.Members.SequenceEqual(Members, (a, b) => a.Id == b.Id && ((ISchema)a.Schema).Equals((ISchema)b.Schema));
        }

        IEnumerable<ISchema> ISchema.Children => Members.Select(m => m.Schema).Cast<ISchema>();

        string IByRefSchema.Id { get; set; }

        XElement IByRefSchema.ToXml()
        {
            return new XElement("Union",
                new XAttribute("Id", ((IByRefSchema)this).Id),
                Members.Select(m => new XElement("Member",
                    new XAttribute("Id", m.Id),
                    new XAttribute("Schema", m.Schema.ToReferenceString()))));
        }
    }

    #endregion
}
