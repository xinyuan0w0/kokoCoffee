namespace BowlFrame.Adapter
{
    /// <summary>
    /// 适配器信息
    /// </summary>
    public readonly struct AdapterInfo
    {
        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// ID
        /// </summary>
        public string ID { get; init; }

        /// <summary>
        /// 平台名
        /// </summary>
        public string Platform { get; init; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// 属性
        /// </summary>
        public AdapterMethod Method { get; init; }
    }

    /// <summary>
    /// 平台信息
    /// </summary>
    public interface IPlatform
    {
        /// <summary>
        /// 适配器信息
        /// </summary>
        public AdapterInfo AdapterInfo { get; }

        /// <summary>
        /// 数据库记录平台ID
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// 实例ID
        /// </summary>
        public string? ConnectID { get; }
    }

    /// <summary>
    /// 适配器属性
    /// </summary>
    public readonly struct AdapterMethod
    {
        /// <summary>
        /// 交互
        /// </summary>
        public bool Interaction { get; init; }

        /// <summary>
        /// 处理
        /// </summary>
        public bool Process { get; init; }
    }
}