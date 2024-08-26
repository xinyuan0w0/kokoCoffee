using Newtonsoft.Json;

namespace BowlFrame.Adapter.TencentQQAdapter
{
    internal struct TencentQQAccount
    {
        public string Account { get; set; }
        public string AppID { get; set; }
        public string Token { get; set; }
        public string AppSecret { get; set; }
        public bool Sandbox { get; set; }
        public int Intents { get; set; }
    }

    internal struct GetAppAccessToken
    {
        /// <summary>
        /// 获取到的凭证。
        /// </summary>
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// 凭证有效时间，单位：秒 目前是7200秒之内的值
        /// </summary>
        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }
    }

    internal struct GatewayWithShards
    {
        /// <summary>
        /// WebSocket 的连接地址
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        /// <summary>
        /// 建议的 shard 数
        /// </summary>
        [JsonProperty(PropertyName = "shards")]
        public int Shards { get; set; }

        /// <summary>
        /// 创建 Session 限制信息
        /// </summary>
        [JsonProperty(PropertyName = "session_start_limit")]
        public SessionStartLimit SessionStartLimit { get; set; }
    }

    internal struct SessionStartLimit
    {
        /// <summary>
        /// 每 24 小时可创建 Session 数
        /// </summary>
        [JsonProperty(PropertyName = "total")]
        public int Total { get; set; }

        /// <summary>
        /// 目前还可以创建的 Session 数
        /// </summary>
        [JsonProperty(PropertyName = "remaining")]
        public int Remaining { get; set; }

        /// <summary>
        /// 重置计数的剩余时间(ms)
        /// </summary>
        [JsonProperty(PropertyName = "reset_after")]
        public int ResetAfter { get; set; }

        /// <summary>
        /// 每 5s 可以创建的 Session 数
        /// </summary>
        [JsonProperty(PropertyName = "max_concurrency")]
        public int MaxConcurrency { get; set; }
    }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    public abstract class WebSocket
    {
        /// <summary>
        /// 操作码
        /// </summary>
        [JsonProperty(PropertyName = "op")]
        public int OpCode { get; set; }

        /// <summary>
        /// 序列号，标志消息的唯一性
        /// </summary>
        [JsonProperty(PropertyName = "s")]
        public int Sequence { get; set; }

        /// <summary>
        /// 事件类型
        /// </summary>
        [JsonProperty(PropertyName = "t")]
        public string Type { get; set; }
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    internal struct Files
    {
        /// <summary>
        /// 文件 ID
        /// </summary>
        [JsonProperty(PropertyName = "file_uuid")]
        public string FileUUID { get; set; }

        /// <summary>
        /// 文件信息，用于发消息接口的 media 字段使用
        /// </summary>
        [JsonProperty(PropertyName = "file_info")]
        public string FileInfo { get; set; }

        /// <summary>
        /// 有效期，表示剩余多少秒到期，到期后 file_info 失效，当等于 0 时，表示可长期使用
        /// </summary>
        [JsonProperty(PropertyName = "ttl")]
        public int TTL { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        [JsonIgnore]
        public long TTLTime { get; set; }
    }

    public struct DMS
    {
        //私信会话关联的频道 id
        [JsonProperty(PropertyName = "guild_id")]
        public string GuildID { get; set; }

        //私信会话关联的子频道 id
        [JsonProperty(PropertyName = "channel_id")]
        public string ChannelID { get; set; }

        //创建私信会话时间戳
        [JsonProperty(PropertyName = "create_time")]
        public string Timestamp { get; set; }
    }

    public class TencentQQ_Offical_Common(string? connectID) : IPlatform
    {
        public AdapterInfo AdapterInfo { get; } = TencentQQ._AdapterInfo;

        public string ID { get; } = "TencentQQ_Offical_Common";

        public string? ConnectID { get; } = connectID;
    }

    public class TencentQQ_Offical_Guild(string? connectID) : IPlatform
    {
        public AdapterInfo AdapterInfo { get; } = TencentQQ._AdapterInfo;

        public string ID { get; } = "TencentQQ_Offical_Guild";

        public string? ConnectID { get; } = connectID;
    }
}