using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.TencentQQAdapter.Struct
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

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

    public struct DIRECT_MESSAGE_CREATE_Data
    {
        /// <summary>
        /// 消息id
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        [JsonProperty(PropertyName = "author")]
        public ChannelAuthor Author { get; set; }

        /// <summary>
        /// 用户添加信息
        /// </summary>
        [JsonProperty(PropertyName = "member")]
        public Member Member { get; set; }

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
        /// 文本消息
        /// </summary>
        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }

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
        /// 来源子频道ID(?)
        /// </summary>
        [JsonProperty(PropertyName = "src_guild_id")]
        public string SrcGuildID { get; set; }

        /// <summary>
        /// 附件信息
        /// </summary>
        [JsonProperty(PropertyName = "attachments")]
        public Attachments[]? Attachments { get; set; }

        /// <summary>
        /// 是否为频道私信(多此一举)
        /// </summary>
        [JsonProperty(PropertyName = "direct_message")]
        public bool IsDirectMessage { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
}