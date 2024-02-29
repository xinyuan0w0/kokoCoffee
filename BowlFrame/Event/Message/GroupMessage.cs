using BowlFrame.Adapter;
using BowlFrame.Target;

namespace BowlFrame.Event.Message
{
    internal abstract class GroupMessage : MessageBase
    {
        public abstract string RoomID { get; }

        public abstract User User { get; }

        public abstract Group Group { get; }

        public GroupMessage(IPlatform platform) : base(platform, SourceType.Group)
        {
        }
    }
}