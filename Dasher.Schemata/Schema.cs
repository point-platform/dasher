using System.Collections.Generic;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace Dasher.Schemata
{
    public interface IWriteSchema
    {
        /// <summary>
        /// Creates a deep copy of this schema within <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        IWriteSchema CopyTo(SchemaCollection collection);
    }

    public interface IReadSchema
    {
        bool CanReadFrom(IWriteSchema writeSchema, bool strict);

        /// <summary>
        /// Creates a deep copy of this schema within <paramref name="collection"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        IReadSchema CopyTo(SchemaCollection collection);
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
        [CanBeNull]
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
}