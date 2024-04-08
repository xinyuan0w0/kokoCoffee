namespace BowlFrame.Adapter.CocoaPlugin.Exceptions
{
    internal class PluginAlreadyExits : CocoaPluginException
    {
        public PluginAlreadyExits() : base("该插件已存在")
        {
        }

        public PluginAlreadyExits(string? message) : base(message)
        {
        }

        public PluginAlreadyExits(CocoaPluginConfig? config) : base(config is null ? "该插件已存在" : $"{config?.Name} ({config?.ID}) 已存在")
        {
        }

        public PluginAlreadyExits(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}