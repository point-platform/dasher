namespace Dasher.Schemata.Utils
{
    internal static class SchemaExtensions
    {
        public static string ToReferenceString(this IWriteSchema schema) => ToReferenceStringInternal(schema);

        public static string ToReferenceString(this IReadSchema schema) => ToReferenceStringInternal(schema);

        private static string ToReferenceStringInternal(object schema)
        {
            var byRefSchema = schema as ByRefSchema;
            return byRefSchema != null
                ? '#' + byRefSchema.Id
                : ((ByValueSchema)schema).MarkupValue;
        }
    }
}