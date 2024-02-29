using BowlFrame.Message;

namespace BowlFrame.Target
{
    internal abstract class Group
    {
        public string UUID { get; }

        public Group(string uuid)
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