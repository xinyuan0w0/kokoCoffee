using BowlFrame.Net.WebSocket;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter.OneBotV11Adapter
{
    internal class OneBotV11WS : WSClient
    {
        private readonly string? accessToken;

        public OneBotV11WS(Uri uri, string? accessToken) : base(uri)
        {
            this.accessToken = accessToken;

            Log.Debug("创建了 OneBotV11WS WSClient");
        }

        ~OneBotV11WS()
        {
            Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        protected override void CreateNewSocket()
        {
            base.CreateNewSocket();
            if (accessToken is not null)
                socket?.Options.SetRequestHeader("Authorization", "Bearer " + accessToken);
        }
    }
}