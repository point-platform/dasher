using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace MsgPack.Strict.SchemaGenerator
{
    public class XMLSchemaGenerator
    {
        public static XElement GenerateSchema(Type type)
        {
            var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            if (ctors.Length != 1)
                throw new SchemaGenerationException("Type must have a single public constructor.", type);

            var result = new XElement("Message", new XAttribute("name", type.Name));

            foreach (var ctorArg in type.GetConstructors().Single().GetParameters())
            {
                var ctorArgType = ctorArg.ParameterType;

                var listType = ctorArgType.GetInterfaces().SingleOrDefault(i => i.Name == "IReadOnlyCollection`1" && i.Namespace == "System.Collections.Generic");
                if (ctorArgType.IsEnum || listType != null || ctorArgType.Namespace == "System" || ctorArgType.IsValueType || ctorArgType.IsEnum)
                {
                    var fieldElem = new XElement("Field",
                                new XAttribute("name", ctorArg.Name),
                                new XAttribute("type", ctorArg.ParameterType));
                    if(ctorArg.HasDefaultValue)
                    {
                        fieldElem.Add(new XAttribute("default", ctorArg.DefaultValue == null ? "null" : ctorArg.DefaultValue));
                    }
                    result.Add(fieldElem);
                }
                else
                {
                    var fieldElem = new XElement("Field",
                                new XAttribute("name", ctorArg.Name),
                                new XAttribute("type", ctorArg.ParameterType));
                    if (ctorArg.HasDefaultValue)
                    {
                        fieldElem.Add(new XAttribute("default", ctorArg.DefaultValue == null ? "null" : ctorArg.DefaultValue));
                    }
                    fieldElem.AddFirst(GenerateSchema(ctorArg.ParameterType));
                    result.Add(fieldElem);
                }
            }

            return result;
        }

        private static StringBuilder indent(StringBuilder sb, int indentLevel)
        {
            for (var i = 0; i < indentLevel; ++i)
                sb.Append("    ");
            return sb;
        }
    }
}
