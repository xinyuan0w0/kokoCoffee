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

        /// <summary>
        /// 被动发送
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public abstract Task<bool> SendAsync(Messages messages);
    }
}