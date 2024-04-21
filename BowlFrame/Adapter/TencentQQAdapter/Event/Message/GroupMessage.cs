using BowlFrame.Adapter.TencentQQAdapter.Struct;
using BowlFrame.Adapter.TencentQQAdapter.Target;
using BowlFrame.Database.TableStruct;
using BowlFrame.Message;
using BowlFrame.Perm;

namespace BowlFrame.Adapter.TencentQQAdapter.Event.Message
{
    public class GroupMessage : BowlFrame.Event.Message.GroupMessage
    {
        //private readonly TencentQQ _tencentQQ;

        private readonly GROUP_AT_MESSAGE_CREATE _message;

        private readonly Permission permission = new() { Platform = platform };

        private static readonly IPlatform platform = new TencentQQ_Offical_Common();

        public GroupMessage(TencentQQ tencentQQ, GROUP_AT_MESSAGE_CREATE message) : base(platform)
        {
            //_tencentQQ = tencentQQ;
            _message = message;
            Messages = Tools.MsgHelper.GetMessages(tencentQQ, message.Data).Result;

            string uuid;

            //用户
            DbPlatformID platformID = permission.FindTarget(message.Data.Author.OpenID).Result ?? throw new NullReferenceException();
            uuid = platformID == DbPlatformID.Empty
                ? permission.CreateTarget(message.Data.Author.OpenID, TargetType.User).Result ?? throw new NullReferenceException()
                : platformID.UUID;

            _user = new(tencentQQ.AccountID, uuid, data: message.Data);

            //群
            platformID = permission.FindTarget(message.Data.GroupOpenID).Result ?? throw new NullReferenceException();
            uuid = platformID == DbPlatformID.Empty
                ? permission.CreateTarget(message.Data.GroupOpenID, TargetType.Group).Result ?? throw new NullReferenceException()
                : platformID.UUID;

            _group = new(tencentQQ.AccountID, uuid, data: message.Data);
        }

        public override string RoomID => _message.Data.GroupOpenID;

        private readonly User _user;

        public override User User => _user;

        private readonly Group _group;

        public override Group Group => _group;

        public override string RawMessage => _message.Data.Content;

        public override string Target => _message.Data.Author.OpenID;

        public override async Task<bool> SendAsync(Messages messages) => await _group.tencentQQApi.SendMessage(messages, _message.Data.ID) == true;
    }
}