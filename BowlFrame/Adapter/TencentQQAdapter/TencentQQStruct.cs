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
}