using BowlFrame.Adapter;
using BowlFrame.Target;

namespace BowlFrame.Event.Message
{
    public abstract class PrivateMessage(IPlatform platform) : MessageBase(platform, SourceType.Private)
    {
        public abstract User User { get; }
    }
}