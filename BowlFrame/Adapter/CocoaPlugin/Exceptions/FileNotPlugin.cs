namespace BowlFrame.Adapter.CocoaPlugin.Exceptions
{
    internal class FileNotPlugin : CocoaPluginException
    {
        public FileNotPlugin() : base("加载文件非支持的插件文件")
        {
        }

        public FileNotPlugin(string? filePath) : base($"文件 {filePath} 非支持的插件文件")
        {
        }

        public FileNotPlugin(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}