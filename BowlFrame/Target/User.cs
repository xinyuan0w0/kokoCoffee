using BowlFrame.Adapter;
using BowlFrame.Message;

namespace BowlFrame.Target
{
    public abstract class User(string uuid) : ITarget
    {
        public string UUID { get; } = uuid;

        public abstract string ID { get; }

        public abstract IPlatform Platform { get; }
        
        public abstract string Nickname { get; }

        public abstract byte[] Avatar { get; }     

        /// <summary>
        /// 主动发送
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public abstract Task<bool> SendAsync(Messages messages);
    }
}