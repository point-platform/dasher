using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgPack.Strict
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class SendMessageAttribute : System.Attribute
    {
        public SendMessageAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class ReceiveMessageAttribute : System.Attribute
    {
        public ReceiveMessageAttribute()
        {
        }
    }
}
