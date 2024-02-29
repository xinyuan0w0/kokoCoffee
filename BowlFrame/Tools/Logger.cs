using NLog;

namespace BowlFrame.Tools
{
    public static class Logger
    {
        public static NLog.Logger Log = LogManager.GetCurrentClassLogger();
    }
}