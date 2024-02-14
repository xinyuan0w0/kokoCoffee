using BowlFrame.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Event
{
    public interface IEvent
    {
        public Adapter.IPlatform Platform { get; }

        public MsgType MsgType { get; }

        public SourceType SourceType { get; }

        public Messages? Messages { get; }
    }
}