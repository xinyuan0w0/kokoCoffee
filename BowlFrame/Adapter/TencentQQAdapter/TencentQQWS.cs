using BowlFrame.Net.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Timers;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter.TencentQQAdapter
{
    internal class TencentQQWS : WSClient
    {
        private readonly TencentQQ tencentQQ;
        private readonly TencentQQAccount account;
        private GetAppAccessToken appAccessToken;

        private readonly int connectID; //分片ID
        private readonly int connectCount; //分片数

        //连接信息
        private string? sessionID; //连接Session

        private bool? isConnectSuccessed = false;

        /// <summary>
        /// Null 不允许再发起重连
        /// </summary>
        public bool? IsConnectSuccessed { get => isConnectSuccessed; }

        private short retryCount = 0;

        private string? id; //机器人ID
        private string? nickname; //机器人昵称

        //private int heartbeat; //心跳包时长
        private int? s; //消息编号

        private readonly System.Timers.Timer heartbeatTimer = new();

        public TencentQQWS(TencentQQ tencentQQ, Uri uri, int connectID, int connectCount, TencentQQAccount account, ref GetAppAccessToken appAccessToken) : base(uri)
        {
            this.connectID = connectID;
            this.connectCount = connectCount;
            this.account = account;
            this.appAccessToken = appAccessToken;
            this.tencentQQ = tencentQQ;

            DisconnectEvent += ListenDisconnectEvent;
            ReceiveEvent += ReceiveMsg;
            tencentQQ.ReflushAppAccessTokenEvent += ListenReflushAppAccessTokenEvent;

            Log.Debug($"创建了 TencentQQWS WSClient");

            //初始化定时器
            heartbeatTimer.Elapsed += HeartbeatCallback;
            heartbeatTimer.AutoReset = false;
        }

        ~TencentQQWS()
        {
            Dispose();
        }

        public override void Dispose()
        {
            heartbeatTimer.Dispose();

            DisconnectEvent -= ListenDisconnectEvent;
            ReceiveEvent -= ReceiveMsg;
            tencentQQ.ReflushAppAccessTokenEvent -= ListenReflushAppAccessTokenEvent;

            base.Dispose();
            GC.SuppressFinalize(this);
        }

        internal async void ReceiveMsg(WSClient client, byte[] bytes, WebSocketReceiveResult receiveResult)
        {
            if (receiveResult.MessageType == WebSocketMessageType.Text)
            {
                //转换为文本
                string receivedMessage = Encoding.UTF8.GetString(bytes);
                JObject value = JObject.Parse(receivedMessage);

                Log.Trace(receivedMessage);

                short op = (short?)value["op"] ?? 9;

                switch (op)
                {
                    //Dispatch 服务端进行消息推送
                    case 0:
                        if ((int?)value["s"] <= s) return; //丢弃重复消息
                        s = (int?)value["s"];
                        Dispatch(value);
                        break;
                    //Reconnect	服务端通知客户端重新连接
                    case 7:
                        Log.Info("服务器通知重新连接");
                        break;
                    //Invalid Session 当 identify 或 resume 的时候，如果参数有错，服务端会返回该消息
                    case 9:
                        Log.Warn("认证失败");
                        retryCount++;

                        //失败次数过多,放弃登录
                        if (retryCount > 5)
                            isConnectSuccessed = null;
                        else
                            isConnectSuccessed = false;

                        //重连失败的话重新发起认证
                        //if (isConnectSuccessed)
                        //{
                        //    isConnectSuccessed = false;
                        //    _ = ReconnectAsync();
                        //}
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

        private async void Dispatch(JObject value)
        {
            string? t = (string?)value["t"];

            switch (t)
            {
                case null:
                    break;

                case "READY":
                    await SendHeartbeat();
                    isConnectSuccessed = true;
                    retryCount = 0;
                    sessionID = (string?)value["d"]?["session_id"];
                    id = (string?)value["d"]?["user"]?["id"];
                    nickname = (string?)value["d"]?["user"]?["username"];
                    Log.Info($"登录成功,当前账号 {nickname}({id})");
                    break;

                case "RESUMED":
                    await SendHeartbeat();
                    Log.Info($"重连成功");
                    break;

                default:
                    Log.Debug($"未使用的事件 {t}");
                    break;
            }
        }

        private async Task Hello(JObject value)
        {
            int heartbeat = (int?)value["d"]?["heartbeat_interval"] ?? 300000;
            heartbeatTimer.Interval = heartbeat;
            Log.Debug($"心跳包间隔: {heartbeat}");
            object data;
            if (isConnectSuccessed == true)
            {
                // OpCode 6 Resume
                data = new
                {
                    op = 6,
                    d = new
                    {
                        token = $"QQBot {appAccessToken.AccessToken}",
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
                        token = $"QQBot {appAccessToken.AccessToken}",
                        intents = account.Intents,
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

        private void ListenDisconnectEvent(WSClient client, WebSocketCloseStatus closeStatus)
        {
            heartbeatTimer.Stop();

            //交给上一层重连
            //if (isConnectSuccessed)
            //{
            //    _ = ReconnectAsync();
            //}
        }

        private void ListenReflushAppAccessTokenEvent(GetAppAccessToken appAccessToken)
        {
            this.appAccessToken = appAccessToken;
        }
    }
}