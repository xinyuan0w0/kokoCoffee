using BowlFrame.Database.TableStruct;
using BowlFrame.Message;
using BowlFrame.Perm;
using BowlFrame.Adapter.OneBotV11Adapter.Target;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.OneBotV11Adapter.Event.Message
{
    public class GroupMessage : BowlFrame.Event.Message.GroupMessage
    {
        //平台识别
        private static readonly IPlatform _platform = new TencentQQ_Common();

        private readonly Struct.GroupMessage _message;

        private readonly Permission _permission = new() { Platform = _platform };

        public GroupMessage(OneBotV11 oneBotV11, Struct.GroupMessage message) : base(_platform)
        {
            _message = message;

            Messages = Tools.MsgHelper.GetMessages(oneBotV11, message).Result;

            string uuid;

            //用户
            DbPlatformID platformID = _permission.FindTarget(message.UserID).Result ?? throw new NullReferenceException();
            uuid = platformID == DbPlatformID.Empty
                ? _permission.CreateTarget(message.UserID, TargetType.User).Result ?? throw new NullReferenceException()
                : platformID.UUID;

            user = new(oneBotV11.AccountID, uuid, data: message);

            //群
            platformID = _permission.FindTarget(message.GroupID).Result ?? throw new NullReferenceException();
            uuid = platformID == DbPlatformID.Empty
                ? _permission.CreateTarget(message.GroupID, TargetType.Group).Result ?? throw new NullReferenceException()
                : platformID.UUID;

            group = new(oneBotV11.AccountID, uuid, data: message);

            permission = new(user.UUID, linkTarget: new(group.UUID) { Platform = _platform }) { Platform = _platform };
        }

        private readonly Permission permission;

        private readonly User user;

        private readonly Group group;

        public override string RoomID => _message.GroupID;

        public override User User => user;

        public override Group Group => group;

        public override Permission Permission => permission;

        public override string RawMessage => _message.RawMessage;

        public override string Target => _message.UserID;

        public override Task<bool> SendAsync(Messages messages) => group.SendAsync(messages);
    }
}