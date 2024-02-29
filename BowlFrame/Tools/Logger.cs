using NLog;

namespace BowlFrame.Tools
{
    public static class Logger
    {
        public static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();
    }
}