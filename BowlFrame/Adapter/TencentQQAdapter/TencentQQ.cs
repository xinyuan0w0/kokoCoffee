using BowlFrame.Net.WebSocket;
using BowlFrame.Tools;
using BowlFrame.Adapter.TencentQQAdapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace BowlFrame.Adapter.TencentQQAdapter
{
    internal class TencentQQ : AdapterBase
    {
        private readonly HttpClient _httpClient = new();

        private static readonly AdapterInfo _adapterInfo = new()
        {
            Name = "TencentQQ_Offical",
            ID = "1",
            Platform = "TencentQQ",
            Description = "腾讯QQ官方API",
        };

        private readonly System.Timers.Timer _accessTokenTimer = new();

        private static readonly WSManager manager = new();

        private GetAppAccessToken? appAccessToken;

        //条件未写
        public new bool IsConnected { get => true; }

        private TencentQQAccount account;

        public TencentQQ(TencentQQAccount account)
        {
            this.account = account;
            Logger.Log.Debug($"创建了 {_adapterInfo.Name} 适配器");

            //初始化定时器
            _accessTokenTimer.Elapsed += AccessTokenTimerCallback;
            _accessTokenTimer.AutoReset = false;
        }

        public override void Dispose()
        {
            _accessTokenTimer.Dispose();
        }

        public override async ValueTask<bool> Restart()
        {
            await StartGetAccessToken();
            return true;
        }

        public override async ValueTask<bool> Start()
        {
            await StartGetAccessToken();
            return true;
        }

        public override ValueTask<bool> Stop()
        {
            //停止Token定时器
            _accessTokenTimer?.Stop();

            return new ValueTask<bool>(true);
        }

        private async ValueTask<bool> StartGetAccessToken()
        {
            //获取Token并启动定时器
            appAccessToken = await GetAppAccessToken();
            if (appAccessToken is null)
                return false;
            _accessTokenTimer.Interval = appAccessToken?.expires_in ?? 1 * 1000;
            _accessTokenTimer.Start();
            return true;
        }

        private async void AccessTokenTimerCallback(object? sender, System.Timers.ElapsedEventArgs e)
        {
            short count = 0;
            do
            {
                count++;
                GetAppAccessToken? accessToken = await GetAppAccessToken();
                if (accessToken is null)
                {
                    Logger.Log.Warn($"Token刷新失败，5秒后重试 ({count}/5)");
                    Thread.Sleep(5000);
                }
                else
                {
                    appAccessToken = accessToken;
                    _accessTokenTimer.Interval = appAccessToken?.expires_in ?? 1 * 1000;
                    _accessTokenTimer.Start();
                    return;
                }
            } while (count < 5);

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
                    Logger.Log.Warn(e);
                    count++;
                    continue;
                }

                if (responseMessage.IsSuccessStatusCode != true)
                    break;
                count++;
            } while (count <= 5);

            if (responseMessage is null)
                return null;
            GetAppAccessToken result = await responseMessage.Content.ReadFromJsonAsync<GetAppAccessToken>();
            result.expires_in -= 60;
            return result;
        }

        private async Task<Uri> GetWSSUri()
        {
        }

        protected void OnConnected(Exception exception)
        {
            OnConnectedEvent(connectID ?? "Null", _adapterInfo);
        }

        protected void OnDisconnect(Exception exception)
        {
            OnDisconnectEvent(connectID ?? "Null", _adapterInfo, exception);
        }

        protected void OnError(Exception exception)
        {
            OnErrorEvent(connectID ?? "Null", _adapterInfo, exception);
        }
    }
}