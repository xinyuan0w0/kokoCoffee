namespace BowlFrame.Adapter.CocoaPlugin.Exceptions
{
    internal class NotFoundPlugin : CocoaPluginException
    {
        public NotFoundPlugin() : base("未找到插件")
        {
        }

        public NotFoundPlugin(string? plugin) : base($"未找到插件 {plugin}")
        {
        }

        public NotFoundPlugin(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}