using System;

namespace Dasher.Schemata.Utils
{
    public static class SchemaExtensions
    {
        public static string ToReferenceString(this IWriteSchema schema) => ToReferenceStringInternal(schema);

        public static string ToReferenceString(this IReadSchema schema) => ToReferenceStringInternal(schema);

        private static string ToReferenceStringInternal(object schema)
        {
            var byRefSchema = schema as ByRefSchema;

            if (byRefSchema != null)
            {
                if (string.IsNullOrWhiteSpace(byRefSchema.Id))
                    throw new Exception("ByRefSchema must have an ID to produce a reference string.");
                return '#' + byRefSchema.Id;
            }

            return ((ByValueSchema)schema).MarkupValue;
        }
    }
}
