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
using MsgPack;
using Xunit;

namespace Dasher.Tests
{
    [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
    public sealed class DeserialiserTests
    {
        [Fact]
        public void MultiplePropertiesExactMatch()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123));

            var after = new Deserialiser<UserScore>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void DisallowsPrimitiveTypes()
        {
            var exception = Assert.Throws<DeserialisationException>(() => new Deserialiser<int>());
            Assert.Equal("Cannot deserialise type \"System.Int32\": Top level types must be complex to support future versioning.", exception.Message);
        }

        [Fact]
        public void ReorderedFields()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Score").Pack(123)
                .Pack("Name").Pack("Bob"));

            var after = new Deserialiser<UserScore>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void MixedUpCapitalisation()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(2)
                .Pack("NaMe").Pack("Bob")
                .Pack("ScorE").Pack(123));

            var after = new Deserialiser<UserScore>().Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void ThrowsOnUnexpectedField()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(3)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123)
                .Pack("SUPRISE").Pack("Unexpected"));

            var deserialiser = new Deserialiser<UserScore>();
            var ex = Assert.Throws<DeserialisationException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Encountered unexpected field \"SUPRISE\" of MsgPack format \"FixStr\" for CLR type \"UserScore\".", ex.Message);
        }

        [Fact]
        public void IgnoresUnexpectedFieldAtBeginning()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(3)
                .Pack("SUPRISE").Pack("Unexpected")
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123));

            var deserialiser = new Deserialiser<UserScore>(UnexpectedFieldBehaviour.Ignore);
            var after = deserialiser.Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void IgnoresUnexpectedFieldAtEnd()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(3)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123)
                .Pack("SUPRISE").Pack("Unexpected"));

            var deserialiser = new Deserialiser<UserScore>(UnexpectedFieldBehaviour.Ignore);
            var after = deserialiser.Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(123, after.Score);
        }

        [Fact]
        public void ThrowsOnMissingField()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack("Name").Pack("Bob"));

            var deserialiser = new Deserialiser<UserScore>();
            var ex = Assert.Throws<DeserialisationException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Missing required field \"score\" for type \"UserScore\".", ex.Message);
        }

        [Fact]
        public void ThrowsOnIncorrectDataType()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123.4)); // double, should be int

            var deserialiser = new Deserialiser<UserScore>();
            var ex = Assert.Throws<DeserialisationException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Unexpected MsgPack format for \"score\". Expected Int32, got Float64.", ex.Message);
        }

        [Fact]
        public void ThrowsOnDuplicateField()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(3)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(123)
                .Pack("Score").Pack(321));

            var deserialiser = new Deserialiser<UserScore>();
            var ex = Assert.Throws<DeserialisationException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal(typeof(UserScore), ex.TargetType);
            Assert.Equal("Encountered duplicate field \"Score\" for type \"UserScore\".", ex.Message);
        }

        [Fact]
        public void ThrowsOnNonMapData()
        {
            var bytes = PackBytes(packer => packer.PackArrayHeader(2)
                .Pack("Name").Pack(123));

            var deserialiser = new Deserialiser<UserScore>();
            var ex = Assert.Throws<DeserialisationException>(() => deserialiser.Deserialise(bytes));
            Assert.Equal("Message must be encoded as a MsgPack map, not \"FixArray\".", ex.Message);
            Assert.Equal(typeof(UserScore), ex.TargetType);
        }

        [Fact]
        public void ThrowsOnEmptyData()
        {
            var bytes = new byte[0];

            var deserialiser = new Deserialiser<UserScore>();
            var ex = Assert.Throws<IOException>(
                () => deserialiser.Deserialise(bytes));

            Assert.Equal("End of stream reached.", ex.Message);
        }

        [Fact]
        public void DeserialisesEnumMembersCaseInsensitively()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack(nameof(ValueWrapper<TestEnum>.Value)).Pack("BAR"));

            var after = new Deserialiser<ValueWrapper<TestEnum>>().Deserialise(bytes);

            Assert.Equal(TestEnum.Bar, after.Value);
        }

        [Fact]
        public void ThrowsWhenEnumNotEncodedAsString()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack(nameof(ValueWrapper<TestEnum>.Value)).Pack(123));

            var ex = Assert.Throws<DeserialisationException>(
                () => new Deserialiser<ValueWrapper<TestEnum>>().Deserialise(bytes));

            Assert.Equal(typeof(ValueWrapper<TestEnum>), ex.TargetType);
            Assert.Equal($"Unable to read string value for enum property \"value\" of type \"{typeof(TestEnum)}\"", ex.Message);
        }

        [Fact]
        public void ThrowsWhenEnumStringNotValidMember()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack(nameof(ValueWrapper<TestEnum>.Value)).Pack("Rubbish"));

            var ex = Assert.Throws<DeserialisationException>(
                () => new Deserialiser<ValueWrapper<TestEnum>>().Deserialise(bytes));

            Assert.Equal(typeof(ValueWrapper<TestEnum>), ex.TargetType);
            Assert.Equal($"Unable to parse value \"Rubbish\" as a member of enum type \"{typeof(TestEnum)}\"", ex.Message);
        }

        [Fact]
        public void UsesDefaultValuesIfNotInMessage()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(0));

            var deserialiser = new Deserialiser<ClassWithAllDefaults>();
            var after = deserialiser.Deserialise(bytes);

            after.AssertHasDefaultValues();
        }

        [Fact]
        public void SpecifiedValueOverridesDefaultValue()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(2)
                .Pack("Name").Pack("Bob")
                .Pack("Score").Pack(12345)); // score has a default of 100

            var deserialiser = new Deserialiser<UserScoreWithDefaultScore>();
            var after = deserialiser.Deserialise(bytes);

            Assert.Equal("Bob", after.Name);
            Assert.Equal(12345, after.Score);
        }

        [Fact]
        public void ThrowsOnMultipleConstructors()
        {
            var ex = Assert.Throws<DeserialisationException>(
                () => new Deserialiser<MultipleConstructors>());
            Assert.Equal($"Cannot deserialise type \"{typeof(MultipleConstructors).FullName}\": Complex type provider requires a 1 public constructor, not 2.", ex.Message);
        }

        [Fact]
        public void ThrowsNoPublicConstructors()
        {
            var ex = Assert.Throws<DeserialisationException>(
                () => new Deserialiser<NoPublicConstructors>());
            Assert.Equal($"Cannot deserialise type \"{typeof(NoPublicConstructors).FullName}\": Complex type provider requires a public constructor.", ex.Message);
        }

        [Fact]
        public void HandlesNullRootObject()
        {
            var stream = new MemoryStream();
            var packer = new Packer(stream);
            packer.PackNull();

            packer.Flush();
            stream.Position = 0;

            Assert.Null(new Deserialiser<Recurring>().Deserialise(stream));
        }

        [Fact]
        public void HandlesNullNestedObject()
        {
            var stream = new MemoryStream();
            var packer = new Packer(stream);
            packer.PackMapHeader(2);
            packer.Pack("Num");
            packer.Pack(1);
            packer.Pack("Inner");
            packer.PackNull();

            packer.Flush();
            stream.Position = 0;

            Assert.Null(new Deserialiser<Recurring>().Deserialise(stream).Inner);
        }

        [Fact]
        public void HandlesRecurringType()
        {
            var stream = new MemoryStream();
            var packer = new Packer(stream);

            packer.PackMapHeader(2);

            packer.Pack("Num");
            packer.Pack(1);

            packer.Pack("Inner");
            {
                packer.PackMapHeader(2);
                packer.Pack("Num");
                packer.Pack(2);
                packer.Pack("Inner");
                packer.PackNull();
            }

            packer.Flush();
            stream.Position = 0;

            var after = new Deserialiser<Recurring>().Deserialise(stream);

            Assert.Equal(1, after.Num);
            Assert.NotNull(after.Inner);
            Assert.Equal(2, after.Inner.Num);
            Assert.Null(after.Inner.Inner);
        }

        [Fact]
        public void HandlesRecurringTreeType()
        {
            var stream = new MemoryStream();
            var packer = new Packer(stream);

            packer.PackMapHeader(2);

            packer.Pack("Num");
            packer.Pack(1);

            packer.Pack("Inner");
            {
                packer.PackArrayHeader(2);
                {
                    packer.PackMapHeader(2);
                    packer.Pack("Num");
                    packer.Pack(2);
                    packer.Pack("Inner");
                    packer.PackNull();

                    packer.PackMapHeader(2);
                    packer.Pack("Num");
                    packer.Pack(3);
                    packer.Pack("Inner");
                    packer.PackNull();
                }
            }

            packer.Flush();
            stream.Position = 0;

            var after = new Deserialiser<RecurringTree>().Deserialise(stream);

            Assert.Equal(1, after.Num);
            Assert.NotNull(after.Inner);
            Assert.Equal(2, after.Inner.Count);
            Assert.Equal(2, after.Inner[0].Num);
            Assert.Null(after.Inner[0].Inner);
            Assert.Equal(3, after.Inner[1].Num);
            Assert.Null(after.Inner[1].Inner);
        }

        [Fact]
        public void HandlesNullableWithDefault()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(0));

            var after = new Deserialiser<NullableWithDefaultValue>().Deserialise(bytes);

            Assert.True(after.B);
        }

        [Fact]
        public void TupleThrowsIfTooFewItems()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1).Pack(nameof(ValueWrapper<Tuple<int, string, bool?>>.Value)).PackArrayHeader(2).Pack(1).Pack("Hello"));

            var ex = Assert.Throws(
                typeof(DeserialisationException),
                () => new Deserialiser<ValueWrapper<Tuple<int, string, bool?>>>().Deserialise(bytes));

            Assert.Equal($"Received array must have length 3 for type {typeof(Tuple<int, string, bool?>).FullName}", ex.Message);
        }

        [Fact]
        public void TupleThrowsIfTooManyItems()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1).Pack(nameof(ValueWrapper<Tuple<int, string>>.Value)).PackArrayHeader(3).Pack(1).Pack("Hello").Pack("Extra!!"));

            var ex = Assert.Throws(
                typeof(DeserialisationException),
                () => new Deserialiser<ValueWrapper<Tuple<int, string>>>().Deserialise(bytes));

            Assert.Equal($"Received array must have length 2 for type {typeof(Tuple<int, string>).FullName}", ex.Message);
        }

        [Fact]
        public void TupleThrowsIfNotArray()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1).Pack(nameof(ValueWrapper<Tuple<int, string>>.Value)).Pack("Not an array"));

            var ex = Assert.Throws(
                typeof(DeserialisationException),
                () => new Deserialiser<ValueWrapper<Tuple<int, string>>>().Deserialise(bytes));

            Assert.Equal("Expecting tuple data to be encoded as array", ex.Message);
        }

        [Fact]
        public void ThrowsWhenDecimalNotEncodedAsString()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack(nameof(ValueWrapper<decimal>.Value)).Pack(true));

            var ex = Assert.Throws<DeserialisationException>(() => new Deserialiser<ValueWrapper<decimal>>().Deserialise(bytes));
            Assert.Equal($"Unable to deserialise decimal value from MsgPack format {nameof(Format.True)}.", ex.Message);
        }

        [Fact]
        public void ThrowsWhenDecimalEncodedAsUnparseableString()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack(nameof(ValueWrapper<decimal>.Value)).Pack("NOTADECIMAL"));

            var ex = Assert.Throws<DeserialisationException>(() => new Deserialiser<ValueWrapper<decimal>>().Deserialise(bytes));
            Assert.Equal("Unable to parse string \"NOTADECIMAL\" as a decimal for \"value\".", ex.Message);
        }

        [Fact]
        public void ThrowsWhenGuidNotEncodedAsString()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack(nameof(ValueWrapper<Guid>.Value)).Pack(1234));

            var ex = Assert.Throws<DeserialisationException>(() => new Deserialiser<ValueWrapper<Guid>>().Deserialise(bytes));
            Assert.Equal("Unable to deserialise GUID value", ex.Message);
        }

        [Fact]
        public void ThrowsWhenGuidEncodedAsUnparseableString()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack(nameof(ValueWrapper<Guid>.Value)).Pack("NOTAGUID"));

            var ex = Assert.Throws<DeserialisationException>(() => new Deserialiser<ValueWrapper<Guid>>().Deserialise(bytes));
            Assert.Equal("Unable to deserialise GUID value", ex.Message);
        }

        [Fact]
        public void DictionaryDataWithDuplicateKeyThrows()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack(nameof(ValueWrapper<IReadOnlyDictionary<int, string>>.Value))
                .PackMapHeader(2)
                    .Pack(1).Pack("Hello")
                    .Pack(1).Pack("Duplicate!"));

            var ex = Assert.Throws<ArgumentException>(
                () => new Deserialiser<ValueWrapper<IReadOnlyDictionary<int, string>>>().Deserialise(bytes));

