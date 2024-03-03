using BowlFrame.Adapter;
using BowlFrame.Target;

namespace BowlFrame.Event.Message
{
    public abstract class GuildMessage(IPlatform platform) : MessageBase(platform, SourceType.Group)
    {
        public abstract string RoomID { get; }

        public abstract User User { get; }

        public abstract Guild SubGuild { get; }

        public abstract Channel Channel { get; }
    }
}