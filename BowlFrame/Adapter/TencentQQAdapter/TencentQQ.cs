using BowlFrame.Net.WebSocket;
using BowlFrame.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.WebSockets;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter.TencentQQAdapter
{
    internal class TencentQQ : AdapterBase
    {
        private static readonly AdapterInfo _adapterInfo = new()
        {
            Name = "TencentQQ_Offical",
            ID = "1",
            Platform = "TencentQQ",
            Description = "腾讯QQ官方API",
        };

        private readonly HttpClient _httpClient = new();

        private readonly System.Timers.Timer _accessTokenTimer = new();

        private readonly WSManagerEx manager = new();

        private bool isConnected;

        public override bool IsConnected { get => isConnected; }

        private TencentQQAccount account;

        private GetAppAccessToken? appAccessToken;

        private readonly Uri baseUrl;

        public delegate void ReflushAppAccessTokenHandle(GetAppAccessToken appAccessToken);

        public event ReflushAppAccessTokenHandle? ReflushAppAccessTokenEvent;

        public TencentQQ(JObject args)
        {
            //反序列化
            JsonSerializer jsonSerializer = new()
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };
            account = args.ToObject<TencentQQAccount>(jsonSerializer);

            Log.Debug($"创建了 {_adapterInfo.Name} 适配器");

            //大概率是被遗弃的内容,但是还是保留了
            baseUrl = new Uri(account.Sandbox ? "https://sandbox.api.sgroup.qq.com" : "https://api.sgroup.qq.com");

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

        public override void Dispose()
        {
            manager.ReceiveEvent -= ListenReceiveEvent;
            manager.ConnectedEvent -= ListenConnectedEvent;
            manager.DisconnectEvent -= ListenDisconnectEvent;

            _accessTokenTimer.Dispose();

            _httpClient.Dispose();
            manager.Dispose();

            Log.Debug($"释放了 {_adapterInfo.Name}({connectID}) 适配器");
            GC.SuppressFinalize(this);
        }

        public override async ValueTask<bool> Restart()
        {
            await StartGetAccessToken();
            if (!manager.StartAllClient())
            {
                manager.StopAllClient();
                return false;
            }
            return true;
        }

        public override async ValueTask<bool> Start()
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

        public override ValueTask<bool> Stop()
        {
            isConnected = false;
            manager.StopAllClient();
            //停止Token定时器
            _accessTokenTimer?.Stop();

            //释放所有WSClient
            manager.DisposeAllClient();
            return new ValueTask<bool>(true);
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
            HttpRequestMessage httpRequest = new(HttpMethod.Get, new Uri(baseUrl, "/gateway/bot"));
            HttpResponseMessage? responseMessage = await Send(httpRequest);

            if (responseMessage is null)
                return null;

            GatewayWithShards result = JsonConvert.DeserializeObject<GatewayWithShards>(await responseMessage.Content.ReadAsStringAsync());
            Log.Debug($"获取 Gateway 成功, Url: {result.Url} 建议分片: {result.Shards}");
            return result;
        }

        private async Task<HttpResponseMessage?> Send(HttpRequestMessage httpRequestMessage)
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

            if (responseMessage is null || !responseMessage.IsSuccessStatusCode)
                return null;
            return responseMessage;
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
        }

        protected void OnConnected()
        {
            isConnected = true;
            OnConnectedEvent(connectID ?? "Null", _adapterInfo);
        }

        protected void OnDisconnect(Exception? exception = null)
        {
            isConnected = false;
            OnDisconnectEvent(connectID ?? "Null", _adapterInfo, exception);
        }

        protected void OnError(Exception exception)
        {
            OnErrorEvent(connectID ?? "Null", _adapterInfo, exception);
        }
    }
}