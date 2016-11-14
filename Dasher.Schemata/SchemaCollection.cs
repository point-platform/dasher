using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Dasher.TypeProviders;

namespace Dasher.Schemata
{
    public sealed class SchemaCollection
    {
        #region SchemaResolver

        private sealed class SchemaResolver
        {
            private readonly Dictionary<string, ByRefSchema> _schemaById = new Dictionary<string, ByRefSchema>();
            private readonly SchemaCollection _collection;
            private bool _allowResolution;

            public SchemaResolver(SchemaCollection collection)
            {
                _collection = collection;
            }

            public void AddByRefSchema(string id, ByRefSchema schema)
            {
                if (_schemaById.ContainsKey(id))
                    throw new SchemaParseException($"Duplicate schema Id \"{id}\".");
                _schemaById[id] = schema;
            }

            public void AllowResolution()
            {
                _allowResolution = true;
            }

            public IReadSchema ResolveReadSchema(string str)
            {
                if (!_allowResolution)
                    throw new InvalidOperationException("Cannot resolve schema at this stage. Use a bind action instead.");

                if (str.StartsWith("#"))
                {
                    var id = str.Substring(1);
                    ByRefSchema schema;
                    if (!_schemaById.TryGetValue(id, out schema))
                        throw new SchemaParseException($"Unresolved schema reference \"{str}\"");
                    var readSchema = schema as IReadSchema;
                    if (readSchema == null)
                        throw new SchemaParseException($"Referenced schema \"{str}\" must be a read schema.");
                    return readSchema;
                }

                if (str.StartsWith("{"))
                {
                    var tokens = SchemaMarkupExtension.Tokenize(str).ToList();

                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (tokens.Count)
                    {
                        case 1:
                            if (tokens[0] == "empty")
                                return _collection.Intern(new EmptySchema());
                            break;
                        case 2:
                            if (tokens[0] == "nullable")
                                return _collection.Intern(new NullableReadSchema(ResolveReadSchema(tokens[1])));
                            if (tokens[0] == "list")
                                return _collection.Intern(new ListReadSchema(ResolveReadSchema(tokens[1])));
                            break;
                        case 3:
                            if (tokens[0] == "dictionary")
                                return _collection.Intern(new DictionaryReadSchema(ResolveReadSchema(tokens[1]), ResolveReadSchema(tokens[2])));
                            break;
                    }

                    if (tokens.Count != 0 && tokens[0] == "tuple")
                        return _collection.Intern(new TupleReadSchema(tokens.Skip(1).Select(ResolveReadSchema).ToList()));

                    throw new SchemaParseException($"Invalid schema markup extension \"{str}\".");
                }

                return _collection.Intern(new PrimitiveSchema(str));
            }

            public IWriteSchema ResolveWriteSchema(string str)
            {
                if (!_allowResolution)
                    throw new InvalidOperationException("Cannot resolve schema at this stage. Use a bind action instead.");

                if (str.StartsWith("#"))
                {
                    var id = str.Substring(1);
                    ByRefSchema schema;
                    if (!_schemaById.TryGetValue(id, out schema))
                        throw new SchemaParseException($"Unresolved schema reference \"{str}\"");
                    var writeSchema = schema as IWriteSchema;
                    if (writeSchema == null)
                        throw new SchemaParseException($"Referenced schema \"{str}\" must be a write schema.");
                    return writeSchema;
                }

                if (str.StartsWith("{"))
                {
                    var tokens = SchemaMarkupExtension.Tokenize(str).ToList();

                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (tokens.Count)
                    {
                        case 1:
                            if (tokens[0] == "empty")
                                return _collection.Intern(new EmptySchema());
                            break;
                        case 2:
                            if (tokens[0] == "nullable")
                                return _collection.Intern(new NullableWriteSchema(ResolveWriteSchema(tokens[1])));
                            if (tokens[0] == "list")
                                return _collection.Intern(new ListWriteSchema(ResolveWriteSchema(tokens[1])));
                            break;
                        case 3:
                            if (tokens[0] == "dictionary")
                                return _collection.Intern(new DictionaryWriteSchema(ResolveWriteSchema(tokens[1]), ResolveWriteSchema(tokens[2])));
                            break;
                    }

                    if (tokens.Count != 0 && tokens[0] == "tuple")
                        return _collection.Intern(new TupleWriteSchema(tokens.Skip(1).Select(ResolveWriteSchema).ToList()));

                    throw new SchemaParseException($"Invalid schema markup extension \"{str}\".");
                }

                return _collection.Intern(new PrimitiveSchema(str));
            }
        }

        #endregion

        #region To/From XML

