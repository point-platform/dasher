using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Dasher.Schemata.Utils;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dasher.Schemata.Tests
{
    // TODO better default names for schema (consider type name, though careful with generics...)
    // TODO support recursive types
    // TODO reflect integral conversions supported by dasher
    // TODO test writing empty message to complex with all-default values

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

    public class SchemaTests
    {
        /// <summary>Required as Dasher won't serialise non-complex top-level types.</summary>
        public class Wrapper<T>
        {
            public T Value { get; }

            public Wrapper(T value)
            {
                Value = value;
            }
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
        private static IReadOnlyList<TRead> Test<TWrite, TRead>(TWrite write, TRead read, bool matchIfRelaxed, bool matchIfStrict)
        {
            var schemaCollection = new SchemaCollection();

            var w = schemaCollection.GetOrAddWriteSchema(typeof(TWrite));
            var r = schemaCollection.GetOrAddReadSchema(typeof(TRead));

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

        public SchemaTests(ITestOutputHelper output)
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

        #region Empty schema

        [Fact]
        public void EmptySchema_ExactMatch()
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
        public void EmptySchema_Complex()
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
        public void EmptySchema_Union()
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
        public void UnionSchema_ExactMatch()
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
        public void UnionSchema_ExtraMember()
        {
            Test<Union<int, string, double>, Union<int, string>>(
                1,
                1,
                matchIfRelaxed: false,
                matchIfStrict: false);
        }

        [Fact]
        public void UnionSchema_FewerMembers()
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
        public void ListSchema_SameType()
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
        public void ListSchema_CompatibleIfRelaxed()
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
        public void ListSchema_IncompatibleTypes()
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
        public void DictionarySchema_SameType()
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
        public void DictionarySchema_CompatibleIfRelaxed()
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
        public void DictionarySchema_IncompatibleTypes()
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
        public void TupleSchema_ExactMatch()
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
        public void TupleSchema_ExtraMember()
        {
            Test(
                Tuple.Create(1, 2, 3),
                Tuple.Create(1, 2),
                matchIfRelaxed: false,
                matchIfStrict: false);
        }

        [Fact]
        public void TupleSchema_FewerMembers()
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
        public void NullableSchema_NonNullableToNullable()
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
        public void NullableSchema_ExactMatch()
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
        public void NullableSchema_IncompatibleTypes()
        {
            Test(
                (double?)1,
                (int?)1,
                matchIfRelaxed: false,
                matchIfStrict: false);
        }

        [Fact]
        public void NullableSchema_NullableToNonNullable()
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
        public void SchemaCollection_ConsolidatesSchemata()
        {
            var schemaCollection = new SchemaCollection();

            var s1 = schemaCollection.GetOrAddReadSchema(typeof(Person));
            var s2 = schemaCollection.GetOrAddReadSchema(typeof(Person));

            Assert.Same(s1, s2);

            var s3 = schemaCollection.GetOrAddReadSchema(typeof(Wrapper<Person>));

            Assert.Same(s2, ((Schema)s3).Children.Single());
        }

        #endregion

        #region SchemaCollection XML Round Trip

        [Fact]
        public void SchemaCollection_XmlRoundTrip()
        {
            var before = new SchemaCollection();

            before.GetOrAddReadSchema(typeof(Person));
            before.GetOrAddWriteSchema(typeof(Person));
            before.GetOrAddReadSchema(typeof(Wrapper<Person>));
            before.GetOrAddReadSchema(typeof(EnumAbc));
            before.GetOrAddWriteSchema(typeof(EnumAbc));
            before.GetOrAddReadSchema(typeof(Wrapper<EnumAbc>));
            before.GetOrAddReadSchema(typeof(Union<int, string, Person, EnumAbcd>));
            before.GetOrAddWriteSchema(typeof(Union<int, string, Person, EnumAbcd>));

            Assert.Equal(8, before.Schema.OfType<ByRefSchema>().Count());

            before.UpdateByRefIds();

            var xml = before.ToXml();

            const string expectedXml = @"<Schema>
  <ComplexRead Id=""Schema0"">
    <Field Name=""age"" Schema=""Int32"" IsRequired=""true"" />
    <Field Name=""name"" Schema=""String"" IsRequired=""true"" />
  </ComplexRead>
  <ComplexWrite Id=""Schema1"">
    <Field Name=""Age"" Schema=""Int32"" />
    <Field Name=""Name"" Schema=""String"" />
  </ComplexWrite>
  <ComplexRead Id=""Schema2"">
    <Field Name=""value"" Schema=""#Schema0"" IsRequired=""true"" />
  </ComplexRead>
  <Enum Id=""Schema3"">
    <A />
    <B />
    <C />
  </Enum>
  <ComplexRead Id=""Schema4"">
    <Field Name=""value"" Schema=""#Schema3"" IsRequired=""true"" />
  </ComplexRead>
  <Enum Id=""Schema5"">
    <A />
    <B />
    <C />
    <D />
  </Enum>
  <UnionRead Id=""Schema6"">
    <Member Id=""Dasher.Schemata.Tests.EnumAbcd"" Schema=""#Schema5"" />
    <Member Id=""Dasher.Schemata.Tests.Person"" Schema=""#Schema0"" />
    <Member Id=""Int32"" Schema=""Int32"" />
    <Member Id=""String"" Schema=""String"" />
  </UnionRead>
  <UnionWrite Id=""Schema7"">
    <Member Id=""Dasher.Schemata.Tests.EnumAbcd"" Schema=""#Schema5"" />
    <Member Id=""Dasher.Schemata.Tests.Person"" Schema=""#Schema1"" />
    <Member Id=""Int32"" Schema=""Int32"" />
    <Member Id=""String"" Schema=""String"" />
  </UnionWrite>
</Schema>";

            var actualXml = xml.ToString();

            _output.WriteLine(actualXml);

            Assert.Equal(expectedXml, actualXml);

            Assert.Equal(8, xml.Elements().Count());

            var after = SchemaCollection.FromXml(xml);

            Assert.True(new HashSet<Schema>(before.Schema).SetEquals(new HashSet<Schema>(after.Schema)));

            Assert.Equal(before.Schema.Count, after.Schema.Count);

            foreach (var b in before.Schema)
                Assert.Equal(1, after.Schema.Count(s => s.Equals(b)));
            foreach (var a in after.Schema)
                Assert.Equal(1, before.Schema.Count(s => s.Equals(a)));
        }

        [Fact]
        public void SchemaMarkupExtensionTokenizer()
        {
            Assert.Equal(new[]{"empty"}, SchemaMarkupExtension.Tokenize("{empty}"));
            Assert.Equal(new[]{"A", "B", "C"}, SchemaMarkupExtension.Tokenize("{A B C}"));
            Assert.Equal(new[]{"A", "{B C}"}, SchemaMarkupExtension.Tokenize("{A {B C}}"));

            Assert.Throws<SchemaParseException>(() => SchemaMarkupExtension.Tokenize("}").ToList());
            Assert.Throws<SchemaParseException>(() => SchemaMarkupExtension.Tokenize("{}}").ToList());
            Assert.Throws<SchemaParseException>(() => SchemaMarkupExtension.Tokenize("{").ToList());
            Assert.Throws<SchemaParseException>(() => SchemaMarkupExtension.Tokenize("{{").ToList());
            Assert.Throws<SchemaParseException>(() => SchemaMarkupExtension.Tokenize("{a} ").ToList());
            Assert.Throws<SchemaParseException>(() => SchemaMarkupExtension.Tokenize("{a}b").ToList());
            Assert.Throws<SchemaParseException>(() => SchemaMarkupExtension.Tokenize(" {a}").ToList());
            Assert.Throws<SchemaParseException>(() => SchemaMarkupExtension.Tokenize("b{a}").ToList());
        }

        #endregion

        [Fact]
        public void SchemaEquality()
        {
            Action<Type> test = type =>
            {
                var c1 = new SchemaCollection();
                var c2 = new SchemaCollection();
                var r1 = c1.GetOrAddReadSchema(type);
                var r2 = c2.GetOrAddReadSchema(type);
                var w1 = c1.GetOrAddWriteSchema(type);
                var w2 = c2.GetOrAddWriteSchema(type);

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

            var c = new SchemaCollection();

            // Intern
            Assert.Same(c.GetOrAddReadSchema(typeof(int)), c.GetOrAddReadSchema(typeof(int)));

            // Read and write same for some types
            Assert.Same(c.GetOrAddReadSchema(typeof(int)), c.GetOrAddWriteSchema(typeof(int)));
            Assert.Same(c.GetOrAddReadSchema(typeof(double)), c.GetOrAddWriteSchema(typeof(double)));
            Assert.Same(c.GetOrAddReadSchema(typeof(EnumAbc)), c.GetOrAddWriteSchema(typeof(EnumAbc)));
            Assert.Same(c.GetOrAddReadSchema(typeof(int)), c.GetOrAddWriteSchema(typeof(int)));
            Assert.Same(c.GetOrAddReadSchema(typeof(Empty)), c.GetOrAddWriteSchema(typeof(Empty)));

            // NOTE for some types, read and write schema are equal (can this cause trouble?)
            Assert.Equal((object)c.GetOrAddReadSchema(typeof(int)), c.GetOrAddWriteSchema(typeof(int)));
            Assert.Equal((object)c.GetOrAddReadSchema(typeof(double)), c.GetOrAddWriteSchema(typeof(double)));
            Assert.Equal((object)c.GetOrAddReadSchema(typeof(EnumAbc)), c.GetOrAddWriteSchema(typeof(EnumAbc)));
            Assert.Equal((object)c.GetOrAddReadSchema(typeof(int)), c.GetOrAddWriteSchema(typeof(int)));
            Assert.Equal((object)c.GetOrAddReadSchema(typeof(Empty)), c.GetOrAddWriteSchema(typeof(Empty)));

            var schemata = new object[]
            {
                c.GetOrAddReadSchema(typeof(int)),
                c.GetOrAddReadSchema(typeof(EnumAbc)),
                c.GetOrAddReadSchema(typeof(EnumAbcd)),
                c.GetOrAddReadSchema(typeof(double)),

                c.GetOrAddReadSchema(typeof(Person)),
                c.GetOrAddWriteSchema(typeof(Person)),
                c.GetOrAddReadSchema(typeof(Wrapper<double>)),
                c.GetOrAddWriteSchema(typeof(Wrapper<double>))
            };

            foreach (var schema in schemata)
            {
                Assert.Equal(1, schemata.Count(s => s.Equals(schema)));
                Assert.Equal(1, schemata.Count(s => s.GetHashCode().Equals(schema.GetHashCode())));
            }
        }
    }
}