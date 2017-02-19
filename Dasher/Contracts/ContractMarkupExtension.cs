#region License
//
// Dasher
//
// Copyright 2015-2017 Drew Noakes
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
// More information about this project is available at:
//
//    https://github.com/drewnoakes/dasher
//
#endregion

using System.Collections.Generic;

namespace Dasher.Contracts
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