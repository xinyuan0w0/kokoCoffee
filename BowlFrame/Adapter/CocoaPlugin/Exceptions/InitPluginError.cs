namespace BowlFrame.Adapter.CocoaPlugin.Exceptions
{
    internal class InitPluginError : CocoaPluginException
    {
        public InitPluginError() : base("初始化插件时发生错误")
        {
        }

        public InitPluginError(string? message) : base(message)
        {
        }

        public InitPluginError(Exception? innerException) : base("初始化插件时发生错误", innerException)
        {
        }

        public InitPluginError(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}