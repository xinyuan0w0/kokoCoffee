using BowlFrame.Adapter.TencentQQAdapter.Struct;
using BowlFrame.Adapter.TencentQQAdapter.Target;
using BowlFrame.Database.TableStruct;
using BowlFrame.Message;
using BowlFrame.Perm;

namespace BowlFrame.Adapter.TencentQQAdapter.Event.Message
{
    public class PrivateMessage : BowlFrame.Event.Message.PrivateMessage
    {
        private readonly TencentQQ _tencentQQ;

        private readonly C2C_MESSAGE_CREATE _message;

        private readonly Permission permission = new() { Platform = platform };

        private static readonly IPlatform platform = new TencentQQ_Offical_Common();

        public PrivateMessage(TencentQQ tencentQQ, C2C_MESSAGE_CREATE message) : base(platform)
        {
            _tencentQQ = tencentQQ;
            _message = message;
            Messages = Tools.MsgHelper.GetMessages(tencentQQ, message.Data).Result;

            string? uuid;

            DbPlatformID platformID = permission.FindTarget(message.Data.Author.OpenID).Result ?? throw new NullReferenceException();
            if (platformID == DbPlatformID.Empty)
                uuid = permission.CreateTarget(message.Data.Author.OpenID, TargetType.User).Result ?? throw new NullReferenceException();
            else
                uuid = platformID.UUID;

            if (uuid == "")
                throw new NullReferenceException();

            _user = new(tencentQQ.AccountID, uuid, data: message.Data);
        }

        private readonly User _user;

        public override User User => _user;

        public override string RawMessage => _message.Data.Content;

        public override string Target => _message.Data.Author.OpenID;

        public override async Task<bool> SendAsync(Messages messages) => await User.tencentQQApi.Send(messages, _message.Data.ID) == true;
    }
}