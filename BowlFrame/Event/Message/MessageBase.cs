using BowlFrame.Adapter;
using BowlFrame.Message;

namespace BowlFrame.Event.Message
{
    public abstract class MessageBase(IPlatform platform, SourceType sourceType) : TargetEventBase(platform, sourceType, EventType.Message)
    {
        public abstract string RawMessage { get; }

        /// <summary>
        /// 被动发送
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public abstract Task<bool> SendAsync(Messages messages);
    }
}