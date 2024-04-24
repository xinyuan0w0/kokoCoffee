using Newtonsoft.Json;

namespace BowlFrame.Adapter.OneBotV11Adapter.Struct
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    internal class NoticeBase : Global
    {
        [JsonProperty(PropertyName = "notice_type")]
        public string NoticeEventType { get; set; }
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
}