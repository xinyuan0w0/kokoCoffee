using BowlFrame.Adapter.TencentQQAdapter.Struct;
using BowlFrame.Adapter.TencentQQAdapter;
using BowlFrame.Message;
using BowlFrame.Perm;
using BowlFrame.Adapter.OneBotV11Adapter.Target;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BowlFrame.Adapter.OneBotV11Adapter.Struct;
using BowlFrame.Database.TableStruct;

namespace BowlFrame.Adapter.OneBotV11Adapter.Event.Message
{
    public class PrivateMessage : BowlFrame.Event.Message.PrivateMessage
    {
        private readonly MessageBase _message;

        private readonly Permission _permission;

        private readonly IPlatform _platform;

        public PrivateMessage(OneBotV11 oneBotV11, MessageBase message) : base(oneBotV11.Platform)
        {
            _platform = oneBotV11.Platform;

            _permission = new() { Platform = _platform };

            _message = message;

            Messages = Tools.MsgHelper.GetMessages(oneBotV11, message).Result;

            string uuid;

            //用户
            DbPlatformID platformID = _permission.FindTarget(message.UserID).Result ?? throw new NullReferenceException();
            uuid = platformID == DbPlatformID.Empty
                ? _permission.CreateTarget(message.UserID, TargetType.User).Result ?? throw new NullReferenceException()
                : platformID.UUID;

            user = new(oneBotV11.AccountID, uuid, data: message);

            permission = user.permission;
        }

        private readonly Permission permission;

        private readonly User user;

        public override User User => user;

        public override Permission Permission => permission;

        public override string RawMessage => _message.RawMessage;

        public override string Target => _message.UserID;

        public override Task<bool> SendAsync(Messages messages) => user.SendAsync(messages);
    }
}