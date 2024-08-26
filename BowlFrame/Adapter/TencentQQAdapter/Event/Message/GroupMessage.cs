using BowlFrame.Adapter.TencentQQAdapter.Struct;
using BowlFrame.Adapter.TencentQQAdapter.Target;
using BowlFrame.Database.TableStruct;
using BowlFrame.Message;
using BowlFrame.Perm;
using System.Runtime.InteropServices;

namespace BowlFrame.Adapter.TencentQQAdapter.Event.Message
{
    public class GroupMessage : BowlFrame.Event.Message.GroupMessage
    {
        //private readonly TencentQQ _tencentQQ;

        private readonly GROUP_AT_MESSAGE_CREATE _message;

        private readonly Permission _permission;

        private readonly IPlatform _platform;

        public GroupMessage(TencentQQ tencentQQ, GROUP_AT_MESSAGE_CREATE message) : base(tencentQQ.Platform[0])
        {
            //_tencentQQ = tencentQQ;
            _message = message;
            _platform = tencentQQ.Platform[0];
            _permission = new() { Platform = _platform };
            Messages = Tools.MsgHelper.GetMessages(tencentQQ, message.Data).Result;

            string uuid;

            //用户
            DbPlatformID platformID = _permission.FindTarget(message.Data.Author.OpenID).Result ?? throw new NullReferenceException();
            uuid = platformID == DbPlatformID.Empty
                ? _permission.CreateTarget(message.Data.Author.OpenID, TargetType.User).Result ?? throw new NullReferenceException()
                : platformID.UUID;

            user = new(tencentQQ.AccountID, uuid, data: message.Data);

            //群
            platformID = _permission.FindTarget(message.Data.GroupOpenID).Result ?? throw new NullReferenceException();
            uuid = platformID == DbPlatformID.Empty
                ? _permission.CreateTarget(message.Data.GroupOpenID, TargetType.Group).Result ?? throw new NullReferenceException()
                : platformID.UUID;

            group = new(tencentQQ.AccountID, uuid, data: message.Data);

            permission = new(user.UUID, linkTarget: new(group.UUID) { Platform = _platform }) { Platform = _platform };
        }

        public override string RoomID => _message.Data.GroupOpenID;

        private readonly Permission permission;

        public override Permission Permission => permission;

        private readonly User user;

        public override User User => user;

        private readonly Group group;

        public override Group Group => group;

        public override string RawMessage => _message.Data.Content;

        public override string Target => _message.Data.Author.OpenID;

        public override async Task<bool> SendAsync(Messages messages) => await group.tencentQQApi.SendMessage(messages, _message.Data.ID) == true;
    }
}