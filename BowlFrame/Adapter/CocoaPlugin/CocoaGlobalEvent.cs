using BowlFrame.Event;
using BowlFrame.Event.Message;

namespace BowlFrame.Adapter.CocoaPlugin
{
    internal class CocoaGlobalEvent
    {
        public delegate void CocoaEventHandler(IEvent @event);

        public delegate void CocoaMessageEventHandler(MessageBase message);

        public delegate void CocoaBroadcastHandler(IEvent @event, ICocoaPlugin plugin);

        public event CocoaEventHandler? MainEvent;

        public event CocoaBroadcastHandler? Broadcast;

        public event CocoaMessageEventHandler? MessageEvent;

        public event CocoaMessageEventHandler? PrivateMessage;

        public event CocoaMessageEventHandler? GroupMessage;

        public event CocoaMessageEventHandler? ChannelMessage;

        public event CocoaMessageEventHandler? PostMessage;

        public event CocoaEventHandler? MetaEvent;

        public event CocoaEventHandler? Event;

        public void OnMainEvent(IEvent @event) => MainEvent?.Invoke(@event);

        public void OnBroadcast(IEvent @event, ICocoaPlugin plugin) => Broadcast?.Invoke(@event, plugin);

        public void OnMessageEvent(MessageBase @event) => MessageEvent?.Invoke(@event);

        public void OnPrivateMessage(PrivateMessage @event) => PrivateMessage?.Invoke(@event);

        public void OnGroupMessage(GroupMessage @event) => GroupMessage?.Invoke(@event);

        public void OnChannelMessage(ChannelMessage @event) => ChannelMessage?.Invoke(@event);

        public void OnPostMessage(MessageBase @event) => PostMessage?.Invoke(@event);

        public void OnMetaEvent(IEvent @event) => MetaEvent?.Invoke(@event);

        public void OnEvent(IEvent @event) => Event?.Invoke(@event);
    }
}