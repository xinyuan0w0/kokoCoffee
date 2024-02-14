using BowlFrame.Net.WebSocket;
using static BowlFrame.Tools.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using BowlFrame.Adapter.TencentQQAdapter;

namespace BowlFrame.Adapter.OneBotV11Adapter
{
    internal class OneBotV11WS : WSClient
    {
        private readonly string? accessToken;

        public OneBotV11WS(Uri uri, string? accessToken) : base(uri)
        {
            this.accessToken = accessToken;
            ReceiveEvent += ReceiveMsg;

            Log.Debug($"创建了 OneBotV11WS WSClient");
        }

        ~OneBotV11WS()
        {
            Dispose();
        }

        public override void Dispose()
        {
            ReceiveEvent -= ReceiveMsg;

            base.Dispose();
            GC.SuppressFinalize(this);
        }

        protected override void CreateNewSocket()
        {
            base.CreateNewSocket();
            if (accessToken is not null)
                socket?.Options.SetRequestHeader("Authorization", "Bearer " + accessToken);
        }

        internal void ReceiveMsg(WSClient client, byte[] bytes, WebSocketReceiveResult receiveResult)
        {
            if (receiveResult.MessageType == WebSocketMessageType.Text)
            {
                //转换为文本
                string receivedMessage = Encoding.UTF8.GetString(bytes);
                JObject value = JObject.Parse(receivedMessage);

                Log.Debug(receivedMessage);

                string? type = (string?)value["post_type"];

                switch (type)
                {
                    //消息
                    case "message":
                        break;
                    //消息发送
                    case "message_sent":
                        break;
                    //请求
                    case "request":
                        break;
                    //通知
                    case "notice":
                        break;
                    //元事件
                    case "meta_event":
                        break;

                    default:
                        Log.Warn($"收到了未知的Type {type}");
                        break;
                }
            }
        }
    }
}