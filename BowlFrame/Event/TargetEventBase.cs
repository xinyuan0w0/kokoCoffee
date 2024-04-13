using BowlFrame.Adapter;
using BowlFrame.Message;
using BowlFrame.Target;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Event
{
    public abstract class TargetEventBase(IPlatform platform, SourceType sourceType, EventType eventType) : EventBase(platform, eventType)
    {
        public SourceType SourceType { get; } = sourceType;

        public abstract string Target { get; }
    }
}