using BowlFrame.Net.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter.OneBotV11Adapter
{
    public class OneBotV11 : AdapterBase
    {
        public new static readonly AdapterInfo _AdapterInfo = new()
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

        private readonly OneBotV11Account account;

        private readonly OneBotV11WS socket;

        private bool isConnected;

        public override bool IsConnected { get => isConnected; }

        public override string AccountID => account.Account;

        public OneBotV11(JObject args)
        {
            //反序列化
            JsonSerializer jsonSerializer = new()
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };
            account = args.ToObject<OneBotV11Account>(jsonSerializer);

            Log.Debug($"创建了 {_AdapterInfo.Name} 适配器");

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
                await socket.ConnectAsync();
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
            await socket.CloseAsync();
            return true;
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
    }
}