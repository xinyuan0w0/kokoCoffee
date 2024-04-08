namespace BowlFrame.Adapter.CocoaPlugin
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CocoaEventAttribute : Attribute
    {
        public bool PrivateMessage { get; set; }

        public bool GroupMessage { get; set; }

        public bool GuildMessage { get; set; }

        public bool ChannelMessage { get; set; }

        public bool GuildPrivateMessage { get; set; }

        public bool PostMessage { get; set; }

        public bool MessageEvent { get; set; }

        public bool MainEvent { get; set; }

        public bool MetaEvent { get; set; }

        public bool Broadcast { get; set; }
    }
}