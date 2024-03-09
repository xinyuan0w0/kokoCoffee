using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public struct GROUP_AT_MESSAGE_CREATE_Data
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
        public GroupAuthor Author { get; set; }

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

        /// <summary>
        /// 文本消息
        /// </summary>
        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }

        /// <summary>
        /// 附件信息
        /// </summary>
        [JsonProperty(PropertyName = "attachments")]
        public Attachments[]? Attachments { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
}