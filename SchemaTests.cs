using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dasher;
using Xunit;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SchemaComparisons
{
    // TODO reflect integral conversions supported by dasher
    // TODO test writing empty message to complex with all-default values

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

            var w = schemaCollection.GetWriteSchema(typeof(TWrite));
            var r = schemaCollection.GetReadSchema(typeof(TRead));

            var actualMatchIfRelaxed = r.CanReadFrom(w, allowWideningConversion: true);
            var actualMatchIfStrict = r.CanReadFrom(w, allowWideningConversion: false);

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

        [Fact]
        public void EmptySchema_ExactMatch()
        {
            var read = Test<EmptyMessage, EmptyMessage>(
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
            var read = Test<Person, EmptyMessage>(
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
            var read = Test<Union<int, string>, EmptyMessage>(
                1,
                null,
                matchIfRelaxed: true,
                matchIfStrict: false);

            foreach (var v in read)
                Assert.Null(v);
        }

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
    }
}