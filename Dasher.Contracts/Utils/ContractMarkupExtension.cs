using System.Collections.Generic;

namespace Dasher.Contracts.Utils
{
    internal static class ContractMarkupExtension
    {
        public static IEnumerable<string> Tokenize(string s)
        {
            if (s.Length == 0)
                yield break;
            if (s[0] != '{')
                throw new ContractParseException("Contract markup extension must start with '{'.");

            var depth = 0;
            var tokenStart = 0;

            for (var i = 0; i < s.Length; i++)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (s[i])
                {
                    case '{':
                        depth++;
                        if (depth == 1)
                            tokenStart = i + 1;
                        break;
                    case '}':
                        if (depth == 1)
                        {
                            if (i != tokenStart)
                                yield return s.Substring(tokenStart, i - tokenStart);
                            tokenStart = i + 1;
                        }
                        depth--;
                        if (depth < 0)
                            throw new ContractParseException($"Invalid contract markup extension \"{s}\".");
                        break;
                    case ' ':
                        if (depth == 0)
                            throw new ContractParseException($"Invalid contract markup extension \"{s}\".");
                        if (depth == 1)
                        {
                            if (i != tokenStart)
                                yield return s.Substring(tokenStart, i - tokenStart);
                            tokenStart = i + 1;
                        }
                        break;
                    default:
                        if (depth == 0)
                            throw new ContractParseException($"Invalid contract markup extension \"{s}\".");
                        break;
                }
            }

            if (depth != 0)
                throw new ContractParseException($"Invalid contract markup extension \"{s}\".");
        }
    }
}