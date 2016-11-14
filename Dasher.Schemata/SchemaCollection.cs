using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Dasher.Schemata.Types;
using Dasher.Schemata.Utils;

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
}
