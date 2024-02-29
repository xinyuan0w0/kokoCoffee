using BowlFrame.Message;

namespace BowlFrame.Event
{
    public abstract class EventBase(Adapter.IPlatform platform, MsgType msgType, SourceType sourceType, DateTime? time = null) : IEvent
    {
        public Adapter.IPlatform Platform { get; } = platform;

        public MsgType MsgType { get; } = msgType;

        public SourceType SourceType { get; } = sourceType;

        public DateTime Time { get; } = time ?? DateTime.UtcNow;

        public Messages? Messages { get; }
    }
}