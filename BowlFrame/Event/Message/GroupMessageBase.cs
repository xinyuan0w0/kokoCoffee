using BowlFrame.Adapter;
using BowlFrame.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Event.Message
{
    internal abstract class GroupMessageBase : MessageBase
    {
        public abstract string Room { get; }

        public GroupMessageBase(IPlatform platform) : base(platform, SourceType.Group)
        {
        }
    }
}