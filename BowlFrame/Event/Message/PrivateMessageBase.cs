using BowlFrame.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Event.Message
{
    internal abstract class PrivateMessageBase : MessageBase
    {
        protected PrivateMessageBase(IPlatform platform) : base(platform, SourceType.Private)
        {
        }
    }
}