using BowlFrame.Adapter;
using BowlFrame.Message;

namespace BowlFrame.Target
{
    public abstract class Group(string uuid) : ITarget
    {
        public string UUID { get; } = uuid;

        public abstract string ID { get; }

        public abstract IPlatform Platform { get; }

        /// <summary>
        /// 主动发送
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public abstract Task<bool> SendAsync(Messages messages);
    }
}