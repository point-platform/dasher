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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Dasher.Tests
{
    public sealed class TypeConversionMatrices
    {
        private enum ConversionResult
        {
            Yes,
            Maybe,
            No
        }

        private readonly Dictionary<ConversionResult, string> _emojiByResult = new Dictionary<ConversionResult, string>
        {
            {ConversionResult.Yes, ":full_moon:"},
            {ConversionResult.No, ":new_moon:"},
            {ConversionResult.Maybe, ":waning_crescent_moon:"}
        };

        private readonly ITestOutputHelper _output;

        public TypeConversionMatrices(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(Skip = "Doesn't assert anything")]
        public void LogTypeConversionMatrices()
        {
            var valueArrays = new Array[]
            {
                new[] {byte.MinValue, byte.MaxValue, default(byte)},
                new[] {sbyte.MinValue, sbyte.MaxValue, default(sbyte)},
                new[] {short.MinValue, short.MaxValue, default(short)},
                new[] {ushort.MinValue, ushort.MaxValue, default(ushort)},
                new[] {int.MinValue, int.MaxValue, default(int)},
                new[] {uint.MinValue, uint.MaxValue, default(uint)},
                new[] {long.MinValue, long.MaxValue, default(long)},
                new[] {ulong.MinValue, ulong.MaxValue, default(ulong)},
                new[] {float.MinValue, float.MaxValue, default(float), float.NaN, float.PositiveInfinity, float.NegativeInfinity, float.Epsilon},
                new[] {double.MinValue, double.MaxValue, default(double), double.NaN, double.PositiveInfinity, double.NegativeInfinity, double.Epsilon},
                new[] {decimal.MinValue, decimal.MaxValue, default(decimal), decimal.MinusOne, decimal.One, decimal.Zero},
                new[] {true, false},
                new[] {char.MinValue, char.MaxValue, default(char), '0'},
                new[] {"", "123", "12.3", "Hello", null, "True", "false", "NaN", "-100", "0xFACE"},
                new[] {(Empty)null},
                new[] {new byte[0], new byte[] {1, 2, 3}},
                new[] {new ArraySegment<byte>(new byte[0]), new ArraySegment<byte>(new byte[] {1, 2, 3})}
            };

            var dotNetConversions = new Dictionary<Tuple<Type, Type>, ConversionResult>();
            var dasherConversions = new Dictionary<Tuple<Type, Type>, ConversionResult>();

            var types = valueArrays.Select(a => a.GetType().GetElementType()).ToList();
            var wrapperTypes = types.Select(t => typeof(ValueWrapper<>).MakeGenericType(t)).ToList();
            var serialisers = wrapperTypes.Select(t => new Serialiser(t)).ToList();
            var deserialisers = wrapperTypes.Select(t => new Deserialiser(t)).ToList();
            var wrapperCtors = wrapperTypes.Select(t => t.GetTypeInfo().GetConstructors().Single()).ToList();

            ConversionResult BuildResult(bool anyPassed, bool anyFailed)
            {
                if (anyPassed && anyFailed)
                    return ConversionResult.Maybe;
                if (anyPassed)
                    return ConversionResult.Yes;
                if (anyFailed)
                    return ConversionResult.No;
                throw new Exception("No tests ran.");
            }

            var stream = new MemoryStream();

            for (var i = 0; i < valueArrays.Length; i++)
            {
                for (var j = 0; j < valueArrays.Length; j++)
                {
                    if (i == j)
                        continue;

                    var tuple = Tuple.Create(types[i], types[j]);

                    var anyDotNetPassed = false;
                    var anyDotNetFailed = false;

                    foreach (var fromValue in valueArrays[i])
                    {
                        try
                        {
                            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                            Convert.ChangeType(fromValue, types[j]);
                            anyDotNetPassed = true;
                        }
                        catch
                        {
                            anyDotNetFailed = true;
                        }
                    }

                    dotNetConversions.Add(tuple, BuildResult(anyDotNetPassed, anyDotNetFailed));

                    var anyDasherPassed = false;
                    var anyDasherFailed = false;

                    foreach (var fromValue in valueArrays[i])
                    {
                        var fromWrapper = wrapperCtors[i].Invoke(new[] { fromValue });
                        stream.Position = 0;
                        serialisers[i].Serialise(stream, fromWrapper);

                        try
                        {
                            stream.Position = 0;
                            deserialisers[j].Deserialise(stream);
                            anyDasherPassed = true;
                        }
                        catch
                        {
                            anyDasherFailed = true;
                        }
                    }

                    dasherConversions.Add(tuple, BuildResult(anyDasherPassed, anyDasherFailed));
                }
            }

            _output.WriteLine("# Dasher Conversion Support");

            DumpMarkdownMatrix(dasherConversions);

            _output.WriteLine("# .NET Conversion Support");

            DumpMarkdownMatrix(dotNetConversions);
        }

        private void DumpMarkdownMatrix(Dictionary<Tuple<Type, Type>, ConversionResult> dic)
        {
            var sb = new StringBuilder();
            var types = dic.Keys.Select(t => t.Item1).Distinct().ToList();

            sb.AppendLine("| | " + string.Join(" | ", types.Select(t => t.Name)));
            sb.AppendLine("|---|" + string.Join("", types.Select(t => ":---:|")));

            foreach (var fromType in types)
            {
                sb.Append($"|{fromType.Name}|");

                foreach (var toType in types)
                {
                    if (fromType != toType)
                        sb.Append(_emojiByResult[dic[Tuple.Create(fromType, toType)]]);
                    else
                        sb.Append(' ');

                    sb.Append('|');
                }

                sb.AppendLine();
            }

            var markdownTable = sb.ToString();

            _output.WriteLine(markdownTable);
        }
    }
}
