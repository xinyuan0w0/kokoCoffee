using BowlFrame.Adapter.TencentQQAdapter.Event.Message;
using BowlFrame.Adapter.TencentQQAdapter.Struct;
using BowlFrame.Net.WebSocket;
using BowlFrame.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter.TencentQQAdapter
{
    public class TencentQQ : AdapterBase
    {
        public new static readonly AdapterInfo _AdapterInfo = new()
        {
            Name = "TencentQQ_Offical",
            ID = "cn.kokobot.tencentqqapi",
            Platform = "TencentQQ",
            Description = "腾讯QQ官方API",
            Method = new()
            {
                Interaction = true
            }
        };

        public readonly Uri BaseUrl;
        private readonly System.Timers.Timer _accessTokenTimer = new();
        private readonly HttpClient _httpClient = new();
        private readonly WSManagerEx manager = new();

        private TencentQQAccount account;
        private GetAppAccessToken? appAccessToken;
        private bool isConnected;

        public TencentQQ(JObject args)
        {
            //反序列化
            JsonSerializer jsonSerializer = new()
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };
            account = args.ToObject<TencentQQAccount>(jsonSerializer);

            Platform = [new TencentQQ_Offical_Common(connectID), new TencentQQ_Offical_Guild(connectID)];
            Log.Debug($"创建了 {_AdapterInfo.Name} 适配器");

            //大概率是被遗弃的内容,但是还是保留了
            BaseUrl = new Uri(account.Sandbox ? "https://sandbox.api.sgroup.qq.com" : "https://api.sgroup.qq.com");

            //初始化定时器
            _accessTokenTimer.Elapsed += AccessTokenTimerCallback;
            _accessTokenTimer.AutoReset = false;

            //监听
            manager.ReceiveEvent += ListenReceiveEvent;
            manager.ConnectedEvent += ListenConnectedEvent;
            manager.DisconnectEvent += ListenDisconnectEvent;
        }

        ~TencentQQ()
        {
            Dispose();
        }

        internal delegate void ReflushAppAccessTokenHandle(GetAppAccessToken appAccessToken);

        internal event ReflushAppAccessTokenHandle? ReflushAppAccessTokenEvent;

        public IPlatform[] Platform { get; }
        public override string AccountID => account.AppID;
        public string? ID { get; private set; }
        public override bool IsStarted { get => isConnected; }
        public string? Nickname { get; private set; }

        public override void Dispose()
        {
            manager.ReceiveEvent -= ListenReceiveEvent;
            manager.ConnectedEvent -= ListenConnectedEvent;
            manager.DisconnectEvent -= ListenDisconnectEvent;

            _accessTokenTimer.Dispose();

            _httpClient.Dispose();
            manager.Dispose();

            Log.Debug($"释放了 {_AdapterInfo.Name}({connectID}) 适配器");
            GC.SuppressFinalize(this);
        }

        public override async Task<bool> Restart()
        {
            await StartGetAccessToken();
            if (!manager.StartAllClient())
            {
                manager.StopAllClient();
                return false;
            }
            return true;
        }

        public async Task<HttpResponseMessage?> Send(HttpRequestMessage httpRequestMessage)
        {
            short retryCount = 0;
            int retryInterval = 2000;
            HttpResponseMessage? responseMessage = null;

            //添加请求头
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("QQBot", appAccessToken?.AccessToken);
            httpRequestMessage.Headers.Add("X-Union-Appid", account.AppID);
            do
            {
                HttpRequestMessage requestMessage = await Copyer.CopyHttpRequestMessage(httpRequestMessage);

                try
                {
                    responseMessage = await _httpClient.SendAsync(requestMessage);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                    retryCount++;
                    await Task.Delay(retryInterval);
                    continue;
                }

                if (responseMessage.IsSuccessStatusCode)
                    break;
                retryCount++;
            } while (retryCount <= 5);

            if (responseMessage is null)
                return null;

            Log.Trace("Headers: {0}\nContent: {1}", responseMessage.Headers.ToString(), await responseMessage.Content.ReadAsStringAsync());

            if (!responseMessage.IsSuccessStatusCode)
            {
                Log.Warn("Headers: {0}\nContent: {1}", responseMessage.Headers.ToString(), await responseMessage.Content.ReadAsStringAsync());
                return null;
            }

            return responseMessage;
        }

        public override async Task<bool> Start()
        {
            bool result = true;
            result = result && await StartGetAccessToken();
            GatewayWithShards? gateway = await GetGatewayWithShards();
            result = result && gateway is not null;

            if (!result || appAccessToken is null)
                return false;

            string? connectID;
            for (int i = 0; i < gateway?.Shards; i++)
            {
                connectID = manager.CreateClient(typeof(TencentQQWS), this, new Uri(gateway.Value.Url), i, gateway.Value.Shards, account, appAccessToken);
                if (connectID is null)
                    return false;
            }

            if (!manager.StartAllClient())
            {
                manager.StopAllClient();
                return false;
            }
            return result;
        }

        public override async Task<bool> Stop()
        {
            isConnected = false;
            await Task.Run(() =>
            {
                manager.StopAllClient();
                //停止Token定时器
                _accessTokenTimer?.Stop();

                //释放所有WSClient
                manager.DisposeAllClient();
            });
            return true;
        }

        protected void OnConnected()
        {
            isConnected = true;
            OnConnected(connectID ?? "Null", _AdapterInfo);
        }

        protected void OnDisconnect(Exception? exception = null)
        {
            isConnected = false;
            OnDisconnect(connectID ?? "Null", _AdapterInfo, exception);
        }

        protected void OnError(Exception exception)
        {
            OnError(connectID ?? "Null", _AdapterInfo, exception);
        }

        private async void AccessTokenTimerCallback(object? sender, System.Timers.ElapsedEventArgs e)
        {
            short retryCount = 0;
            int retryInterval = 2000;
            do
            {
                retryCount++;
                GetAppAccessToken? accessToken = await GetAppAccessToken();
                if (accessToken is null)
                {
                    Log.Warn($"Token刷新失败，5秒后重试 ({retryCount}/5)");
                    await Task.Delay(retryInterval);
                }
                else
                {
                    appAccessToken = accessToken;

                    //广播Token刷新
                    ReflushAppAccessTokenEvent?.Invoke(appAccessToken ?? new GetAppAccessToken());

                    //启动定时器
                    _accessTokenTimer.Interval = (appAccessToken?.ExpiresIn ?? 1) * 1000;
                    _accessTokenTimer.Start();
                    return;
                }
            } while (retryCount < 5);

            OnError(new Exception("AppAccessToken 获取失败"));
        }

        private void Dispatch(JObject value)
        {
            string? t = (string?)value["t"];
            BowlFrame.Event.Message.MessageBase? @event = null;

            try
            {
                switch (t)
                {
                    case "AT_MESSAGE_CREATE":
                        @event = new ChannelMessage(this, value.ToObject<AT_MESSAGE_CREATE>() ?? throw new NullReferenceException());
                        break;

                    case "DIRECT_MESSAGE_CREATE":
                        @event = new GuildPrivateMessage(this, value.ToObject<DIRECT_MESSAGE_CREATE>() ?? throw new NullReferenceException());
                        break;

                    case "GROUP_AT_MESSAGE_CREATE":
                        @event = new GroupMessage(this, value.ToObject<GROUP_AT_MESSAGE_CREATE>() ?? throw new NullReferenceException());
                        break;

                    case "C2C_MESSAGE_CREATE":
                        @event = new PrivateMessage(this, value.ToObject<C2C_MESSAGE_CREATE>() ?? throw new NullReferenceException());
                        break;

                    case "READY":
                        ID = (string?)value["d"]?["user"]?["id"];
                        Nickname = (string?)value["d"]?["user"]?["username"];
                        break;

                    default:
                        Log.Debug("未使用的事件 {0}", t);
                        break;
                }

                if (@event is not null)
                    AdapterManager.OnBroadcastEvent(@event, _AdapterInfo);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private async Task<GetAppAccessToken?> GetAppAccessToken()
        {
            short count = 0;
            HttpResponseMessage? responseMessage = null;
            do
            {
                try
                {
                    responseMessage = await _httpClient.PostAsync("https://bots.qq.com/app/getAppAccessToken", JsonContent.Create(
                        new { appId = account.AppID, clientSecret = account.AppSecret }));
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                    count++;
                    continue;
                }

                if (responseMessage.IsSuccessStatusCode)
                    break;
                count++;
            } while (count <= 5);

            if (responseMessage is null)
                return null;
            GetAppAccessToken result = JsonConvert.DeserializeObject<GetAppAccessToken>(await responseMessage.Content.ReadAsStringAsync());

            //提前60秒进行刷新(写59不是手误)
            result.ExpiresIn -= 59;
            Log.Debug($"获取 AppAccessToken 成功, AccessToken: {result.AccessToken} ExpirTime: {result.ExpiresIn}");
            return result;
        }

        private async Task<GatewayWithShards?> GetGatewayWithShards()
        {
            HttpRequestMessage httpRequest = new(HttpMethod.Get, new Uri(BaseUrl, "/gateway/bot"));
            HttpResponseMessage? responseMessage = await Send(httpRequest);

            if (responseMessage is null)
                return null;

            GatewayWithShards result = JsonConvert.DeserializeObject<GatewayWithShards>(await responseMessage.Content.ReadAsStringAsync());
            Log.Debug($"获取 Gateway 成功, Url: {result.Url} 建议分片: {result.Shards}");
            return result;
        }

        private void ListenConnectedEvent(WSClient client)
        {
            foreach (string connectID in manager.GetAllConnectID())
                if (manager[connectID]?.IsConnected != true)
                    return;
            OnConnected();
        }

        private async void ListenDisconnectEvent(WSClient client, WebSocketCloseStatus closeStatus)
        {
            if (!isConnected)
                return;

            int retryInterval = 2000;
            TencentQQWS? client1 = client as TencentQQWS;

            if (client1 is not null)
            {
                if (client1.IsConnectSuccessed is not null)
                {
                    if (client1.IsConnectSuccessed != true)
                    {
                        Log.Info($"{client.GetType().Name}({client.ConnectID}) 等待 {retryInterval / 1000}s 后重连");
                        await Task.Delay(retryInterval);
                    }

                    if (client.ConnectAsync().Result)
                        return;
                }
                else
                {
                    Log.Warn($"{client.GetType().Name}({client.ConnectID}) 无法重连,重新启动适配器");
                    _ = Stop().Result;
                    OnDisconnect();
                    return;
                }
            }

            //重连失败,判断其他是否也断开
            foreach (string connectID in manager.GetAllConnectID())
                if (manager[connectID]?.IsConnected == true)
                    return;
            OnDisconnect();
        }

        private void ListenReceiveEvent(WSClient client, byte[] bytes, WebSocketReceiveResult receiveResult)
        {
            if (receiveResult.MessageType == WebSocketMessageType.Text)
            {
                //转换为文本
                string receivedMessage = Encoding.UTF8.GetString(bytes);
                JObject value = JObject.Parse(receivedMessage);

                //Log.Trace(receivedMessage);

                short op = (short?)value["op"] ?? 9;

                switch (op)
                {
                    //Dispatch 服务端进行消息推送
                    case 0:
                        Dispatch(value);
                        break;
                }
            }
        }

        private async ValueTask<bool> StartGetAccessToken()
        {
            //获取Token并启动定时器
            appAccessToken = await GetAppAccessToken();
            if (appAccessToken is null)
                return false;
            _accessTokenTimer.Interval = (appAccessToken?.ExpiresIn ?? 1) * 1000;
            _accessTokenTimer.Start();
            return true;
        }
    }
}