namespace BowlFrame.Adapter.CocoaPlugin.Exceptions
{
    internal class NotFoundDependPlugin : CocoaPluginException
    {
        public NotFoundDependPlugin() : base("载入插件时未找到依赖插件")
        {
        }

        public NotFoundDependPlugin(string? message) : base(message)
        {
        }

        public NotFoundDependPlugin(string? plugin, string? dependPlugin) : base($"未载入插件 {plugin} 依赖插件 {dependPlugin}")
        {
        }

        public NotFoundDependPlugin(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}