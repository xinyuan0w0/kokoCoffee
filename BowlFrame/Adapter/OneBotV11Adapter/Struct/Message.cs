using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BowlFrame.Adapter.OneBotV11Adapter.Struct
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    public class MessageBase : Global
    {
        [JsonProperty(PropertyName = "message_type")]
        public string SubEventType { get; set; }

        [JsonProperty(PropertyName = "message_id")]
        public string MessageID { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserID { get; set; }

        [JsonProperty(PropertyName = "message")]
        public Message[] Messages { get; set; }

        [JsonProperty(PropertyName = "raw_message")]
        public string RawMessage { get; set; }

        [JsonProperty(PropertyName = "font")]
        public int Font { get; set; }

        [JsonProperty(PropertyName = "sender")]
        public Sender Sender { get; set; }
    }

    public struct Message
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "data")]
        public JObject Data { get; set; }
    }

    public struct Sender
    {
        [JsonProperty(PropertyName = "user_id")]
        public string UserID { get; set; }

        [JsonProperty(PropertyName = "nickname")]
        public string Nickname { get; set; }

        [JsonProperty(PropertyName = "card")]
        public string Card { get; set; }

        [JsonProperty(PropertyName = "sex")]
        public string Sex { get; set; }

        [JsonProperty(PropertyName = "age")]
        public int Age { get; set; }

        [JsonProperty(PropertyName = "area")]
        public string Area { get; set; }

        [JsonProperty(PropertyName = "level")]
        public string Level { get; set; }

        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
    }

    public class GroupMessage : MessageBase
    {
        [JsonProperty(PropertyName = "group_id")]
        public string GroupID { get; set; }

        [JsonProperty(PropertyName = "anonymous")]
        public Anonymous? Anonymous { get; set; }
    }

    public struct Anonymous
    {
        [JsonProperty(PropertyName = "id")]
        public long ID { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "flag")]
        public string Flag { get; set; }
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
}