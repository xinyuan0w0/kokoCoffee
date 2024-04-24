namespace BowlFrame.Adapter.CocoaPlugin
{
    internal struct CocoaAccount
    {
        public string Account { get; set; }
    }

    public struct CocoaPluginConfig
    {
        public string Name { get; set; }

        public string ID { get; set; }

        public string Version { get; set; }

        public string Author { get; set; }

        public string Description { get; set; }

        public string Main { get; set; }

        public string[]? Depend { get; set; }

        public string? Website { get; set; }

        public string? Content { get; set; }
    }

    public class CocoaPlatform(string id) : IPlatform
    {
        public AdapterInfo AdapterInfo => Cocoa._AdapterInfo;

        public string ID => id;
    }
}