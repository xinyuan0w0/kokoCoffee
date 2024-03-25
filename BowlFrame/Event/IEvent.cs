using BowlFrame.Message;

namespace BowlFrame.Event
{
    public interface IEvent
    {
        public Adapter.IPlatform Platform { get; }

        public EventType EventType { get; }

        public SourceType SourceType { get; }

        public Messages? Messages { get; }

        public DateTime Time { get; }
    }
}