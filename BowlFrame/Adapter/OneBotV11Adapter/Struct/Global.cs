using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.OneBotV11Adapter.Struct
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    public class Global
    {
        [JsonProperty(PropertyName = "self_id")]
        public long BotID { get; set; }

        [JsonProperty(PropertyName = "post_type")]
        public string EventType { get; set; }

        [JsonProperty(PropertyName = "time")]
        public long Timestamp { get; set; }
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
}