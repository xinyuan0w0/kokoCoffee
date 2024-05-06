namespace BowlFrame.Adapter.CocoaPlugin.Exceptions
{
    internal class FuncAlreadyExits : CocoaPluginException
    {
        public FuncAlreadyExits() : base("该插件已存在")
        {
        }

        public FuncAlreadyExits(string? funcID) : base($"{funcID} 已存在")
        {
        }

        public FuncAlreadyExits(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}