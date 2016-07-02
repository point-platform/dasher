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

using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dasher.Tests
{
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

    public struct UserScoreStruct
    {
        public UserScoreStruct(string name, int score)
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

    public sealed class WeightedUserScore
    {
        public double Weight { get; }
        public UserScore UserScore { get; }

        public WeightedUserScore(double weight, UserScore userScore)
        {
            Weight = weight;
            UserScore = userScore;
        }
    }

    public sealed class ValueWrapper<T>
    {
        public T Value { get; }

        public ValueWrapper(T value)
        {
            Value = value;
        }
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
            bool bo = true,
            TestEnum e = TestEnum.Bar,
            UserScore complex = null)
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

    public sealed class NoPublicConstructors
    {
        public int Number { get; }

        internal NoPublicConstructors(int number)
        {
            Number = number;
        }
    }

    public sealed class ListOfList
    {
        public IReadOnlyList<IReadOnlyList<int>> Jagged { get; }

        public ListOfList(IReadOnlyList<IReadOnlyList<int>> jagged)
        {
            Jagged = jagged;
        }
    }

    public sealed class Recurring
    {
        public int Num { get; }
        public Recurring Inner { get; }

        public Recurring(int num, Recurring inner)
        {
            Num = num;
            Inner = inner;
        }
    }

    public sealed class RecurringTree
    {
        public int Num { get; }
        public IReadOnlyList<RecurringTree> Inner { get; }

        public RecurringTree(int num, IReadOnlyList<RecurringTree> inner)
        {
            Num = num;
            Inner = inner;
        }
    }

    public sealed class NullableWithDefaultValue
    {
        public NullableWithDefaultValue(bool? b = true)
        {
            B = b;
        }

        public bool? B { get; }
    }

    public sealed class NoProperties
    {
    }
}