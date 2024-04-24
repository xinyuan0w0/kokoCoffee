using BowlFrame.Event;

namespace BowlFrame.Adapter.CocoaPlugin.Event
{
    internal class BroadcastEvent : EventBase
    {
        public BroadcastEvent(IPlatform platform) : base(platform, EventType.Event)
        {
            Messages = new();
        }
    }
}