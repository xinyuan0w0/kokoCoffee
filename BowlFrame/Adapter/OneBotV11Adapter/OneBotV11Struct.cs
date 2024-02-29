namespace BowlFrame.Adapter.OneBotV11Adapter
{
    internal struct OneBotV11Account
    {
        public string Account { get; set; }
        public string? AccessToken { get; set; }
        public OneBotV11ConnectInfo ConnectInfo { get; set; }
    }

    internal struct OneBotV11ConnectInfo
    {
        public string WebSocket { get; set; }
        public string Http { get; set; }
    }
}