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
        /// 凭证有效时间，单位：秒。目前是7200秒之内的值。
        /// </summary>
        public int expires_in { get; set; }
    }
}