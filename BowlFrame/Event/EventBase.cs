using BowlFrame.Message;

namespace BowlFrame.Event
{
    public abstract class EventBase : IEvent
    {
        public Adapter.IPlatform Platform { get; }

        public MsgType MsgType { get; }

        public SourceType SourceType { get; }

        public DateTime Time { get; }

        public EventBase(Adapter.IPlatform platform, MsgType msgType, SourceType sourceType, DateTime? time = null)
        {
            Platform = platform;
            MsgType = msgType;
            SourceType = sourceType;
            Time = time ?? DateTime.UtcNow;
        }

        public Messages? Messages { get; }
    }
}