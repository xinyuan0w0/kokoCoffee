using BowlFrame.Message;

namespace BowlFrame.Target
{
    public abstract class Guild
    {
        public string UUID { get; }

        public string FatherUUID { get; }

        public Guild(string uuid, string fatherUUID)
        {
            UUID = uuid;
            FatherUUID = fatherUUID;
        }

        /// <summary>
        /// 主动发送
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public abstract Task<bool> SendAsync(Messages messages);
    }
}