        public XElement ToXml()
        {
            // TODO revisit how IDs are assigned
            var i = 0;
            foreach (var schema in Schema.OfType<ByRefSchema>())
                schema.Id = $"Schema{i++}";

            return new XElement("Schema",
                Schema.OfType<ByRefSchema>().Select(s => s.ToXml()));
        }

        public static SchemaCollection FromXml(XElement element)
        {
            var bindActions = new List<Action>();
            var unboundSchemata = new List<Schema>();

            var collection = new SchemaCollection();
            var resolver = new SchemaResolver(collection);

            foreach (var el in element.Elements())
            {
                var id = el.Attribute("Id")?.Value;

                if (string.IsNullOrWhiteSpace(id))
                    throw new SchemaParseException("Schema XML element must contain a non-empty \"Id\" attribute.");

                ByRefSchema schema;
                switch (el.Name.LocalName)
                {
                    case "ComplexRead":
                        schema = new ComplexReadSchema(el, resolver.ResolveReadSchema, bindActions);
                        break;
                    case "ComplexWrite":
                        schema = new ComplexWriteSchema(el, resolver.ResolveWriteSchema, bindActions);
                        break;
                    case "UnionRead":
                        schema = new UnionReadSchema(el, resolver.ResolveReadSchema, bindActions);
                        break;
                    case "UnionWrite":
                        schema = new UnionWriteSchema(el, resolver.ResolveWriteSchema, bindActions);
                        break;
                    case "Enum":
                        schema = new EnumSchema(el);
                        break;
                    default:
                        throw new SchemaParseException($"Unsupported schema XML element with name \"{el.Name.LocalName}\".");
                }

                schema.Id = id;
                resolver.AddByRefSchema(id, schema);
                // We can't add these to the collection until after bind actions execute
                unboundSchemata.Add(schema);
            }

            resolver.AllowResolution();

            foreach (var bindAction in bindActions)
                bindAction();

            foreach (var schema in unboundSchemata)
                collection.Intern(schema);

            return collection;
        }

        #endregion

        private readonly Dictionary<Schema, Schema> _schema = new Dictionary<Schema, Schema>();

        internal ICollection<Schema> Schema => _schema.Keys;

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

        private T Intern<T>(T schema) where T : Schema
        {
            Debug.Assert(schema is ByRefSchema || schema is ByValueSchema, "schema is ByRefSchema || schema is ByValueSchema");

            Schema existing;
            if (_schema.TryGetValue(schema, out existing))
                return (T)existing;

            _schema.Add(schema, schema);
            return schema;
        }
    }

    [SuppressMessage("ReSharper", "ConvertToStaticClass")]
    public sealed class EmptyMessage
    {
        private EmptyMessage() { }
    }

    public interface IWriteSchema
    { }

    public interface IReadSchema
    {
        bool CanReadFrom(IWriteSchema writeSchema, bool strict);
    }

    public abstract class Schema
    {
        internal abstract IEnumerable<Schema> Children { get; }

        public override bool Equals(object obj)
        {
            var other = obj as Schema;
            return other != null && Equals(other);
        }

        public abstract bool Equals(Schema other);

        public override int GetHashCode() => ComputeHashCode();

        protected abstract int ComputeHashCode();
    }

    /// <summary>For complex, union and enum.</summary>
    public abstract class ByRefSchema : Schema
    {
        internal string Id { get; set; }
        internal abstract XElement ToXml();
        public override string ToString() => Id;
    }

