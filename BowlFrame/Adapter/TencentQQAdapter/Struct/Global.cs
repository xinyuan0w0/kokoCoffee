using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.TencentQQAdapter.Struct
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    public struct GroupAuthor
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        /// <summary>
        /// 用户openid
        /// </summary>
        [JsonProperty(PropertyName = "member_openid")]
        public string OpenID { get; set; }
    }

    public struct ChannelAuthor()
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        /// <summary>
        /// 用户是否为Bot
        /// </summary>
        [JsonProperty(PropertyName = "bot")]
        public bool Bot { get; set; } = false;

        /// <summary>
        /// 用户昵称
        /// </summary>
        [JsonProperty(PropertyName = "username")]
        public string UserName { get; set; }

        /// <summary>
        /// 用户头像Url
        /// </summary>
        [JsonProperty(PropertyName = "avatar")]
        public string Avatar { get; set; }
    }

    public struct Attachments
    {
        /// <summary>
        /// 文件类型
        /// </summary>
        [JsonProperty(PropertyName = "content_type")]
        public string MimeType { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        [JsonProperty(PropertyName = "filename")]
        public string FileName { get; set; }

        /// <summary>
        /// ID(仅频道)
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string? ID { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        [JsonProperty(PropertyName = "height")]
        public int? Height { get; set; }

        /// <summary>
        /// 宽度
        /// </summary>
        [JsonProperty(PropertyName = "width")]
        public int? Width { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        [JsonProperty(PropertyName = "size")]
        public int Size { get; set; }

        /// <summary>
        /// 文件Url
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }

    public class Member
    {
        /// <summary>
        /// 加入时间
        /// </summary>
        [JsonProperty(PropertyName = "joined_at")]
        public DateTime JoinTime { get; set; }
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
}