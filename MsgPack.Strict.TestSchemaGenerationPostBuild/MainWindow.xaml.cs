using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MsgPack.Strict.TestSchemaGenerationPostBuild
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public sealed class MessageOne
    {
        public MessageOne(int i)
        {
            I = i;
        }
        public int I { get; }
    }

    public sealed class MessageTwo
    {
        public MessageTwo(int i, MessageOne messageOne)
        {
            I = i;
            MessageOne = messageOne;
        }
        public int I { get; }
        public MessageOne MessageOne { get;}
    }


    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            var stream = new MemoryStream();

            var messageOne = new MessageOne(1);
            var messageTwo = new MessageTwo(2, messageOne);

            StrictSerialiser.Get<MessageOne>().Serialise(stream, messageOne);
            stream.Position = 0;
            var messageOne_2 = StrictDeserialiser.Get<MessageOne>().Deserialise(stream.ToArray());

            stream.Position = 0;
            StrictSerialiser.Get<MessageTwo>().Serialise(stream, messageTwo);
            stream.Position = 0;
            var messageTwo_2 = StrictDeserialiser.Get<MessageTwo>().Deserialise(stream.ToArray());
        }
    }
}
