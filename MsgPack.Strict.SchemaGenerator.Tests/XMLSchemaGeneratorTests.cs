using System;
using Xunit;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MsgPack.Strict.SchemaGenerator.Tests
{
    public class XMLSchemaGeneratorTests
    {
        #region test classes
        public sealed class UserScore
        {
            public UserScore(string name, int score)
            {
                Name = name;
                Score = score;
            }

            public string Name { get; }
            public int Score { get; }
        }
        public sealed class UserScoreWithDefaultScore
        {
            public UserScoreWithDefaultScore(string name, int score = 100)
            {
                Name = name;
                Score = score;
            }

            public string Name { get; }
            public int Score { get; }
        }
        public enum TestEnum
        {
            Foo = 1,
            Bar = 2
        }
        public sealed class TestDefaultParams
        {
            public byte B { get; }
            public sbyte Sb { get; }
            public short S { get; }
            public ushort Us { get; }
            public int I { get; }
            public uint Ui { get; }
            public long L { get; }
            public ulong Ul { get; }
            public string Str { get; }
            public float F { get; }
            public double D { get; }
            public decimal Dc { get; }
            public bool Bo { get; }
            public TestEnum E { get; }
            public UserScore Complex { get; }

            public TestDefaultParams(
                sbyte sb = -12,
                byte b = 12,
                short s = -1234,
                ushort us = 1234,
                int i = -12345,
                uint ui = 12345,
                long l = -12345678900L,
                ulong ul = 12345678900UL,
                string str = "str",
                float f = 1.23f,
                double d = 1.23,
                decimal dc = 1.23M,
                TestEnum e = TestEnum.Bar,
                UserScore complex = null,
                bool bo = true)
            {
                B = b;
                Sb = sb;
                S = s;
                Us = us;
                I = i;
                Ui = ui;
                L = l;
                Ul = ul;
                Str = str;
                F = f;
                D = d;
                Dc = dc;
                Bo = bo;
                E = e;
                Complex = complex;
            }
        }

        public sealed class UserScoreList
        {
            public UserScoreList(string name, IReadOnlyList<int> scores)
            {
                Name = name;
                Scores = scores;
            }

            public string Name { get; }
            public IReadOnlyList<int> Scores { get; }
        }

        public sealed class NoPublicConstructors
        {
            public int Number { get; }

            internal NoPublicConstructors(int number)
            {
                Number = number;
            }
        }

        public sealed class MultipleConstructors
        {
            public int Number { get; }
            public string Text { get; }

            public MultipleConstructors(int number, string text)
            {
                Number = number;
                Text = text;
            }

            public MultipleConstructors(int number)
            {
                Number = number;
            }
        }


        #endregion


        [Fact]
        public void GenerateXMLSchemaForSimpleType()
        {
            /*
            <Message = "UserScore">
              <Field name="name" type="System.String" />
              <Field name="score" type = "System.Int32" />
            </UserScore>
            */

            var expected = new XElement("Message", new XAttribute("name", "UserScore"),
                new XElement("Field",
                    new XAttribute("name", "name"),
                    new XAttribute("type", "System.String")),
                new XElement("Field",
                    new XAttribute("name", "score"),
                    new XAttribute("type", "System.Int32"))
                    );

            var actual = XMLSchemaGenerator.GenerateSchema(typeof(UserScore));
            // Comparing XElements seems problematic, so bodge it by comparing
            // the string forms.
            Assert.Equal(expected.ToString(), actual.ToString());
        }

        [Fact]
        public void GenerateXMLSchemaForSimpleTypeWithDefaults()
        {
            /*
            <Message name=UserScoreWithDefaultScore">
              <Field name="name" type="System.String" />
              <Field name="score" type="System.Int32" default="100" />
            </UserScoreWithDefaultScore>
            */
            var expected = new XElement("Message", new XAttribute("name", "UserScoreWithDefaultScore"),
                            new XElement("Field",
                                new XAttribute("name", "name"),
                                new XAttribute("type", "System.String")),
                            new XElement("Field",
                                new XAttribute("name", "score"),
                                new XAttribute("type", "System.Int32"),
                                new XAttribute("default", "100"))
                                );
            var actual = XMLSchemaGenerator.GenerateSchema(typeof(UserScoreWithDefaultScore));
            // Comparing XElements seems problematic, so bodge it by comparing
            // the string forms.
            Assert.Equal(expected.ToString(), actual.ToString());
        }

        [Fact]
        public void GenerateXMLSchemaForTypeContainingList()
        {
            /*
            <Message name="UserScoreList">
              <Field name="name" type="System.String" />
              <Field name="scores" type="System.Collections.Generic.IReadOnlyList`1[System.Int32]" />
            </UserScoreList>
            */

            var expected = new XElement("Message", new XAttribute("name", "UserScoreList"),
                new XElement("Field",
                    new XAttribute("name", "name"),
                    new XAttribute("type", "System.String")),
                new XElement("Field",
                    new XAttribute("name", "scores"),
                    new XAttribute("type", "System.Collections.Generic.IReadOnlyList`1[System.Int32]"))
                    );

            var actual = XMLSchemaGenerator.GenerateSchema(typeof(UserScoreList));
            // Comparing XElements seems problematic, so bodge it by comparing
            // the string forms.
            Assert.Equal(expected.ToString(), actual.ToString());
        }

        [Fact]
        public void ThrowsOnNoPublicConstructors()
        {
            var ex = Assert.Throws<SchemaGenerationException>(
                () => XMLSchemaGenerator.GenerateSchema(typeof(NoPublicConstructors)));

            Assert.Equal(typeof(NoPublicConstructors), ex.TargetType);
            Assert.Equal("Type must have a single public constructor.", ex.Message);
        }

        [Fact]
        public void ThrowsOnMultipleConstructors()
        {
            var ex = Assert.Throws<SchemaGenerationException>(
                () => SchemaGenerator.GenerateSchema(typeof(MultipleConstructors)));

            Assert.Equal(typeof(MultipleConstructors), ex.TargetType);
            Assert.Equal("Type must have a single public constructor.", ex.Message);
        }

        [Fact]
        public void GenerateSchemaForTypeContainingComplexType()
        {
            /*
<Message name="TestDefaultParams">
  <Field name="sb" type="System.SByte" default="-12" />
  <Field name="b" type="System.Byte" default="12" />
  <Field name="s" type="System.Int16" default="-1234" />
  <Field name="us" type="System.UInt16" default="1234" />
  <Field name="i" type="System.Int32" default="-12345" />
  <Field name="ui" type="System.UInt32" default="12345" />
  <Field name="l" type="System.Int64" default="-12345678900" />
  <Field name="ul" type="System.UInt64" default="12345678900" />
  <Field name="str" type="System.String" default="str" />
  <Field name="f" type="System.Single" default="1.23" />
  <Field name="d" type="System.Double" default="1.23" />
  <Field name="dc" type="System.Decimal" default="1.23" />
  <Field name="e" type="MsgPack.Strict.SchemaGenerator.Tests.XMLSchemaGeneratorTests+TestEnum" default="Bar" />
  <Field name="complex" type="MsgPack.Strict.SchemaGenerator.Tests.XMLSchemaGeneratorTests+UserScore" default="null">
    <Type name="UserScore">
      <Field name="name" type="System.String" />
      <Field name="score" type="System.Int32" />
    </UserScore>
  </Field>
  <Field name="bo" type="System.Boolean" default="true" />
</TestDefaultParams>
            */

            var expected = new XElement("Message", new XAttribute("name", "TestDefaultParams"),
                            new XElement("Field",
                                new XAttribute("name", "sb"),
                                new XAttribute("type", "System.SByte"),
                                new XAttribute("default", "-12")),
                            new XElement("Field",
                                new XAttribute("name", "b"),
                                new XAttribute("type", "System.Byte"),
                                new XAttribute("default", "12")),
                            new XElement("Field",
                                new XAttribute("name", "s"),
                                new XAttribute("type", "System.Int16"),
                                new XAttribute("default", "-1234")),
                            new XElement("Field",
                                new XAttribute("name", "us"),
                                new XAttribute("type", "System.UInt16"),
                                new XAttribute("default", "1234")),
                            new XElement("Field",
                                new XAttribute("name", "i"),
                                new XAttribute("type", "System.Int32"),
                                new XAttribute("default", "-12345")),
                            new XElement("Field",
                                new XAttribute("name", "ui"),
                                new XAttribute("type", "System.UInt32"),
                                new XAttribute("default", "12345")),
                            new XElement("Field",
                                new XAttribute("name", "l"),
                                new XAttribute("type", "System.Int64"),
                                new XAttribute("default", "-12345678900")),
                            new XElement("Field",
                                new XAttribute("name", "ul"),
                                new XAttribute("type", "System.UInt64"),
                                new XAttribute("default", "12345678900")),
                            new XElement("Field",
                                new XAttribute("name", "str"),
                                new XAttribute("type", "System.String"),
                                new XAttribute("default", "str")),
                            new XElement("Field",
                                new XAttribute("name", "f"),
                                new XAttribute("type", "System.Single"),
                                new XAttribute("default", "1.23")),
                            new XElement("Field",
                                new XAttribute("name", "d"),
                                new XAttribute("type", "System.Double"),
                                new XAttribute("default", "1.23")),
                            new XElement("Field",
                                new XAttribute("name", "dc"),
                                new XAttribute("type", "System.Decimal"),
                                new XAttribute("default", "1.23")),
                            new XElement("Field",
                                new XAttribute("name", "e"),
                                new XAttribute("type", "MsgPack.Strict.SchemaGenerator.Tests.XMLSchemaGeneratorTests+TestEnum"),
                                new XAttribute("default", "Bar")),
                            new XElement("Field",
                                new XAttribute("name", "complex"),
                                new XAttribute("type", "MsgPack.Strict.SchemaGenerator.Tests.XMLSchemaGeneratorTests+UserScore"),
                                new XAttribute("default", "null"),
                                    new XElement("Type", new XAttribute("name", "UserScore"),
                                        new XElement("Field",
                                            new XAttribute("name", "name"),
                                            new XAttribute("type", "System.String")),
                                        new XElement("Field",
                                            new XAttribute("name", "score"),
                                            new XAttribute("type", "System.Int32"))
                                            )
                                ),
                            new XElement("Field",
                                new XAttribute("name", "bo"),
                                new XAttribute("type", "System.Boolean"),
                                new XAttribute("default", "true"))
                                );
            var actual = XMLSchemaGenerator.GenerateSchema(typeof(TestDefaultParams));
            // Comparing XElements seems problematic, so bodge it by comparing
            // the string forms.
            Assert.Equal(expected.ToString(), actual.ToString());
        }
    }
}
