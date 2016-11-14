using System.Collections.Generic;
using Dasher.Schemata.Utils;

namespace Dasher.Schemata.Types
{
    internal sealed class EmptySchema : ByValueSchema, IWriteSchema, IReadSchema
    {
        public bool CanReadFrom(IWriteSchema writeSchema, bool strict) => writeSchema is EmptySchema || !strict;

        public override bool Equals(Schema other) => other is EmptySchema;

        internal override IEnumerable<Schema> Children => EmptyArray<Schema>.Instance;

        internal override string MarkupValue => "{empty}";

        protected override int ComputeHashCode() => MarkupValue.GetHashCode();
    }
}