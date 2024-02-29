namespace BowlFrame.Adapter
{
    public struct AdapterInfo
    {
        public string Name;
        public string ID;
        public string Platform;
        public string Description;
    }

    public interface IPlatform
    {
        /// <summary>
        /// 适配器信息
        /// </summary>
        public AdapterInfo AdapterInfo { get; }

        /// <summary>
        /// 唯一平台ID
        /// </summary>
        public string ID { get; }
    }
}