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
        private readonly Dictionary<Schema, Schema> _schema = new Dictionary<Schema, Schema>();

        internal ICollection<Schema> Schema => _schema.Keys;

        #region Schema resolution

        private bool AllowResolution { get; set; }

        public IReadSchema ResolveReadSchema(string str)
        {
            IReadSchema schema;
            if (!TryResolveReadSchema(str, out schema))
                throw new Exception("String \"{str}\" cannot be resolved as a read schema within this collection.");
            return schema;
        }

        public bool TryResolveReadSchema(string str, out IReadSchema readSchema)
        {
            if (!AllowResolution)
                throw new InvalidOperationException("Cannot resolve schema at this stage. Use a bind action instead.");

            if (str.StartsWith("#"))
            {
                var id = str.Substring(1);
                readSchema = Schema.OfType<ByRefSchema>().SingleOrDefault(s => s.Id == id) as IReadSchema;
                return readSchema != null;
            }

            if (str.StartsWith("{"))
            {
                var tokens = SchemaMarkupExtension.Tokenize(str).ToList();

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (tokens.Count)
                {
                    case 1:
                        if (tokens[0] == "empty")
                        {
                            readSchema = Intern(new EmptySchema());
                            return true;
                        }
                        break;
                    case 2:
                        if (tokens[0] == "nullable")
                        {
                            IReadSchema inner;
                            if (TryResolveReadSchema(tokens[1], out inner))
                            {
                                readSchema = Intern(new NullableReadSchema(inner));
                                return true;
                            }
                        }
                        else if (tokens[0] == "list")
                        {
                            IReadSchema itemSchema;
                            if (TryResolveReadSchema(tokens[1], out itemSchema))
                            {
                                readSchema = Intern(new ListReadSchema(itemSchema));
                                return true;
                            }
                        }
                        break;
                    case 3:
                        if (tokens[0] == "dictionary")
                        {
                            IReadSchema keySchema;
                            IReadSchema valueSchema;
                            if (TryResolveReadSchema(tokens[1], out keySchema) && TryResolveReadSchema(tokens[2], out valueSchema))
                            {
                                readSchema = Intern(new DictionaryReadSchema(keySchema, valueSchema));
                                return true;
                            }
                        }
                        break;
                }

                if (tokens.Count != 0 && tokens[0] == "tuple")
                {
                    var itemSchemata = new List<IReadSchema>();
                    foreach (var token in tokens.Skip(1))
                    {
                        IReadSchema itemSchema;
                        if (!TryResolveReadSchema(token, out itemSchema))
                        {
                            readSchema = null;
                            return false;
                        }
                        itemSchemata.Add(itemSchema);
                    }
                    readSchema = Intern(new TupleReadSchema(itemSchemata));
                    return true;
                }

                readSchema = null;
                return false;
            }

            readSchema = Intern(new PrimitiveSchema(str));
            return true;
        }

        public IWriteSchema ResolveWriteSchema(string str)
        {
            IWriteSchema schema;
            if (!TryResolveWriteSchema(str, out schema))
                throw new Exception("String \"{str}\" cannot be resolved as a write schema within this collection.");
            return schema;
        }

        public bool TryResolveWriteSchema(string str, out IWriteSchema writeSchema)
        {
            if (!AllowResolution)
                throw new InvalidOperationException("Cannot resolve schema at this stage. Use a bind action instead.");

            if (str.StartsWith("#"))
            {
                var id = str.Substring(1);
                writeSchema = Schema.OfType<ByRefSchema>().SingleOrDefault(s => s.Id == id) as IWriteSchema;
                return writeSchema != null;
            }

            if (str.StartsWith("{"))
            {
                var tokens = SchemaMarkupExtension.Tokenize(str).ToList();

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (tokens.Count)
                {
                    case 1:
                        if (tokens[0] == "empty")
                        {
                            writeSchema = Intern(new EmptySchema());
                            return true;
                        }
                        break;
                    case 2:
                        if (tokens[0] == "nullable")
                        {
                            IWriteSchema inner;
                            if (TryResolveWriteSchema(tokens[1], out inner))
                            {
                                writeSchema = Intern(new NullableWriteSchema(inner));
                                return true;
                            }
                        }
                        else if (tokens[0] == "list")
                        {
                            IWriteSchema itemSchema;
                            if (TryResolveWriteSchema(tokens[1], out itemSchema))
                            {
                                writeSchema = Intern(new ListWriteSchema(itemSchema));
                                return true;
                            }
                        }
                        break;
                    case 3:
                        if (tokens[0] == "dictionary")
                        {
                            IWriteSchema keySchema;
                            IWriteSchema valueSchema;
                            if (TryResolveWriteSchema(tokens[1], out keySchema) && TryResolveWriteSchema(tokens[2], out valueSchema))
                            {
                                writeSchema = Intern(new DictionaryWriteSchema(keySchema, valueSchema));
                                return true;
                            }
                        }
                        break;
                }

                if (tokens.Count != 0 && tokens[0] == "tuple")
                {
                    var itemSchemata = new List<IWriteSchema>();
                    foreach (var token in tokens.Skip(1))
                    {
                        IWriteSchema itemSchema;
                        if (!TryResolveWriteSchema(token, out itemSchema))
                        {
                            writeSchema = null;
                            return false;
                        }
                        itemSchemata.Add(itemSchema);
                    }
                    writeSchema = Intern(new TupleWriteSchema(itemSchemata));
                    return true;
                }

                writeSchema = null;
                return false;
            }

            writeSchema = Intern(new PrimitiveSchema(str));
            return true;
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

            foreach (var el in element.Elements())
            {
                var id = el.Attribute("Id")?.Value;

                if (string.IsNullOrWhiteSpace(id))
                    throw new SchemaParseException("Schema XML element must contain a non-empty \"Id\" attribute.");

                ByRefSchema schema;
                switch (el.Name.LocalName)
                {
                    case "ComplexRead":
                        schema = new ComplexReadSchema(el, collection.ResolveReadSchema, bindActions);
                        break;
                    case "ComplexWrite":
                        schema = new ComplexWriteSchema(el, collection.ResolveWriteSchema, bindActions);
                        break;
                    case "UnionRead":
                        schema = new UnionReadSchema(el, collection.ResolveReadSchema, bindActions);
                        break;
                    case "UnionWrite":
                        schema = new UnionWriteSchema(el, collection.ResolveWriteSchema, bindActions);
                        break;
                    case "Enum":
                        schema = new EnumSchema(el);
                        break;
                    default:
                        throw new SchemaParseException($"Unsupported schema XML element with name \"{el.Name.LocalName}\".");
                }

                schema.Id = id;

                // We can't add these to the collection until after bind actions execute
                unboundSchemata.Add(schema);
            }

            collection.AllowResolution = true;

            foreach (var bindAction in bindActions)
                bindAction();

            foreach (var schema in unboundSchemata)
                collection.Intern(schema);

            return collection;
        }

        #endregion

        public IWriteSchema GetOrAddWriteSchema(Type type)
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

        public IReadSchema GetOrAddReadSchema(Type type)
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
