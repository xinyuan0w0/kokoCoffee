using BowlFrame.Adapter.TencentQQAdapter.Struct;
using BowlFrame.Adapter.TencentQQAdapter.Target;
using BowlFrame.Database.TableStruct;
using BowlFrame.Message;
using BowlFrame.Perm;

namespace BowlFrame.Adapter.TencentQQAdapter.Event.Message
{
    public class PrivateMessage : BowlFrame.Event.Message.PrivateMessage
    {
        //private readonly TencentQQ _tencentQQ;

        private readonly C2C_MESSAGE_CREATE _message;

        private readonly Permission _permission;

        private readonly IPlatform _platform;

        public PrivateMessage(TencentQQ tencentQQ, C2C_MESSAGE_CREATE message) : base(tencentQQ.Platform[0])
        {
            //_tencentQQ = tencentQQ;
            _message = message;
            _platform = tencentQQ.Platform[0];
            _permission = new() { Platform = _platform };
            Messages = Tools.MsgHelper.GetMessages(tencentQQ, message.Data).Result;

            //用户
            DbPlatformID platformID = _permission.FindTarget(message.Data.Author.OpenID).Result ?? throw new NullReferenceException();

            string uuid = platformID == DbPlatformID.Empty
                ? _permission.CreateTarget(message.Data.Author.OpenID, TargetType.User).Result ?? throw new NullReferenceException()
                : platformID.UUID;

            user = new(tencentQQ.AccountID, uuid, data: message.Data);

            permission = user.permission;
        }

        private readonly Permission permission;

        public override Permission Permission => permission;

        private readonly User user;

        public override User User => user;

        public override string RawMessage => _message.Data.Content;

        public override string Target => _message.Data.Author.OpenID;

        public override async Task<bool> SendAsync(Messages messages) => await User.tencentQQApi.SendMessage(messages, _message.Data.ID) == true;
    }
}