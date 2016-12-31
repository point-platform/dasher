#region License
//
// Dasher
//
// Copyright 2015-2016 Drew Noakes
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Dasher.Contracts;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dasher.Tests
{
    // TODO better default IDs for by-ref contracts (consider type name, though careful with generics...)
    // TODO support recursive types
    // TODO reflect integral conversions supported by dasher
    // TODO test writing empty message to complex with all-default values
    // TODO divorce to/from XML from class hierarchy, allowing other serialisation formats
    // TODO consider integrating contract types with ITypeProvider

/*
    [Flags]
    internal enum CompatabilityLevel
    {
        Strict,
        AllowExtraFieldsOnComplex,
        AllowFewerMembersInEnum,
        AllowFewerMembersInUnion,
        AllowWideningIntegralTypes,
        AllowWideningFloatingPointTypes,
        AllowMakingNullable,
        Lenient // = AllowExtraFieldsOnComplex | AllowFewerMembersInEnum | AllowFewerMembersInUnion | AllowLosslessTypeConversion
    }
*/

    #region Test types

    public enum EnumAbc { A, B, C }
    public enum EnumAbcd { A, B, C, D }

    public class Person
    {
        public Person(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public string Name { get; }
        public int Age { get; }
    }

    public class PersonWithScore
    {
        public PersonWithScore(string name, int age, double score)
        {
            Name = name;
            Age = age;
            Score = score;
        }

        public string Name { get; }
        public int Age { get; }
        public double Score { get; }
    }

    public class PersonWithDefaultScore
    {
        public PersonWithDefaultScore(string name, int age, double score = 100.0)
        {
            Name = name;
            Age = age;
            Score = score;
        }

        public string Name { get; }
        public int Age { get; }
        public double Score { get; }
    }

    public class PersonWithDefaultHeight
    {
        public PersonWithDefaultHeight(string name, int age, double height = double.NaN)
        {
            Name = name;
            Age = age;
            Height = height;
        }

        public string Name { get; }
        public int Age { get; }
        public double Height { get; }
    }
    /// <summary>Required as Dasher won't serialise non-complex top-level types.</summary>
    public class Wrapper<T>
    {
        public T Value { get; }

        public Wrapper(T value)
        {
            Value = value;
        }
    }

    #endregion

    public sealed class ContractTests
    {
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
        private static IReadOnlyList<TRead> Test<TWrite, TRead>(TWrite write, TRead read, bool matchIfRelaxed, bool matchIfStrict)
        {
            var contractCollection = new ContractCollection();

            var w = contractCollection.GetOrAddWriteContract(typeof(TWrite));
            var r = contractCollection.GetOrAddReadContract(typeof(TRead));

            var actualMatchIfRelaxed = r.CanReadFrom(w, strict: false);
            var actualMatchIfStrict = r.CanReadFrom(w, strict: true);

            Assert.Equal(matchIfRelaxed, actualMatchIfRelaxed);
            Assert.Equal(matchIfStrict,  actualMatchIfStrict);

            if (!actualMatchIfRelaxed && !actualMatchIfStrict)
                return new TRead[0];

            var stream = new MemoryStream();

            new Serialiser<Wrapper<TWrite>>().Serialise(stream, new Wrapper<TWrite>(write));

            var values = new List<TRead>();

            if (actualMatchIfRelaxed)
            {
                stream.Position = 0;
                values.Add(new Deserialiser<Wrapper<TRead>>(UnexpectedFieldBehaviour.Ignore).Deserialise(stream).Value);
            }

            if (actualMatchIfStrict)
            {
                stream.Position = 0;
                values.Add(new Deserialiser<Wrapper<TRead>>(UnexpectedFieldBehaviour.Throw).Deserialise(stream).Value);
            }

            return values;
        }

        private readonly ITestOutputHelper _output;

        public ContractTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region Complex Types

        [Fact]
        public void ComplexTypes_FieldsMatch()
        {
            var read = Test(
                new Person("Bob", 36),
                new Person("Bob", 36),
                matchIfRelaxed: true,
                matchIfStrict: true);

            foreach (var person in read)
            {
                Assert.Equal("Bob", person.Name);
                Assert.Equal(36, person.Age);
            }
        }

        [Fact]
        public void ComplexTypes_ExtraField()
        {
            var read = Test(
                new PersonWithScore("Bob", 36, 100.0),
                new Person("Bob", 36),
                matchIfRelaxed: true,
                matchIfStrict: false);

            foreach (var person in read)
            {
                Assert.Equal("Bob", person.Name);
                Assert.Equal(36, person.Age);
            }
        }

        [Fact]
        public void ComplexTypes_InsufficientFields()
        {
            Test(
                new Person("Bob", 36),
                new PersonWithScore("Bob", 36, 100.0),
                matchIfRelaxed: false,
                matchIfStrict: false);
        }

        [Fact]
        public void ComplexTypes_MissingNonRequiredField_AtLexicographicalEnd()
        {
            var read = Test(
                new Person("Bob", 36),
                new PersonWithDefaultScore("Bob", 36),
                matchIfRelaxed: true,
                matchIfStrict: true);

            foreach (var person in read)
            {
                Assert.Equal("Bob", person.Name);
                Assert.Equal(36, person.Age);
                Assert.Equal(100.0, person.Score);
            }
        }

        [Fact]
        public void ComplexTypes_MissingNonRequiredField_InLexicographicalMiddle()
        {
            var read = Test(
                new Person("Bob", 36),
                new PersonWithDefaultHeight("Bob", 36),
                matchIfRelaxed: true,
                matchIfStrict: true);

            foreach (var person in read)
            {
                Assert.Equal("Bob", person.Name);
                Assert.Equal(36, person.Age);
                Assert.Equal(double.NaN, person.Height);
            }
        }

            #endregion

        #region Enums

        [Fact]
        public void Enum_MembersMatch()
        {
            var read = Test(
                EnumAbc.A,
                EnumAbc.A,
                matchIfRelaxed: true,
                matchIfStrict: true);

            foreach (var e in read)
                Assert.Equal(EnumAbc.A, e);
        }

        [Fact]
        public void Enum_ExtraMember()
        {
            var read = Test(
                EnumAbc.A,
                EnumAbcd.A,
                matchIfRelaxed: true,
                matchIfStrict: false);

            foreach (var e in read)
                Assert.Equal(EnumAbcd.A, e);
        }

        [Fact]
        public void Enum_InsufficientMembers()
        {
            Test(
                EnumAbcd.A,
                EnumAbc.A,
                matchIfRelaxed: false,
                matchIfStrict: false);
        }

        #endregion

        #region Empty contract

        [Fact]
        public void EmptyContract_ExactMatch()
        {
            var read = Test<Empty, Empty>(
                null,
                null,
                matchIfRelaxed: true,
                matchIfStrict: true);

            foreach (var v in read)
                Assert.Null(v);
        }

        [Fact]
        public void EmptyContract_Complex()
        {
            var read = Test<Person, Empty>(
                new Person("Bob", 36),
                null,
                matchIfRelaxed: true,
                matchIfStrict: false);

            foreach (var v in read)
                Assert.Null(v);
        }

        [Fact]
        public void EmptyContract_Union()
        {
            var read = Test<Union<int, string>, Empty>(
                1,
                null,
                matchIfRelaxed: true,
                matchIfStrict: false);

            foreach (var v in read)
                Assert.Null(v);
        }

        #endregion

        #region Unions

        [Fact]
        public void UnionContract_ExactMatch()
        {
            var read = Test<Union<int, string>, Union<int, string>>(
                1,
                1,
                matchIfRelaxed: true,
                matchIfStrict: true);

            foreach (var v in read)
                Assert.Equal(1, v);
        }

        [Fact]
        public void UnionContract_ExtraMember()
        {
            Test<Union<int, string, double>, Union<int, string>>(
                1,
                1,
                matchIfRelaxed: false,
                matchIfStrict: false);
        }

        [Fact]
        public void UnionContract_FewerMembers()
        {
            var read = Test<Union<int, string>, Union<int, string, double>>(
                1,
                1,
                matchIfRelaxed: true,
                matchIfStrict: false);

            foreach (var v in read)
                Assert.Equal(1, v);
        }

        #endregion

        #region Lists

        [Fact]
        public void ListContract_SameType()
        {
            var read = Test<IReadOnlyList<int>, IReadOnlyList<int>>(
                new[] {1, 2, 3},
                new[] {1, 2, 3},
                matchIfRelaxed: true,
                matchIfStrict: true);

            foreach (var list in read)
                Assert.Equal(new[] {1, 2, 3}, list);
        }

        [Fact]
        public void ListContract_CompatibleIfRelaxed()
        {
            var read = Test<IReadOnlyList<PersonWithScore>, IReadOnlyList<Person>>(
                new[] {new PersonWithScore("Bob", 36, 100.0) },
                new[] {new Person("Bob", 36) },
                matchIfRelaxed: true,
                matchIfStrict: false);

            foreach (var list in read)
            {
                foreach (var person in list)
                {
                    Assert.Equal("Bob", person.Name);
                    Assert.Equal(36, person.Age);
                }
            }
        }

        [Fact]
        public void ListContract_IncompatibleTypes()
        {
            Test<IReadOnlyList<int>, IReadOnlyList<string>>(
                new[] {1, 2, 3},
                new[] {"1", "2", "3"},
                matchIfRelaxed: false,
                matchIfStrict: false);
        }

        #endregion

        #region Dictionaries

        [Fact]
        public void DictionaryContract_SameType()
        {
            var read = Test<IReadOnlyDictionary<int, int>, IReadOnlyDictionary<int, int>>(
                new Dictionary<int, int> {{1, 1}, {2, 2}},
                new Dictionary<int, int> {{1, 1}, {2, 2}},
                matchIfRelaxed: true,
                matchIfStrict: true);

            foreach (var dic in read)
            {
                Assert.Equal(2, dic.Count);
                Assert.Equal(1, dic[1]);
                Assert.Equal(2, dic[2]);
            }
        }

        [Fact]
        public void DictionaryContract_CompatibleIfRelaxed()
        {
            var read = Test<IReadOnlyDictionary<int, PersonWithScore>, IReadOnlyDictionary<int, Person>>(
                new Dictionary<int, PersonWithScore> {{1, new PersonWithScore("Bob", 36, 100.0) } },
                new Dictionary<int, Person> {{1, new Person("Bob", 36) } },
                matchIfRelaxed: true,
                matchIfStrict: false);

            foreach (var dic in read)
            {
                Assert.Equal(1, dic.Count);
                Assert.Equal("Bob", dic[1].Name);
                Assert.Equal(36, dic[1].Age);
            }
        }

        [Fact]
        public void DictionaryContract_IncompatibleTypes()
        {
            Test<IReadOnlyDictionary<int, int>, IReadOnlyDictionary<string, int>>(
                new Dictionary<int, int> {{1, 1}},
                new Dictionary<string, int> {{"1", 1}},
                matchIfRelaxed: false,
                matchIfStrict: false);

            Test<IReadOnlyDictionary<int, int>, IReadOnlyDictionary<int, string>>(
                new Dictionary<int, int> {{1, 1}},
                new Dictionary<int, string> {{1, "1"}},
                matchIfRelaxed: false,
                matchIfStrict: false);
        }

        #endregion

        #region Tuples

        [Fact]
        public void TupleContract_ExactMatch()
        {
            var read = Test(
                Tuple.Create(1, 2),
                Tuple.Create(1, 2),
                matchIfRelaxed: true,
                matchIfStrict: true);

            foreach (var v in read)
                Assert.Equal(Tuple.Create(1, 2), v);
        }

        [Fact]
        public void TupleContract_ExtraMember()
        {
            Test(
                Tuple.Create(1, 2, 3),
                Tuple.Create(1, 2),
                matchIfRelaxed: false,
                matchIfStrict: false);
        }

        [Fact]
        public void TupleContract_FewerMembers()
        {
            Test(
                Tuple.Create(1, 2),
                Tuple.Create(1, 2, 3),
                matchIfRelaxed: false,
                matchIfStrict: false);
        }

        #endregion

        #region Nullables

        [Fact]
        public void NullableContract_NonNullableToNullable()
        {
            var read = Test(
                1,
                (int?)1,
                matchIfRelaxed: true,
                matchIfStrict: false);

            foreach (var i in read)
                Assert.Equal(1, i);
        }

        [Fact]
        public void NullableContract_ExactMatch()
        {
            var read = Test(
                (int?)1,
                (int?)1,
                matchIfRelaxed: true,
                matchIfStrict: true);

            foreach (var i in read)
                Assert.Equal(1, i);
        }

        [Fact]
        public void NullableContract_IncompatibleTypes()
        {
            Test(
                (double?)1,
                (int?)1,
                matchIfRelaxed: false,
                matchIfStrict: false);
        }

        [Fact]
        public void NullableContract_NullableToNonNullable()
        {
            Test(
                (int?)1,
                1,
                matchIfRelaxed: false,
                matchIfStrict: false);
        }

        #endregion

        #region Consolidation

        [Fact]
        public void ContractCollection_ConsolidatesContracts()
        {
            var contractCollection = new ContractCollection();

            var s1 = contractCollection.GetOrAddReadContract(typeof(Person));
            var s2 = contractCollection.GetOrAddReadContract(typeof(Person));

            Assert.Same(s1, s2);

            var s3 = contractCollection.GetOrAddReadContract(typeof(Wrapper<Person>));

            Assert.Same(s2, ((Contract)s3).Children.Single());
        }

        #endregion

        #region ContractCollection XML Round Trip

        [Fact]
        public void ContractCollection_XmlRoundTrip()
        {
            var before = new ContractCollection();

            before.GetOrAddReadContract(typeof(Person));
            before.GetOrAddWriteContract(typeof(Person));
            before.GetOrAddReadContract(typeof(Wrapper<Person>));
            before.GetOrAddReadContract(typeof(EnumAbc));
            before.GetOrAddWriteContract(typeof(EnumAbc));
            before.GetOrAddReadContract(typeof(Wrapper<EnumAbc>));
            before.GetOrAddReadContract(typeof(Union<int, string, Person, EnumAbcd>));
            before.GetOrAddWriteContract(typeof(Union<int, string, Person, EnumAbcd>));

            Assert.Equal(8, before.Contracts.OfType<ByRefContract>().Count());

            before.UpdateByRefIds();

            var xml = before.ToXml();

            const string expectedXml = @"<Contracts>
  <ComplexRead Id=""Contract1"">
    <Field Name=""age"" Contract=""Int32"" IsRequired=""true"" />
    <Field Name=""name"" Contract=""String"" IsRequired=""true"" />
  </ComplexRead>
  <ComplexWrite Id=""Contract2"">
    <Field Name=""Age"" Contract=""Int32"" />
    <Field Name=""Name"" Contract=""String"" />
  </ComplexWrite>
  <ComplexRead Id=""Contract3"">
    <Field Name=""value"" Contract=""#Contract1"" IsRequired=""true"" />
  </ComplexRead>
  <Enum Id=""Contract4"">
    <Member Name=""A"" />
    <Member Name=""B"" />
    <Member Name=""C"" />
  </Enum>
  <ComplexRead Id=""Contract5"">
    <Field Name=""value"" Contract=""#Contract4"" IsRequired=""true"" />
  </ComplexRead>
  <Enum Id=""Contract6"">
    <Member Name=""A"" />
    <Member Name=""B"" />
    <Member Name=""C"" />
    <Member Name=""D"" />
  </Enum>
  <UnionRead Id=""Contract7"">
    <Member Id=""Dasher.Tests.EnumAbcd"" Contract=""#Contract6"" />
    <Member Id=""Dasher.Tests.Person"" Contract=""#Contract1"" />
    <Member Id=""Int32"" Contract=""Int32"" />
    <Member Id=""String"" Contract=""String"" />
  </UnionRead>
  <UnionWrite Id=""Contract8"">
    <Member Id=""Dasher.Tests.EnumAbcd"" Contract=""#Contract6"" />
    <Member Id=""Dasher.Tests.Person"" Contract=""#Contract2"" />
    <Member Id=""Int32"" Contract=""Int32"" />
    <Member Id=""String"" Contract=""String"" />
  </UnionWrite>
</Contracts>";

            var actualXml = xml.ToString();

            _output.WriteLine(actualXml);

            Assert.Equal(expectedXml, actualXml, new SelectiveStringComparer());

            Assert.Equal(8, xml.Elements().Count());

            var after = ContractCollection.FromXml(xml);

            Assert.True(new HashSet<Contract>(before.Contracts).SetEquals(new HashSet<Contract>(after.Contracts)));

            Assert.Equal(before.Contracts.Count, after.Contracts.Count);

            foreach (var b in before.Contracts)
                Assert.Equal(1, after.Contracts.Count(s => s.Equals(b)));
            foreach (var a in after.Contracts)
                Assert.Equal(1, before.Contracts.Count(s => s.Equals(a)));
        }

        [Fact]
        public void ContractMarkupExtensionTokenizer()
        {
            Assert.Equal(new[]{"empty"}, ContractMarkupExtension.Tokenize("{empty}"));
            Assert.Equal(new[]{"A", "B", "C"}, ContractMarkupExtension.Tokenize("{A B C}"));
            Assert.Equal(new[]{"A", "{B C}"}, ContractMarkupExtension.Tokenize("{A {B C}}"));

            Assert.Throws<ContractParseException>(() => ContractMarkupExtension.Tokenize("}").ToList());
            Assert.Throws<ContractParseException>(() => ContractMarkupExtension.Tokenize("{}}").ToList());
            Assert.Throws<ContractParseException>(() => ContractMarkupExtension.Tokenize("{").ToList());
            Assert.Throws<ContractParseException>(() => ContractMarkupExtension.Tokenize("{{").ToList());
            Assert.Throws<ContractParseException>(() => ContractMarkupExtension.Tokenize("{a} ").ToList());
            Assert.Throws<ContractParseException>(() => ContractMarkupExtension.Tokenize("{a}b").ToList());
            Assert.Throws<ContractParseException>(() => ContractMarkupExtension.Tokenize(" {a}").ToList());
            Assert.Throws<ContractParseException>(() => ContractMarkupExtension.Tokenize("b{a}").ToList());
        }

        #endregion

        #region ContractCollection GarbageCollect

        [Fact]
        public void ContractCollectionGarbageCollects()
        {
            var contractCollection = new ContractCollection();

            var s1 = (Contract)contractCollection.GetOrAddReadContract(typeof(Person));
            var s2 = (Contract)contractCollection.GetOrAddReadContract(typeof(Wrapper<Person>));

            Assert.Equal(4, contractCollection.Contracts.Count);
            Assert.True(contractCollection.Contracts.Contains(s1));
            Assert.True(contractCollection.Contracts.Contains(s2));

            contractCollection.GarbageCollect(new[] { s2 });

            Assert.Equal(4, contractCollection.Contracts.Count);
            Assert.True(contractCollection.Contracts.Contains(s1));
            Assert.True(contractCollection.Contracts.Contains(s2));

            contractCollection.GarbageCollect(new[] { s1 });

            Assert.Equal(3, contractCollection.Contracts.Count);
            Assert.True(contractCollection.Contracts.Contains(s1));
            Assert.False(contractCollection.Contracts.Contains(s2));

            contractCollection.GarbageCollect(new Contract[0]);

            Assert.Equal(0, contractCollection.Contracts.Count);
            Assert.False(contractCollection.Contracts.Contains(s1));
            Assert.False(contractCollection.Contracts.Contains(s2));
        }

        #endregion

        [Fact]
        public void ContractEquality()
        {
            Action<Type> test = type =>
            {
                var c1 = new ContractCollection();
                var c2 = new ContractCollection();
                var r1 = c1.GetOrAddReadContract(type);
                var r2 = c2.GetOrAddReadContract(type);
                var w1 = c1.GetOrAddWriteContract(type);
                var w2 = c2.GetOrAddWriteContract(type);

                Assert.Equal(r1, r2);
                Assert.Equal(r2, r1);
                Assert.Equal(r1.GetHashCode(), r2.GetHashCode());

                Assert.Equal(w1, w2);
                Assert.Equal(w2, w1);
                Assert.Equal(w1.GetHashCode(), w2.GetHashCode());
            };

            test(typeof(Person));
            test(typeof(Wrapper<Person>));
            test(typeof(EnumAbc));
            test(typeof(int?));
            test(typeof(IReadOnlyList<EnumAbc>));
            test(typeof(IReadOnlyDictionary<EnumAbc, int?>));
            test(typeof(Tuple<int, long, double>));
            test(typeof(Union<int, long, double>));
            test(typeof(int));
            test(typeof(long));

            //////

            var c = new ContractCollection();

            // Intern
            Assert.Same(c.GetOrAddReadContract(typeof(int)), c.GetOrAddReadContract(typeof(int)));

            // Read and write same for some types
            Assert.Same(c.GetOrAddReadContract(typeof(int)), c.GetOrAddWriteContract(typeof(int)));
            Assert.Same(c.GetOrAddReadContract(typeof(double)), c.GetOrAddWriteContract(typeof(double)));
            Assert.Same(c.GetOrAddReadContract(typeof(EnumAbc)), c.GetOrAddWriteContract(typeof(EnumAbc)));
            Assert.Same(c.GetOrAddReadContract(typeof(int)), c.GetOrAddWriteContract(typeof(int)));
            Assert.Same(c.GetOrAddReadContract(typeof(Empty)), c.GetOrAddWriteContract(typeof(Empty)));

            // NOTE for some types, read and write contract are equal (can this cause trouble?)
            Assert.Equal((object)c.GetOrAddReadContract(typeof(int)), c.GetOrAddWriteContract(typeof(int)));
            Assert.Equal((object)c.GetOrAddReadContract(typeof(double)), c.GetOrAddWriteContract(typeof(double)));
            Assert.Equal((object)c.GetOrAddReadContract(typeof(EnumAbc)), c.GetOrAddWriteContract(typeof(EnumAbc)));
            Assert.Equal((object)c.GetOrAddReadContract(typeof(int)), c.GetOrAddWriteContract(typeof(int)));
            Assert.Equal((object)c.GetOrAddReadContract(typeof(Empty)), c.GetOrAddWriteContract(typeof(Empty)));

            var contracts = new object[]
            {
                c.GetOrAddReadContract(typeof(int)),
                c.GetOrAddReadContract(typeof(EnumAbc)),
                c.GetOrAddReadContract(typeof(EnumAbcd)),
                c.GetOrAddReadContract(typeof(double)),

                c.GetOrAddReadContract(typeof(Person)),
                c.GetOrAddWriteContract(typeof(Person)),
                c.GetOrAddReadContract(typeof(Wrapper<double>)),
                c.GetOrAddWriteContract(typeof(Wrapper<double>))
            };

            foreach (var contract in contracts)
            {
                Assert.Equal(1, contracts.Count(s => s.Equals(contract)));
                Assert.Equal(1, contracts.Count(s => s.GetHashCode().Equals(contract.GetHashCode())));
            }
        }
    }
}