#if NETCOREAPP1_0
            Assert.Equal("An item with the same key has already been added. Key: 1", ex.Message);
#else
#if NET451
            Assert.Equal("An item with the same key has already been added.", ex.Message);
#else
            throw new Exception("Build configuration is not tested.")
#endif
#endif
        }

        [Fact]
        public void ThrowsIfNullByteArray()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new Deserialiser<UserScore>().Deserialise((byte[])null));

            Assert.Equal("bytes", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => new Deserialiser(typeof(UserScore)).Deserialise((byte[])null));

            Assert.Equal("bytes", ex.ParamName);
        }

        [Fact]
        public void ThrowsIfNullStream()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new Deserialiser<UserScore>().Deserialise((Stream)null));

            Assert.Equal("stream", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => new Deserialiser(typeof(UserScore)).Deserialise((Stream)null));

            Assert.Equal("stream", ex.ParamName);
        }

        [Fact]
        public void ThrowsIfNullUnpacker()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new Deserialiser<UserScore>().Deserialise((Unpacker)null));

            Assert.Equal("unpacker", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => new Deserialiser(typeof(UserScore)).Deserialise((Unpacker)null));

            Assert.Equal("unpacker", ex.ParamName);
        }

        [Fact]
        public void ThrowsIfEmptyChar()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack(nameof(ValueWrapper<Union<int, string>>.Value))
                .PackMapHeader(0));

            var ex = Assert.Throws<DeserialisationException>(() => new Deserialiser<ValueWrapper<char>>().Deserialise(bytes));

            Assert.Equal("Unexpected MsgPack format for \"value\". Expected string, got FixMap.", ex.Message);
        }

        [Fact]
        public void ThrowsIfStringTooLongForChar()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack(nameof(ValueWrapper<Union<int, string>>.Value))
                .PackString("Hello"));

            var ex = Assert.Throws<DeserialisationException>(() => new Deserialiser<ValueWrapper<char>>().Deserialise(bytes));

            Assert.Equal("Unexpected string length for char value \"value\". Expected 1, got 5.", ex.Message);
        }

        [Fact]
        public void ThrowsIfUnionHasWrongNumberOfArrayElements()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack(nameof(ValueWrapper<Union<int, string>>.Value))
                .PackArrayHeader(3)
                    .Pack("String")
                    .Pack("Hello")
                    .Pack("World"));

            var ex = Assert.Throws<DeserialisationException>(() => new Deserialiser<ValueWrapper<Union<int, string>>>().Deserialise(bytes));

            Assert.Equal(@"Union array should have 2 elements (not 3) for property ""value"" of type ""Dasher.Union`2[System.Int32,System.String]""",
                ex.Message);
        }

        [Fact]
        public void ThrowsIfReceivedTypeNotInUnion()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack(nameof(ValueWrapper<Union<int, double>>.Value))
                .PackArrayHeader(3)
                    .Pack("String")
                    .Pack("Hello"));

            var ex = Assert.Throws<DeserialisationException>(() => new Deserialiser<ValueWrapper<Union<int, string>>>().Deserialise(bytes));

            Assert.Equal(@"Union array should have 2 elements (not 3) for property ""value"" of type ""Dasher.Union`2[System.Int32,System.String]""",
                ex.Message);
        }

        [Fact]
        public void ThrowsIfReceivedDataNotAnArray()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1)
                .Pack(nameof(ValueWrapper<Union<int, double>>.Value))
                .Pack("String"));

            var ex = Assert.Throws<DeserialisationException>(() => new Deserialiser<ValueWrapper<Union<int, string>>>().Deserialise(bytes));

            Assert.Equal(@"Union values must be encoded as an array for property ""value"" of type ""Dasher.Union`2[System.Int32,System.String]""",
                ex.Message);
        }

        [Fact]
        public void DeserialiseComplexAsEmpty()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(1).Pack("Hello").Pack("World"));

            Assert.Null(new Deserialiser<Empty>(UnexpectedFieldBehaviour.Ignore).Deserialise(bytes));

            var ex = Assert.Throws<DeserialisationException>(
                () => new Deserialiser<Empty>(UnexpectedFieldBehaviour.Throw).Deserialise(bytes));

            Assert.Equal("Unable to deserialise Empty type for \"<root>\". Expected map with 0 entries, got 1.",
                ex.Message);
        }

        [Fact]
        public void DeserialiseArrayStringAsEmpty()
        {
            var bytes = PackBytes(packer => packer.PackArrayHeader(2).Pack("Hello").Pack("World"));

            Assert.Null(new Deserialiser<Empty>(UnexpectedFieldBehaviour.Ignore).Deserialise(bytes));

            var ex = Assert.Throws<DeserialisationException>(
                () => new Deserialiser<Empty>(UnexpectedFieldBehaviour.Throw).Deserialise(bytes));

            Assert.Equal("Unable to deserialise Empty type for \"<root>\". Expected MsgPack format Null or Map, got FixArray.",
                ex.Message);
        }

        [Fact]
        public void DeserialiseStringAsEmpty()
        {
            var bytes = PackBytes(packer => packer.PackString("Hello"));

            Assert.Null(new Deserialiser<Empty>(UnexpectedFieldBehaviour.Ignore).Deserialise(bytes));

            var ex = Assert.Throws<DeserialisationException>(
                () => new Deserialiser<Empty>(UnexpectedFieldBehaviour.Throw).Deserialise(bytes));

            Assert.Equal("Unable to deserialise Empty type for \"<root>\". Expected MsgPack format Null or Map, got FixStr.",
                ex.Message);
        }

        [Fact]
        public void DeserialiseNullAsEmpty()
        {
            var bytes = PackBytes(packer => packer.PackNull());

            Assert.Null(new Deserialiser<Empty>(UnexpectedFieldBehaviour.Ignore).Deserialise(bytes));
            Assert.Null(new Deserialiser<Empty>(UnexpectedFieldBehaviour.Throw).Deserialise(bytes));
        }

        [Fact]
        public void DeserialiseEmptyAsComplexWithAllDefaults()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(0));

            var o = new Deserialiser<ClassWithAllDefaults>(UnexpectedFieldBehaviour.Throw).Deserialise(bytes);

            o.AssertHasDefaultValues();
        }

        [Fact]
        public void DeserialiseEmptyAsUnion()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(0));

            var o = new Deserialiser<Union<UserScore, UserScoreStruct>>(UnexpectedFieldBehaviour.Throw).Deserialise(bytes);

            Assert.Null(o);
        }

        [Fact]
        public void DeserialiseEmptyAsNullableComplexStructWithAllDefaults()
        {
            var bytes = PackBytes(packer => packer.PackMapHeader(0));

            var o = new Deserialiser<StructWithAllDefaults?>(UnexpectedFieldBehaviour.Throw).Deserialise(bytes);

            Assert.Null(o);
        }

        [Fact]
        public void ValueConversions()
        {
            // bool
            ConversionFails<byte,    bool>(byte.MaxValue);
            ConversionFails<sbyte,   bool>(sbyte.MaxValue);
            ConversionFails<char,    bool>(char.MaxValue);
            ConversionFails<short,   bool>(short.MaxValue);
            ConversionFails<ushort,  bool>(ushort.MaxValue);
            ConversionFails<int,     bool>(int.MaxValue);
            ConversionFails<uint,    bool>(uint.MaxValue);
            ConversionFails<long,    bool>(long.MaxValue);
            ConversionFails<ulong,   bool>(ulong.MaxValue);
            ConversionFails<decimal, bool>(decimal.MaxValue);
            ConversionFails<float,   bool>(float.MaxValue);
            ConversionFails<double,  bool>(double.MaxValue);

            // byte
            ConversionFails<bool,    byte>(true, false);
            ConversionFails<sbyte,   byte>(sbyte.MinValue);
            ConversionFails<char,    byte>(char.MaxValue);
            ConversionFails<short,   byte>(short.MaxValue);
            ConversionFails<ushort,  byte>(ushort.MaxValue);
            ConversionFails<int,     byte>(int.MaxValue);
            ConversionFails<uint,    byte>(uint.MaxValue);
            ConversionFails<long,    byte>(long.MaxValue);
            ConversionFails<ulong,   byte>(ulong.MaxValue);
            ConversionFails<decimal, byte>(decimal.MaxValue);
            ConversionFails<float,   byte>(float.MaxValue);
            ConversionFails<double,  byte>(double.MaxValue);

            // sbyte
            ConversionFails<bool,    sbyte>(true);
            ConversionFails<byte,    sbyte>(byte.MaxValue);
            ConversionFails<char,    sbyte>(char.MaxValue);
            ConversionFails<short,   sbyte>(short.MaxValue);
            ConversionFails<ushort,  sbyte>(ushort.MaxValue);
            ConversionFails<int,     sbyte>(int.MaxValue);
            ConversionFails<uint,    sbyte>(uint.MaxValue);
            ConversionFails<long,    sbyte>(long.MaxValue);
            ConversionFails<ulong,   sbyte>(ulong.MaxValue);
            ConversionFails<decimal, sbyte>(decimal.MaxValue);
            ConversionFails<float,   sbyte>(float.MaxValue);
            ConversionFails<double,  sbyte>(double.MaxValue);

            // char
            ConversionFails<bool,    char>(true);
            ConversionFails<byte,    char>(byte.MaxValue);
            ConversionFails<sbyte,   char>(sbyte.MaxValue);
            ConversionFails<short,   char>(short.MaxValue);
            ConversionFails<ushort,  char>(ushort.MaxValue);
            ConversionFails<int,     char>(int.MaxValue);
            ConversionFails<uint,    char>(uint.MaxValue);
            ConversionFails<long,    char>(long.MaxValue);
            ConversionFails<ulong,   char>(ulong.MaxValue);
            ConversionFails<decimal, char>(decimal.MaxValue);
            ConversionFails<float,   char>(float.MaxValue);
            ConversionFails<double,  char>(double.MaxValue);

            // short
            ConversionFails<bool,    short>(true);
            ConversionWorks<byte,    short>(byte.MaxValue);
            ConversionWorks<sbyte,   short>(sbyte.MaxValue);
            ConversionFails<char,    short>(char.MaxValue);
            ConversionFails<ushort,  short>(ushort.MaxValue);
            ConversionFails<int,     short>(int.MaxValue);
            ConversionFails<uint,    short>(uint.MaxValue);
            ConversionFails<long,    short>(long.MaxValue);
            ConversionFails<ulong,   short>(ulong.MaxValue);
            ConversionFails<decimal, short>(decimal.MaxValue);
            ConversionFails<float,   short>(float.MaxValue);
            ConversionFails<double,  short>(double.MaxValue);

            // ushort
            ConversionFails<bool,    ushort>(true);
            ConversionWorks<byte,    ushort>(byte.MaxValue);
            ConversionFails<sbyte,   ushort>(sbyte.MinValue);
            ConversionFails<char,    ushort>(char.MaxValue);
            ConversionFails<short,   ushort>(short.MaxValue);
            ConversionFails<int,     ushort>(int.MaxValue);
            ConversionFails<uint,    ushort>(uint.MaxValue);
            ConversionFails<long,    ushort>(long.MaxValue);
            ConversionFails<ulong,   ushort>(ulong.MaxValue);
            ConversionFails<decimal, ushort>(decimal.MaxValue);
            ConversionFails<float,   ushort>(float.MaxValue);
            ConversionFails<double,  ushort>(double.MaxValue);

            // int
            ConversionFails<bool,    int>(true);
            ConversionWorks<byte,    int>(byte.MaxValue);
            ConversionWorks<sbyte,   int>(sbyte.MaxValue);
            ConversionFails<char,    int>(char.MaxValue);
            ConversionWorks<short,   int>(short.MaxValue);
            ConversionWorks<ushort,  int>(ushort.MaxValue);
            ConversionFails<uint,    int>(uint.MaxValue);
            ConversionFails<long,    int>(long.MaxValue);
            ConversionFails<ulong,   int>(ulong.MaxValue);
            ConversionFails<decimal, int>(decimal.MaxValue);
            ConversionFails<float,   int>(float.MaxValue);
            ConversionFails<double,  int>(double.MaxValue);

            // uint
            ConversionFails<bool,    uint>(true);
            ConversionWorks<byte,    uint>(byte.MaxValue);
            ConversionFails<sbyte,   uint>(sbyte.MinValue);
            ConversionFails<char,    uint>(char.MaxValue);
            ConversionFails<short,   uint>(short.MaxValue);
            ConversionWorks<ushort,  uint>(ushort.MaxValue);
            ConversionFails<int,     uint>(int.MaxValue);
            ConversionFails<long,    uint>(long.MaxValue);
            ConversionFails<ulong,   uint>(ulong.MaxValue);
            ConversionFails<decimal, uint>(decimal.MaxValue);
            ConversionFails<float,   uint>(float.MaxValue);
            ConversionFails<double,  uint>(double.MaxValue);

            // long
            ConversionFails<bool,    long>(true);
            ConversionWorks<byte,    long>(byte.MaxValue);
            ConversionWorks<sbyte,   long>(sbyte.MaxValue);
            ConversionFails<char,    long>(char.MaxValue);
            ConversionWorks<short,   long>(short.MaxValue);
            ConversionWorks<ushort,  long>(ushort.MaxValue);
            ConversionWorks<int,     long>(int.MaxValue);
            ConversionWorks<uint,    long>(uint.MaxValue);
            ConversionFails<ulong,   long>(ulong.MaxValue);
            ConversionFails<decimal, long>(decimal.MaxValue);
            ConversionFails<float,   long>(float.MaxValue);
            ConversionFails<double,  long>(double.MaxValue);

            // ulong
            ConversionFails<bool,    ulong>(true);
            ConversionWorks<byte,    ulong>(byte.MaxValue);
            ConversionFails<sbyte,   ulong>(sbyte.MinValue);
            ConversionFails<char,    ulong>(char.MaxValue);
            ConversionFails<short,   ulong>(short.MaxValue);
            ConversionWorks<ushort,  ulong>(ushort.MaxValue);
            ConversionFails<int,     ulong>(int.MaxValue);
            ConversionWorks<uint,    ulong>(uint.MaxValue);
            ConversionFails<long,    ulong>(long.MaxValue);
            ConversionFails<decimal, ulong>(decimal.MaxValue);
            ConversionFails<float,   ulong>(float.MaxValue);
            ConversionFails<double,  ulong>(double.MaxValue);

            // decimal
            ConversionFails<bool,    decimal>(true);
            ConversionWorks<byte,    decimal>(byte.MaxValue);
            ConversionWorks<sbyte,   decimal>(sbyte.MaxValue);
            ConversionFails<char,    decimal>(char.MaxValue);
            ConversionWorks<short,   decimal>(short.MaxValue);
            ConversionWorks<ushort,  decimal>(ushort.MaxValue);
            ConversionWorks<int,     decimal>(int.MaxValue);
            ConversionWorks<uint,    decimal>(uint.MaxValue);
            ConversionWorks<long,    decimal>(long.MaxValue);
            ConversionWorks<ulong,   decimal>(ulong.MaxValue);
            ConversionFails<float,   decimal>(float.MaxValue);
            ConversionFails<double,  decimal>(double.MaxValue);

            // TODO Make a decistion on commented code
            // float
            ConversionFails<bool,    float>(true);
            //ConversionWorks<byte,    float>(byte.MaxValue, byte.MaxValue);
            //ConversionWorks<sbyte,   float>(sbyte.MaxValue, sbyte.MaxValue);
            ConversionFails<char,    float>(char.MaxValue);
            //ConversionWorks<short,   float>(short.MaxValue, short.MaxValue);
            //ConversionWorks<ushort,  float>(ushort.MaxValue, ushort.MaxValue);
            ConversionFails<int,     float>(int.MaxValue);
            ConversionFails<uint,    float>(uint.MaxValue);
            ConversionFails<long,    float>(long.MaxValue);
            ConversionFails<ulong,   float>(ulong.MaxValue);
            ConversionFails<decimal, float>(decimal.MaxValue);
            ConversionFails<double,  float>(char.MaxValue);

            // double
            ConversionFails<bool,    double>(true);
            //ConversionWorks<byte,    double>(byte.MaxValue, byte.MaxValue);
            //ConversionWorks<sbyte,   double>(sbyte.MaxValue, sbyte.MaxValue);
            ConversionFails<char,    double>(char.MaxValue);
            //ConversionWorks<short,   double>(short.MaxValue, short.MaxValue);
            //ConversionWorks<ushort,  double>(ushort.MaxValue, ushort.MaxValue);
            //ConversionWorks<int,     double>(int.MaxValue, int.MaxValue);
            //ConversionWorks<uint,    double>(uint.MaxValue, uint.MaxValue);
            ConversionFails<long,    double>(long.MaxValue);
            ConversionFails<ulong,   double>(ulong.MaxValue);
            ConversionFails<decimal, double>(decimal.MaxValue);
            ConversionWorks<float,   double>(float.MaxValue);
        }

        private static void ConversionWorks<TFrom, TTo>(params TFrom[] values)
        {
            var stream = new MemoryStream();
            var serialiser = new Serialiser<ValueWrapper<TFrom>>();
            var deserialiser = new Deserialiser<ValueWrapper<TTo>>();

            foreach (var val in values)
            {
                serialiser.Serialise(stream, new ValueWrapper<TFrom>(val));
                stream.Position = 0;

                var actual = deserialiser.Deserialise(stream).Value;
                Assert.Equal(Convert.ChangeType(val, typeof(TTo)), actual);
            }
        }

        private static void ConversionFails<TFrom, TTo>(params TFrom[] values)
        {
            var stream = new MemoryStream();
            var serialiser = new Serialiser<ValueWrapper<TFrom>>();
            var deserialiser = new Deserialiser<ValueWrapper<TTo>>();

            foreach (var value in values)
            {
                stream.Position = 0;
                serialiser.Serialise(stream, new ValueWrapper<TFrom>(value));

                stream.Position = 0;
                Assert.Throws<DeserialisationException>(() => deserialiser.Deserialise(stream).Value);
            }
        }

        #region Helper

        private static byte[] PackBytes(Action<MsgPack.Packer> packAction)
        {
            var stream = new MemoryStream();
            var packer = MsgPack.Packer.Create(stream, PackerCompatibilityOptions.None);
            packAction(packer);
            stream.Position = 0;
            return stream.ToArray();
        }

        #endregion
    }
}
