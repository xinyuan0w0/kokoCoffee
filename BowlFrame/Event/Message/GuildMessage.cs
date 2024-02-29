using BowlFrame.Adapter;
using BowlFrame.Target;

namespace BowlFrame.Event.Message
{
    internal abstract class GuildMessage : MessageBase
    {
        public abstract string RoomID { get; }

        public abstract User User { get; }

        public abstract Guild Guild { get; }

        public GuildMessage(IPlatform platform) : base(platform, SourceType.Group)
        {
        }
    }
}