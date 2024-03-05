using BowlFrame.Adapter;
using BowlFrame.Target;

namespace BowlFrame.Event.Message
{
    public abstract class GroupMessage(IPlatform platform) : MessageBase(platform, SourceType.Group)
    {
        public abstract string RoomID { get; }

        public abstract User User { get; }

        public abstract Group Group { get; }
    }
}