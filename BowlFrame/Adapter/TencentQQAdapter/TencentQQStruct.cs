using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.TencentQQAdapter
{
    internal struct TencentQQAccount
    {
        public string Account { get; set; }
        public string AppID { get; set; }
        public string Token { get; set; }
        public string AppSecret { get; set; }
        public bool Sandbox { get; set; }
    }

    internal struct GetAppAccessToken
    {
        /// <summary>
        /// 获取到的凭证。
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// 凭证有效时间，单位：秒 目前是7200秒之内的值
        /// </summary>
        public int expires_in { get; set; }
    }

    internal struct GatewayWithShards
    {
        /// <summary>
        /// WebSocket 的连接地址
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// 建议的 shard 数
        /// </summary>
        public int shards { get; set; }

        /// <summary>
        /// 创建 Session 限制信息
        /// </summary>
        public SessionStartLimit session_start_limit { get; set; }
    }

    internal struct SessionStartLimit
    {
        /// <summary>
        /// 每 24 小时可创建 Session 数
        /// </summary>
        public int total { get; set; }
        /// <summary>
        /// 目前还可以创建的 Session 数
        /// </summary>
        public int remaining { get; set; }
        /// <summary>
        /// 重置计数的剩余时间(ms)
        /// </summary>
        public int reset_after { get; set; }
        /// <summary>
        /// 每 5s 可以创建的 Session 数
        /// </summary>
        public int max_concurrency { get; set; }
    }
}