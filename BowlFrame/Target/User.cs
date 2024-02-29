using BowlFrame.Message;

namespace BowlFrame.Target
{
    public abstract class User
    {
        public string UUID { get; }

        public abstract string Nickname { get; }

        public abstract byte[] Avatar { get; }

        public User(string uuid)
        {
            UUID = uuid;
        }

        /// <summary>
        /// 主动发送
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public abstract Task<bool> SendAsync(Messages messages);
    }
}