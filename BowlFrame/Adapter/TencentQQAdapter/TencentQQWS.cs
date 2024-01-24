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
using System.Timers;

namespace BowlFrame.Adapter.TencentQQAdapter
{
    internal class TencentQQWS : WSClient
    {
        internal TencentQQAccount account;
        internal GetAppAccessToken appAccessToken;

        private int connectID; //分片ID
        private int connectCount; //分片数

        //连接信息
        private string? sessionID; //连接Session

        private bool isConnectSuccessed;

        private string? id; //机器人ID
        private string? nickname; //机器人昵称

        //private int heartbeat; //心跳包时长
        private int? s; //消息编号

        private readonly System.Timers.Timer heartbeatTimer = new();

        public TencentQQWS(Uri uri, int connectID, int connectCount, TencentQQAccount account, GetAppAccessToken appAccessToken) : base(uri)
        {
            this.connectID = connectID;
            this.connectCount = connectCount;
            this.account = account;
            this.appAccessToken = appAccessToken;
            DisconnectEvent += ListenDisconnectEvent;
            ReceiveEvent += ReceiveMsg;

            Log.Debug($"创建了 TencentQQWS WSClient");

            //初始化定时器
            heartbeatTimer.Elapsed += HeartbeatCallback;
            heartbeatTimer.AutoReset = false;
        }

        internal async void ReceiveMsg(WSClient client, byte[] bytes, WebSocketReceiveResult receiveResult)
        {
            if (receiveResult.MessageType == WebSocketMessageType.Text)
            {
                //转换为文本
                string receivedMessage = Encoding.UTF8.GetString(bytes);
                JObject value = JObject.Parse(receivedMessage);

                Log.Debug(receivedMessage);

                short op = (short?)value["op"] ?? 9;

                switch (op)
                {
                    //Dispatch 服务端进行消息推送
                    case 0:
                        if ((int?)value["s"] <= s) return; //丢弃重复消息
                        s = (int?)value["s"];
                        string t = (string?)value["t"] ?? "";
                        Dispatch(value);
                        break;
                    //Reconnect	服务端通知客户端重新连接
                    case 7:
                        Log.Info("服务器通知重新连接");
                        break;
                    //Invalid Session 当 identify 或 resume 的时候，如果参数有错，服务端会返回该消息
                    case 9:
                        Log.Warn("认证失败");
                        isConnectSuccessed = false;
                        await CloseAsync();
                        break;
                    //Hello 当客户端与网关建立 ws 连接之后，网关下发的第一条消息
                    case 10:
                        await Hello(value);
                        break;
                    //Heartbeat ACK	当发送心跳成功之后，就会收到该消息
                    case 11:
                        heartbeatTimer.Start();
                        break;
                    //HTTP Callback ACK	仅用于 http 回调模式的回包，代表机器人收到了平台推送的数据
                    case 12:
                        break;

                    default:
                        Log.Warn($"收到了未知的OpCode {op}");
                        break;
                }
            }
        }

        internal async void Dispatch(JObject value)
        {
            string? t = (string?)value["t"];

            switch (t)
            {
                case null:
                    break;

                case "READY":
                    await SendHeartbeat();
                    isConnectSuccessed = true;
                    sessionID = (string?)value["d"]?["session_id"];
                    id = (string?)value["d"]?["user"]?["id"];
                    nickname = (string?)value["d"]?["user"]?["username"];
                    Log.Info($"登录成功, 当前账号 {nickname}({id})");
                    break;

                case "RESUMED":
                    await SendHeartbeat();
                    Log.Info($"重连成功");
                    break;

                default:
                    Log.Trace($"未使用的事件 {t}");
                    break;
            }
        }

        internal async Task Hello(JObject value)
        {
            int heartbeat = (int?)value["d"]?["heartbeat_interval"] ?? 300000;
            heartbeatTimer.Interval = heartbeat;
            Log.Trace($"心跳包间隔: {heartbeat}");

            var data = new object();
            if (isConnectSuccessed)
            {
                // OpCode 6 Resume
                data = new
                {
                    op = 6,
                    d = new
                    {
                        token = $"QQBot {appAccessToken.access_token}",
                        session_id = sessionID,
                        seq = s,
                    }
                };
            }
            else
            {
                //重置消息ID
                s = null;
                // OpCode 2 Identify
                data = new
                {
                    op = 2,
                    d = new
                    {
                        token = $"QQBot {appAccessToken.access_token}",
                        intents = 1107301379, //0|1 << 0|1 << 1|1 << 10|1 << 12|1 << 25|1 << 30
                        shard = new int[] { connectID, connectCount }, //切片数
                        properties = new { }
                    }
                };
            }
            await SendAsync(JsonConvert.SerializeObject(data));
        }

        private async void HeartbeatCallback(object? sender, ElapsedEventArgs e)
        {
            await SendHeartbeat();
        }

        private async Task SendHeartbeat()
        {
            var data = new
            {
                op = 1,
                d = s,
            };
            await SendAsync(JsonConvert.SerializeObject(data));
        }

        private async void ListenDisconnectEvent(WSClient client, WebSocketCloseStatus closeStatus)
        {
            if (isConnectSuccessed)
            {
                while (socket.State != WebSocketState.Aborted)
                    Thread.Sleep(50);
                await ConnectAsync();
            }
        }

        public override void Dispose()
        {
            heartbeatTimer.Dispose();

            DisconnectEvent -= ListenDisconnectEvent;
            ReceiveEvent -= ReceiveMsg;

            base.Dispose();
        }
    }
}