using Newtonsoft.Json;

namespace BowlFrame.Adapter.TencentQQAdapter.Struct
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    public class AT_MESSAGE_CREATE : WebSocket
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
        public AT_MESSAGE_CREATE_Data Data { get; set; }
    }

    public class AT_MESSAGE_CREATE_Data : GuildMsgData
    {
        /// <summary>
        /// 频道id
        /// </summary>
        [JsonProperty(PropertyName = "channel_id")]
        public string ChannelID { get; set; }

        /// <summary>
        /// 子频道id
        /// </summary>
        [JsonProperty(PropertyName = "guild_id")]
        public string GuildID { get; set; }

        /// <summary>
        /// 提及用户
        /// </summary>
        [JsonProperty(PropertyName = "mentions")]
        public ChannelAuthor[] Mentions { get; set; }
    }

    public class DIRECT_MESSAGE_CREATE : WebSocket
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
        public DIRECT_MESSAGE_CREATE_Data Data { get; set; }
    }

    public class DIRECT_MESSAGE_CREATE_Data : GuildMsgData
    {
        /// <summary>
        /// 频道id
        /// </summary>
        [JsonProperty(PropertyName = "channel_id")]
        public string ChannelID { get; set; }

        /// <summary>
        /// 子频道id
        /// </summary>
        [JsonProperty(PropertyName = "guild_id")]
        public string GuildID { get; set; }

        /// <summary>
        /// 来源子频道ID(?)
        /// </summary>
        [JsonProperty(PropertyName = "src_guild_id")]
        public string SrcGuildID { get; set; }
    }

    public class GuildMember : Member
    {
        /// <summary>
        /// 频道昵称
        /// </summary>
        [JsonProperty(PropertyName = "nick")]
        public string Nick { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        [JsonProperty(PropertyName = "roles")]
        public string[] Roles { get; set; }
    }

    public class GuildMsgData : MessageData
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        [JsonProperty(PropertyName = "author")]
        public ChannelAuthor Author { get; set; }

        /// <summary>
        /// 用户添加信息
        /// </summary>
        [JsonProperty(PropertyName = "member")]
        public GuildMember Member { get; set; }

        /// <summary>
        /// 消息序列
        /// </summary>
        [JsonProperty(PropertyName = "seq")]
        public int Seq { get; set; }

        /// <summary>
        /// 消息在频道中的序列
        /// </summary>
        [JsonProperty(PropertyName = "seq_in_channel")]
        public int ChannelSeq { get; set; }

        /// <summary>
        /// 是否为频道私信(多此一举)
        /// </summary>
        [JsonIgnore]
        public bool IsPrivate { get; set; } = false;
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
}