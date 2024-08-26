using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.CocoaPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CocoaEventAttribute : Attribute
    {
        public bool PrivateMessage { get; init; }

        public bool GroupMessage { get; init; }

        public bool GuildMessage { get; init; }

        public bool ChannelMessage { get; init; }

        public bool GuildPrivateMessage { get; init; }

        public bool PostMessage { get; init; }

        public bool MessageEvent { get; init; }

        public bool MainEvent { get; init; }

        public bool MetaEvent { get; init; }

        public bool Broadcast { get; init; }
    }
}