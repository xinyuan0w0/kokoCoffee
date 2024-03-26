using BowlFrame.Message;

namespace BowlFrame.Event
{
    public abstract class EventBase(Adapter.IPlatform platform, EventType msgType, DateTime? time = null) : IEvent
    {
        public Adapter.IPlatform Platform { get; } = platform;

        public EventType EventType { get; } = msgType;

        public DateTime Time { get; } = time ?? DateTime.UtcNow;

        public Messages? Messages { get; protected set; }
    }
}