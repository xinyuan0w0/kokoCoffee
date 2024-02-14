using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.OneBotV11Adapter
{
    internal struct OneBotV11Account
    {
        public string Account { get; set; }
        public string? AccessToken { get; set; }
        public OneBotV11ConnectInfo ConnectInfo { get; set; }
    }

    internal struct OneBotV11ConnectInfo
    {
        public string WebSocket { get; set; }
        public string Http { get; set; }
    }
}