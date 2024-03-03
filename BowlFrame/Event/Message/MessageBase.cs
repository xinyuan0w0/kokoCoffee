using BowlFrame.Adapter;
using BowlFrame.Message;

namespace BowlFrame.Event.Message
{
    public abstract class MessageBase(IPlatform platform, SourceType sourceType) : EventBase(platform, MsgType.Message, sourceType)
    {
        public abstract string RawMessage { get; }

        public abstract string Target { get; }

        /// <summary>
        /// 被动发送
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public abstract Task<bool> SendAsync(Messages messages);
    }
}