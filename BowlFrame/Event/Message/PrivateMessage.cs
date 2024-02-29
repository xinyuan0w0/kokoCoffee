using BowlFrame.Adapter;
using BowlFrame.Target;

namespace BowlFrame.Event.Message
{
    internal abstract class PrivateMessage : MessageBase
    {
        public abstract User User { get; }

        protected PrivateMessage(IPlatform platform) : base(platform, SourceType.Private)
        {
        }
    }
}