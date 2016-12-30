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
using System.IO;
using Xunit;

namespace Dasher.Tests
{
    public sealed class TypeConversionTests
    {
        [Fact]
        public void ValueConversions()
        {
            // bool
            ConversionFails<byte,    bool>(byte.MaxValue, byte.MinValue, default(byte));
            ConversionFails<sbyte,   bool>(sbyte.MaxValue, sbyte.MinValue, default(sbyte));
            ConversionFails<char,    bool>(char.MaxValue, char.MinValue, default(char));
            ConversionFails<short,   bool>(short.MaxValue, short.MinValue, default(short));
            ConversionFails<ushort,  bool>(ushort.MaxValue, ushort.MinValue, default(ushort));
            ConversionFails<int,     bool>(int.MaxValue, int.MinValue, default(int));
            ConversionFails<uint,    bool>(uint.MaxValue, uint.MinValue, default(uint));
            ConversionFails<long,    bool>(long.MaxValue, long.MinValue, default(long));
            ConversionFails<ulong,   bool>(ulong.MaxValue, ulong.MinValue, default(ulong));
            ConversionFails<decimal, bool>(decimal.MaxValue, decimal.MinValue, default(decimal));
            ConversionFails<float,   bool>(float.MaxValue, float.MinValue, default(float));
            ConversionFails<double,  bool>(double.MaxValue, double.MinValue, default(double));

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

        #region Helpers

        private static void ConversionWorks<TFrom, TTo>(params TFrom[] values)
        {
            var stream = new MemoryStream();
            var serialiser = new Serialiser<ValueWrapper<TFrom>>();
            var deserialiser = new Deserialiser<ValueWrapper<TTo>>();

            foreach (var value in values)
            {
                stream.Position = 0;
                serialiser.Serialise(stream, new ValueWrapper<TFrom>(value));

                stream.Position = 0;
                var actual = deserialiser.Deserialise(stream).Value;
                Assert.Equal(Convert.ChangeType(value, typeof(TTo)), actual);
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

        #endregion
    }
}