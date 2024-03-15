using BowlFrame.Adapter.TencentQQAdapter.Struct;
using BowlFrame.Adapter.TencentQQAdapter.Target;
using BowlFrame.Database.TableStruct;
using BowlFrame.Message;
using BowlFrame.Perm;

namespace BowlFrame.Adapter.TencentQQAdapter.Event.Message
{
    public class GuildPrivateMessage : BowlFrame.Event.Message.PrivateMessage
    {
        private readonly TencentQQ _tencentQQ;

        private readonly DIRECT_MESSAGE_CREATE _message;

        private readonly Permission permission = new() { Platform = platform };

        private static readonly IPlatform platform = new TencentQQ_Offical_Guild();

        public GuildPrivateMessage(TencentQQ tencentQQ, DIRECT_MESSAGE_CREATE message) : base(platform)
        {
            _tencentQQ = tencentQQ;
            _message = message;
            Messages = Tools.MsgHelper.GetMessages(tencentQQ, message.Data).Result;

            string? uuid;

            DbPlatformID platformID = permission.FindTarget(message.Data.Author.ID).Result ?? throw new NullReferenceException();
            if (platformID == DbPlatformID.Empty)
            {
                uuid = permission.CreateTarget(message.Data.Author.ID, TargetType.User).Result ?? throw new NullReferenceException();
                if (uuid == "")
                    throw new NullReferenceException();
            }
            else
                uuid = platformID.UUID;

            _user = new(tencentQQ.AccountID, uuid,data: message.Data);
        }

        private readonly GuildUser _user;

        public override GuildUser User => _user;

        public override string RawMessage => _message.Data.Content;

        public override string Target => _message.Data.Author.ID;

        public override async Task<bool> SendAsync(Messages messages) => await _user.tencentQQApi.GuildSend(messages, _message.Data.ID) == true;
    }
}