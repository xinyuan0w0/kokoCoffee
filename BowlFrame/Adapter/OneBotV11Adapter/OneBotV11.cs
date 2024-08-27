using BowlFrame.Event;
using BowlFrame.Net.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter.OneBotV11Adapter
{
    public class OneBotV11 : AdapterBase
    {
        public static readonly AdapterInfo _AdapterInfo = new()
        {
            Name = "OneBotV11",
            ID = "cn.kokobot.onebotv11",
            Platform = "TencentQQ",
            Description = "腾讯QQ第三方API",
            Method = new()
            {
                Interaction = true
            }
        };

        public override AdapterInfo AdapterInfo => _AdapterInfo;

        private readonly OneBotV11Account _account;

        private readonly OneBotV11WS _socket;

        private readonly HttpClient _httpClient;

        private bool _isConnected;

        public override bool IsStarted { get => _isConnected; }

        public override string AccountID => _account.Account;

        public IPlatform Platform { get; }

        public OneBotV11(JObject args)
        {
            //反序列化
            JsonSerializer jsonSerializer = new()
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };
            _account = args.ToObject<OneBotV11Account>(jsonSerializer);

            Platform = new TencentQQ_Common(connectID);

            Log.Debug($"创建了 {_AdapterInfo.Name} 适配器");

            _socket = new(new Uri(_account.ConnectInfo.WebSocket), _account.AccessToken);

            //监听
            _socket.ConnectedEvent += ListenConnectedEvent;
            _socket.DisconnectEvent += ListenDisconnectEvent;
            _socket.ReceiveEvent += ReceiveMsg;

            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(_account.ConnectInfo.Http),
            };

            if (_account.AccessToken is not null)
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _account.AccessToken);
        }

        ~OneBotV11()
        {
            Dispose();
        }

        public override void Dispose()
        {
            _socket.ConnectedEvent -= ListenConnectedEvent;
            _socket.DisconnectEvent -= ListenDisconnectEvent;
            _socket.ReceiveEvent -= ReceiveMsg;

            _socket.Dispose();

            Log.Debug($"释放了 {_AdapterInfo.Name}({connectID}) 适配器");
            GC.SuppressFinalize(this);
        }

        public override async Task<bool> Restart()
        {
            bool result = await Stop();
            result = result && await Start();
            return result;
        }

        public override async Task<bool> Start()
        {
            try
            {
                await _socket.ConnectAsync();
            }
            catch (Exception e)
            {
                Log.Warn(e);
                return false;
            }

            return true;
        }

        public override async Task<bool> Stop()
        {
            await _socket.CloseAsync();
            return true;
        }

        private void ListenConnectedEvent(WSClient client)
        {
            OnConnected();
        }

        private async void ListenDisconnectEvent(WSClient client, WebSocketCloseStatus closeStatus)
        {
            if (!_isConnected)
                return;

            int retryInterval = 2000;

            Log.Info($"{client.GetType().Name}({client.ConnectID}) 等待 {retryInterval / 1000}s 后重连");
            await Task.Delay(retryInterval);

            if (client.ConnectAsync().Result)
                return;

            OnDisconnect();
        }

        protected void OnConnected()
        {
            _isConnected = true;
            OnConnected(connectID ?? "Null", _AdapterInfo);
        }

        protected void OnDisconnect(Exception? exception = null)
        {
            _isConnected = false;
            OnDisconnect(connectID ?? "Null", _AdapterInfo, exception);
        }

        protected void OnError(Exception exception)
        {
            OnError(connectID ?? "Null", _AdapterInfo, exception);
        }

        public async Task WebSocketSend(string text, CancellationToken cancellationToken = default) => await _socket.SendAsync(text, cancellationToken);

        public async Task<HttpResponseMessage> HttpSend(string text, string? path = null, CancellationToken cancellationToken = default) => await _httpClient.PostAsync(path, new StringContent(text, Encoding.UTF8, "application/json"), cancellationToken);

        //TOOD: 拆！
        internal void ReceiveMsg(WSClient client, byte[] bytes, WebSocketReceiveResult receiveResult)
        {
            try
            {
                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    //转换为文本
                    string receivedMessage = Encoding.UTF8.GetString(bytes);
                    JObject value = JObject.Parse(receivedMessage);

                    Log.Trace(receivedMessage);

                    string? type = (string?)value["post_type"];

                    switch (type)
                    {
                        //消息
                        case "message":
                            string? msgType = (string?)value["message_type"];
                            string? subType = (string?)value["sub_type"];
                            IEvent? @event = null;

                            if (msgType == "group" && subType == "normal")
                            {
                                @event = new Event.Message.GroupMessage(this, value.ToObject<Struct.GroupMessage>() ?? throw new NullReferenceException());
                            }
                            else if (msgType == "private" && subType == "friend")
                            {
                                @event = new Event.Message.PrivateMessage(this, value.ToObject<Struct.MessageBase>() ?? throw new NullReferenceException());
                            }

                            if (@event is not null)
                                AdapterManager.OnBroadcastEvent(@event, _AdapterInfo);
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
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}