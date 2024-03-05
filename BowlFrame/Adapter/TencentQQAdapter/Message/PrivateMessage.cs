using BowlFrame.Adapter;
using BowlFrame.Target;

namespace BowlFrame.Adapter.TencentQQAdapter.Message
{
    public class PrivateMessage(IPlatform platform) : Event.Message.PrivateMessage(platform)
    {
        public override User User { get; }
    }
}