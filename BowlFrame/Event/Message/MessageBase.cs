using BowlFrame.Adapter;
using BowlFrame.Message;

namespace BowlFrame.Event.Message
{
    internal abstract class MessageBase : EventBase
    {
        public abstract string RawMessage { get; }

        public abstract string Target { get; }

        public MessageBase(IPlatform platform, SourceType sourceType) : base(platform, MsgType.Message, sourceType)
        {
        }

        public abstract Task<bool> SendAsync(Messages messages);
    }
}