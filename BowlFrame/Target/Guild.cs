using BowlFrame.Message;

namespace BowlFrame.Target
{
    public abstract class Guild(string uuid, string fatherUUID)
    {
        public string UUID { get; } = uuid;

        public string FatherUUID { get; } = fatherUUID;

        /// <summary>
        /// 主动发送
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public abstract Task<bool> SendAsync(Messages messages);
    }
}