using System;
using System.Linq;
using System.Text;

namespace MsgPack.Strict.SchemaGenerator
{
    public class SchemaGenerator
    {
        public static string GenerateSchema(Type type, int indentLevel = 0)
        {
            var result = new StringBuilder();
            result.AppendFormat("{0}", type.Name).AppendLine();
            indent(result, indentLevel).AppendLine("{");
            foreach (var ctorArg in type.GetConstructors().Single().GetParameters())
            {
                var ctorArgType = ctorArg.ParameterType;
                if (ctorArgType.Namespace == "System" || ctorArgType.IsValueType || ctorArgType.IsEnum)
                {
                    indent(result, indentLevel + 1).AppendFormat(
                              ctorArg.HasDefaultValue ? "{0}: {1} = {2}" : "{0}: {1}",
                              ctorArg.Name,
                              ctorArg.ParameterType,
                              ctorArg.DefaultValue == null ? "null" : ctorArg.DefaultValue);
                    result.AppendLine();
                }
                else
                {
                    indent(result, indentLevel + 1).AppendFormat("{0}: ", ctorArg.Name).Append(GenerateSchema(ctorArg.ParameterType, indentLevel + 1));
                    result.AppendLine();
                }
            }
            indent(result, indentLevel).Append("}");

            // Write a newline at the end of the entire thing
            if(indentLevel == 0)
                result.AppendLine();

            return result.ToString();
        }

        private static StringBuilder indent(StringBuilder sb, int indentLevel)
        {
            for (var i = 0; i < indentLevel; ++i)
                sb.Append("    ");
            return sb;
        }
    }
}
