using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BowlFrame.Adapter.OneBotV11Adapter.Struct
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    internal class MetaBase : Global
    {
        [JsonProperty(PropertyName = "meta_event_type")]
        public string MetaEventType { get; set; }
    }

    internal class HealthCycle : MetaBase
    {
        [JsonProperty(PropertyName = "sub_type")]
        public string SubType { get; set; }
    }

    internal class Heartbeat : MetaBase
    {
        [JsonProperty(PropertyName = "status")]
        public JObject Status { get; set; }

        [JsonProperty(PropertyName = "interval")]
        public long Interval { get; set; }
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
}