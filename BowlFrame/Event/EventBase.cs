using BowlFrame.Message;

namespace BowlFrame.Event
{
    public abstract class EventBase(Adapter.IPlatform platform, EventType msgType, SourceType sourceType, DateTime? time = null) : IEvent
    {
        public Adapter.IPlatform Platform { get; } = platform;

        public EventType EventType { get; } = msgType;

        public SourceType SourceType { get; } = sourceType;

        public DateTime Time { get; } = time ?? DateTime.UtcNow;

        public Messages? Messages { get; protected set; }
    }
}