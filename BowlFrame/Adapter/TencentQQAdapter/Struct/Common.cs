using Newtonsoft.Json;

namespace BowlFrame.Adapter.TencentQQAdapter.Struct
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    public class GROUP_AT_MESSAGE_CREATE : WebSocket
    {
        /// <summary>
        /// 消息id
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        /// <summary>
        /// 数据结构
        /// </summary>
        [JsonProperty(PropertyName = "d")]
        public GROUP_AT_MESSAGE_CREATE_Data Data { get; set; }
    }

    public class GROUP_AT_MESSAGE_CREATE_Data : CommonMsgData
    {
        /// <summary>
        /// 群id
        /// </summary>
        [JsonProperty(PropertyName = "group_id")]
        public string GroupID { get; set; }

        /// <summary>
        ///  群openid
        /// </summary>
        [JsonProperty(PropertyName = "group_openid")]
        public string GroupOpenID { get; set; }
    }

    public class C2C_MESSAGE_CREATE : WebSocket
    {
        /// <summary>
        /// 消息id
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        /// <summary>
        /// 数据结构
        /// </summary>
        [JsonProperty(PropertyName = "d")]
        public CommonMsgData Data { get; set; }
    }

    public class CommonMsgData : MessageData
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        [JsonProperty(PropertyName = "author")]
        public GroupAuthor Author { get; set; }
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
}