using System.Collections.Generic;
using System.Windows;

namespace MsgPack.Strict.TestSchemaGenerationPostBuild
{
     [ReceiveMessage]
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

    [ReceiveMessage]
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

    [ReceiveMessage]
    [SendMessage]
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

    [ReceiveMessage]
    [SendMessage]
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

    [ReceiveMessage]
    [SendMessage]
    public sealed class UserScoreWrapper
    {
        public double Weight { get; }
        public UserScore UserScore { get; }

        public UserScoreWrapper(double weight, UserScore userScore)
        {
            Weight = weight;
            UserScore = userScore;
        }
    }

    [ReceiveMessage]
    [SendMessage]
    public sealed class UserScoreDecimal
    {
        public UserScoreDecimal(string name, decimal score)
        {
            Name = name;
            Score = score;
        }

        public string Name { get; }
        public decimal Score { get; }
    }

    [ReceiveMessage]
    [SendMessage]
    public sealed class WithEnumProperty
    {
        public WithEnumProperty(TestEnum testEnum)
        {
            TestEnum = testEnum;
        }

        public TestEnum TestEnum { get; }
    }

    [ReceiveMessage]
    [SendMessage]
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

    [ReceiveMessage]
    [SendMessage]
    public sealed class ListOfList
    {
        public IReadOnlyList<IReadOnlyList<int>> Jagged { get; }

        public ListOfList(IReadOnlyList<IReadOnlyList<int>> jagged)
        {
            Jagged = jagged;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