    /// <summary>For primitive, nullable, list, dictionary, tuple, empty.</summary>
    public abstract class ByValueSchema : Schema
    {
        internal abstract string MarkupValue { get; }
        public override string ToString() => MarkupValue;
    }

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
            return new XElement("Enum",
                new XAttribute("Id", Id),
                MemberNames.Select(m => new XElement(m)));
        }
    }

    internal sealed class EmptySchema : ByValueSchema, IWriteSchema, IReadSchema
    {
        public bool CanReadFrom(IWriteSchema writeSchema, bool strict) => writeSchema is EmptySchema || !strict;

        public override bool Equals(Schema other) => other is EmptySchema;

        internal override IEnumerable<Schema> Children => EmptyArray<Schema>.Instance;

        internal override string MarkupValue => "{empty}";

        protected override int ComputeHashCode() => MarkupValue.GetHashCode();
    }

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
    }

    #region Tuple

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

            Items = type.GetGenericArguments().Select(schemaCollection.GetWriteSchema).ToList();
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

    internal sealed class TupleReadSchema : ByValueSchema, IReadSchema
    {
        public static bool CanProcess(Type type) => TupleWriteSchema.CanProcess(type);

        private IReadOnlyList<IReadSchema> Items { get; }

        public TupleReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!TupleWriteSchema.CanProcess(type))
                throw new ArgumentException($"Type {type} is not a supported tuple type.", nameof(type));

            Items = type.GetGenericArguments().Select(schemaCollection.GetReadSchema).ToList();
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

    #endregion

    #region Complex

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
            Fields = properties.Select(p => new Field(p.Name, schemaCollection.GetWriteSchema(p.PropertyType))).ToArray();
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
            return new XElement("ComplexWrite",
                new XAttribute("Id", Id),
                Fields.Select(f => new XElement("Field",
                    new XAttribute("Name", f.Name),
                    new XAttribute("Schema", f.Schema.ToReferenceString()))));
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
                .Select(p => new Field(p.Name, schemaCollection.GetReadSchema(p.ParameterType), isRequired: !p.HasDefaultValue))
                .ToList();
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
            return new XElement("ComplexRead",
                new XAttribute("Id", Id),
                Fields.Select(f => new XElement(nameof(Field),
                    new XAttribute(nameof(Field.Name), f.Name),
                    new XAttribute(nameof(Field.Schema), f.Schema.ToReferenceString()),
                    new XAttribute(nameof(Field.IsRequired), f.IsRequired))));
        }
    }

    #endregion

    #region Nullable

    internal sealed class NullableWriteSchema : ByValueSchema, IWriteSchema
    {
        public static bool CanProcess(Type type) => Nullable.GetUnderlyingType(type) != null;

        public IWriteSchema Inner { get; }

        public NullableWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be nullable.", nameof(type));
            Inner = schemaCollection.GetWriteSchema(Nullable.GetUnderlyingType(type));
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

        internal override IEnumerable<Schema> Children => new[] {(Schema)Inner};

        internal override string MarkupValue => $"{{nullable {Inner.ToReferenceString()}}}";
    }

    internal sealed class NullableReadSchema : ByValueSchema, IReadSchema
    {
        public static bool CanProcess(Type type) => NullableWriteSchema.CanProcess(type);

        private IReadSchema Inner { get; }

        public NullableReadSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be nullable.", nameof(type));
            Inner = schemaCollection.GetReadSchema(Nullable.GetUnderlyingType(type));
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

        internal override IEnumerable<Schema> Children => new[] {(Schema)Inner};

        internal override string MarkupValue => $"{{nullable {Inner.ToReferenceString()}}}";
    }

    #endregion

    #region IReadOnlyList

    internal sealed class ListWriteSchema : ByValueSchema, IWriteSchema
    {
        public static bool CanProcess(Type type) => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>);

        public IWriteSchema ItemSchema { get; }

        public ListWriteSchema(Type type, SchemaCollection schemaCollection)
        {
            if (!CanProcess(type))
                throw new ArgumentException($"Type {type} must be {nameof(IReadOnlyList<int>)}<>.", nameof(type));
            ItemSchema = schemaCollection.GetWriteSchema(type.GetGenericArguments().Single());
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

        internal override IEnumerable<Schema> Children => new[] {(Schema)ItemSchema};

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
            ItemSchema = schemaCollection.GetReadSchema(type.GetGenericArguments().Single());
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

        internal override IEnumerable<Schema> Children => new[] {(Schema)ItemSchema};

        internal override string MarkupValue => $"{{list {ItemSchema.ToReferenceString()}}}";
    }

    #endregion

    #region IReadOnlyDictionary

    internal sealed class DictionaryWriteSchema : ByValueSchema, IWriteSchema
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

        internal override IEnumerable<Schema> Children => new[] {(Schema)KeySchema, (Schema)ValueSchema};

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
            KeySchema = schemaCollection.GetReadSchema(type.GetGenericArguments()[0]);
            ValueSchema = schemaCollection.GetReadSchema(type.GetGenericArguments()[1]);
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

        internal override IEnumerable<Schema> Children => new[] {(Schema)KeySchema, (Schema)ValueSchema};

        internal override string MarkupValue => $"{{dictionary {KeySchema.ToReferenceString()} {ValueSchema.ToReferenceString()}}}";
    }

    #endregion

    #region Union

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
                .Select(t => new Member(UnionEncoding.GetTypeName(t), schemaCollection.GetWriteSchema(t)))
                .OrderBy(m => m.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
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
            return new XElement("UnionWrite",
                new XAttribute("Id", Id),
                Members.Select(m => new XElement("Member",
                    new XAttribute("Id", m.Id),
                    new XAttribute("Schema", m.Schema.ToReferenceString()))));
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
                .Select(t => new Member(UnionEncoding.GetTypeName(t), schemaCollection.GetReadSchema(t)))
                .OrderBy(m => m.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
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
            return new XElement("UnionRead",
                new XAttribute("Id", Id),
                Members.Select(m => new XElement("Member",
                    new XAttribute("Id", m.Id),
                    new XAttribute("Schema", m.Schema.ToReferenceString()))));
        }
    }

    #endregion
}
