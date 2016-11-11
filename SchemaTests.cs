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

        [Fact]
        public void ComplexTypes_FieldsMatch()
        {
            var w = _schemaCollection.GetWriteSchema(typeof(Person));
            var r = _schemaCollection.GetReadSchema(typeof(Person));

            Assert.True(r.CanReadFrom(w, allowWideningConversion: true));
            Assert.True(r.CanReadFrom(w, allowWideningConversion: false));
        }

        [Fact]
        public void ComplexTypes_ExtraField()
        {
            var w = _schemaCollection.GetWriteSchema(typeof(PersonWithScore));
            var r = _schemaCollection.GetReadSchema(typeof(Person));

            Assert.True(r.CanReadFrom(w, allowWideningConversion: true));
            Assert.False(r.CanReadFrom(w, allowWideningConversion: false));
        }

        [Fact]
        public void ComplexTypes_InsufficientFields()
        {
            var w = _schemaCollection.GetWriteSchema(typeof(Person));
            var r = _schemaCollection.GetReadSchema(typeof(PersonWithScore));

            Assert.False(r.CanReadFrom(w, allowWideningConversion: true));
            Assert.False(r.CanReadFrom(w, allowWideningConversion: false));
        }

        [Fact]
        public void ComplexTypes_MissingNonRequiredField_AtLexicographicalEnd()
        {
            var w = _schemaCollection.GetWriteSchema(typeof(Person));
            var r = _schemaCollection.GetReadSchema(typeof(PersonWithDefaultScore));

            Assert.True(r.CanReadFrom(w, allowWideningConversion: true));
            Assert.True(r.CanReadFrom(w, allowWideningConversion: false));
        }

        [Fact]
        public void ComplexTypes_MissingNonRequiredField_InLexicographicalMiddle()
        {
            var w = _schemaCollection.GetWriteSchema(typeof(Person));
            var r = _schemaCollection.GetReadSchema(typeof(PersonWithDefaultHeight));

            Assert.True(r.CanReadFrom(w, allowWideningConversion: true));
            Assert.True(r.CanReadFrom(w, allowWideningConversion: false));
        }

        [Fact]
        public void Enum_MembersMatch()
        {
            var w = _schemaCollection.GetWriteSchema(typeof(EnumAbc));
            var r = _schemaCollection.GetReadSchema(typeof(EnumAbc));

            Assert.True(r.CanReadFrom(w, allowWideningConversion: true));
            Assert.True(r.CanReadFrom(w, allowWideningConversion: false));
        }

        [Fact]
        public void Enum_ExtraMember()
        {
            var w = _schemaCollection.GetWriteSchema(typeof(EnumAbc));
            var r = _schemaCollection.GetReadSchema(typeof(EnumAbcd));

            Assert.True(r.CanReadFrom(w, allowWideningConversion: true));
            Assert.False(r.CanReadFrom(w, allowWideningConversion: false));
        }

        [Fact]
        public void Enum_InsufficientMembers()
        {
            var w = _schemaCollection.GetWriteSchema(typeof(EnumAbcd));
            var r = _schemaCollection.GetReadSchema(typeof(EnumAbc));

            Assert.False(r.CanReadFrom(w, allowWideningConversion: true));
            Assert.False(r.CanReadFrom(w, allowWideningConversion: false));
        }

        [Fact]
        public void EmptySchema_ExactMatch()
        {
            var w = _schemaCollection.GetWriteSchema(typeof(EmptyMessage));
            var r = _schemaCollection.GetReadSchema(typeof(EmptyMessage));

            Assert.True(r.CanReadFrom(w, allowWideningConversion: true));
            Assert.True(r.CanReadFrom(w, allowWideningConversion: false));
        }

        [Fact]
        public void EmptySchema_Complex()
        {
            var w = _schemaCollection.GetWriteSchema(typeof(Person));
            var r = _schemaCollection.GetReadSchema(typeof(EmptyMessage));

            Assert.True(r.CanReadFrom(w, allowWideningConversion: true));
            Assert.False(r.CanReadFrom(w, allowWideningConversion: false));
        }

        [Fact]
        public void EmptySchema_Union()
        {
            var w = _schemaCollection.GetWriteSchema(typeof(Union<int, string>));
            var r = _schemaCollection.GetReadSchema(typeof(EmptyMessage));

            Assert.True(r.CanReadFrom(w, allowWideningConversion: true));
            Assert.False(r.CanReadFrom(w, allowWideningConversion: false));
        }

        [Fact]
        public void UnionSchema_ExactMatch()
        {
            var w = _schemaCollection.GetWriteSchema(typeof(Union<int, string>));
            var r = _schemaCollection.GetReadSchema(typeof(Union<int, string>));

            Assert.True(r.CanReadFrom(w, allowWideningConversion: true));
            Assert.True(r.CanReadFrom(w, allowWideningConversion: false));
        }

        [Fact]
        public void UnionSchema_ExtraMember()
        {
            var w = _schemaCollection.GetWriteSchema(typeof(Union<int, string, double>));
            var r = _schemaCollection.GetReadSchema(typeof(Union<int, string>));

            Assert.False(r.CanReadFrom(w, allowWideningConversion: true));
            Assert.False(r.CanReadFrom(w, allowWideningConversion: false));
        }

        [Fact]
        public void UnionSchema_FewerMembers()
        {
            var w = _schemaCollection.GetWriteSchema(typeof(Union<int, string>));
            var r = _schemaCollection.GetReadSchema(typeof(Union<int, string, double>));

            Assert.True(r.CanReadFrom(w, allowWideningConversion: true));
            Assert.False(r.CanReadFrom(w, allowWideningConversion: false));
        }
    }
}