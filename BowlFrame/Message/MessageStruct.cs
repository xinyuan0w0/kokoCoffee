namespace BowlFrame.Message
{
    public enum MetaType
    {
        /// <summary>
        /// 通用字段
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 拓展字段(用于适配器传递额支持的内容)
        /// </summary>
        Extra = 1,

        /// <summary>
        /// 自定义字段(用于插件传递内容)
        /// </summary>
        Custom = 2
    }
}