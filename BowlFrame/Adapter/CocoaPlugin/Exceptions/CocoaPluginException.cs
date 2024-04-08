namespace BowlFrame.Adapter.CocoaPlugin.Exceptions
{
    internal class CocoaPluginException : Exception
    {
        public CocoaPluginException()
        {
        }

        public CocoaPluginException(string? message) : base(message)
        {
        }

        public CocoaPluginException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}