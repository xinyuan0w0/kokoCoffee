using BowlFrame.Event;

namespace BowlFrame.Adapter.CocoaPlugin
{
    internal class CocoaEvent
    {
        public delegate void CocoaEventHandler(IEvent @Event);

        public delegate void CocoaBroadcastHandler(IEvent @Event, ICocoaPlugin plugin);

        public event CocoaEventHandler? MainEvent;

        public event CocoaBroadcastHandler? Broadcast;

        public event CocoaEventHandler? MessageEvent;

        public event CocoaEventHandler? PrivateMessage;

        public event CocoaEventHandler? GroupMessage;

        public event CocoaEventHandler? GuildMessage;

        public event CocoaEventHandler? ChannelMessage;

        public event CocoaEventHandler? GuildPrivateMessage;

        public event CocoaEventHandler? PostMessage;

        public event CocoaEventHandler? MetaEvent;

        public void OnMainEvent(IEvent @event) => MainEvent?.Invoke(@event);
    }
}