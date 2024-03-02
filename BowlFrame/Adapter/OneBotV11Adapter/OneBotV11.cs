using BowlFrame.Net.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter.OneBotV11Adapter
{
    internal class OneBotV11 : AdapterBase
    {
        private static readonly AdapterInfo _adapterInfo = new()
        {
            Name = "OneBotV11",
            ID = "cn.kokobot.onebotv11_gocqhttp",
            Platform = "TencentQQ",
            Description = "腾讯QQ第三方API",
        };

        private readonly OneBotV11Account account;

        private readonly OneBotV11WS socket;

        private bool isConnected;

        public override bool IsConnected { get => isConnected; }

        public OneBotV11(JObject args)
        {
            //反序列化
            JsonSerializer jsonSerializer = new()
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };
            account = args.ToObject<OneBotV11Account>(jsonSerializer);

            Log.Debug($"创建了 {_adapterInfo.Name} 适配器");

            socket = new(new Uri(account.ConnectInfo.WebSocket), account.AccessToken);

            //监听
            socket.ConnectedEvent += ListenConnectedEvent;
            socket.DisconnectEvent += ListenDisconnectEvent;
        }

        ~OneBotV11()
        {
            Dispose();
        }

        public override void Dispose()
        {
            socket.Dispose();

            Log.Debug($"释放了 {_adapterInfo.Name}({connectID}) 适配器");
            GC.SuppressFinalize(this);
        }

        public override async ValueTask<bool> Restart()
        {
            bool result = await Stop();
            result = result && await Start();
            return result;
        }

        public override async ValueTask<bool> Start()
        {
            try
            {
                await socket.ConnectAsync();
            }
            catch (Exception e)
            {
                Log.Warn(e);
                return false;
            }

            return true;
        }

        public override ValueTask<bool> Stop()
        {
            socket.CloseAsync().Wait();
            return new ValueTask<bool>(true);
        }

        private void ListenConnectedEvent(WSClient client)
        {
            OnConnected();
        }

        private async void ListenDisconnectEvent(WSClient client, WebSocketCloseStatus closeStatus)
        {
            if (!isConnected)
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