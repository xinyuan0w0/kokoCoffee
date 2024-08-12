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

    public class TencentQQ_Common(string? connectID) : IPlatform
    {
        public AdapterInfo AdapterInfo { get; } = OneBotV11._AdapterInfo;

        public string ID { get; } = "TencentQQ_Common";

        public string? ConnectID { get; } = connectID;
    }
}