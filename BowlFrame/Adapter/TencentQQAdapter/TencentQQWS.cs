using BowlFrame.Net.WebSocket;
using static BowlFrame.Tools.Logger;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.TencentQQAdapter
{
    internal class TencentQQWS : WSClient
    {
        public TencentQQWS(Uri uri, int id) : base(uri)
        {
            ReceiveEvent += ReceiveMsg;
        }

        internal async void ReceiveMsg(WSClient client, byte[] bytes, WebSocketReceiveResult receiveResult)
        {
        }
    }
}