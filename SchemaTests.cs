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
        public PersonWithDefaultScore(string name, int age, double score = 0)
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

    // TODO tests should actually serialise/deserialise objects and verify schema claim matches reality

    public class SchemaTests
    {
        private readonly SchemaCollection _schemaCollection = new SchemaCollection();

        private void Test<TWrite, TRead>(TWrite write, TRead read, bool matchIfRelaxed, bool matchIfStrict)
        {
            var w = _schemaCollection.GetWriteSchema(typeof(TWrite));
            var r = _schemaCollection.GetReadSchema(typeof(TRead));

            Assert.Equal(matchIfRelaxed, r.CanReadFrom(w, allowWideningConversion: true));
            Assert.Equal(matchIfStrict,  r.CanReadFrom(w, allowWideningConversion: false));
        }

        [Fact]
        public void ComplexTypes_FieldsMatch()
        {
            Test(
                new Person("Bob", 36),
                new Person("Bob", 36),
                matchIfRelaxed: true,
                matchIfStrict: true);
        }

        [Fact]
        public void ComplexTypes_ExtraField()
        {
            Test(
                new PersonWithScore("Bob", 36, 100.0),
                new Person("Bob", 36),
                matchIfRelaxed: true,
                matchIfStrict: false);
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
            Test(
                new Person("Bob", 36),
                new PersonWithDefaultScore("Bob", 36),
                matchIfRelaxed: true,
                matchIfStrict: true);
        }

        [Fact]
        public void ComplexTypes_MissingNonRequiredField_InLexicographicalMiddle()
        {
            Test(
                new Person("Bob", 36),
                new PersonWithDefaultHeight("Bob", 36),
                matchIfRelaxed: true,
                matchIfStrict: true);
        }

        [Fact]
        public void Enum_MembersMatch()
        {
            Test(
                EnumAbc.A,
                EnumAbc.A,
                matchIfRelaxed: true,
                matchIfStrict: true);
        }

        [Fact]
        public void Enum_ExtraMember()
        {
            Test(
                EnumAbc.A,
                EnumAbcd.A,
                matchIfRelaxed: true,
                matchIfStrict: false);
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
            Test<EmptyMessage,EmptyMessage>(
                null,
                null,
                matchIfRelaxed: true,
                matchIfStrict: true);
        }

        [Fact]
        public void EmptySchema_Complex()
        {
            Test<Person, EmptyMessage>(
                new Person("Bob", 36),
                null,
                matchIfRelaxed: true,
                matchIfStrict: false);
        }

        [Fact]
        public void EmptySchema_Union()
        {
            Test<Union<int, string>, EmptyMessage>(
                1,
                null,
                matchIfRelaxed: true,
                matchIfStrict: false);
        }

        [Fact]
        public void UnionSchema_ExactMatch()
        {
            Test<Union<int, string>, Union<int, string>>(
                1,
                1,
                matchIfRelaxed: true,
                matchIfStrict: true);
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
            Test<Union<int, string>, Union<int, string, double>>(
                1,
                1,
                matchIfRelaxed: true,
                matchIfStrict: false);
        }
    }
}