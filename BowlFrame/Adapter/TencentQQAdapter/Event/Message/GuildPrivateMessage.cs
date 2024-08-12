using BowlFrame.Adapter.TencentQQAdapter.Struct;
using BowlFrame.Adapter.TencentQQAdapter.Target;
using BowlFrame.Database.TableStruct;
using BowlFrame.Message;
using BowlFrame.Perm;
using System.Runtime.InteropServices;

namespace BowlFrame.Adapter.TencentQQAdapter.Event.Message
{
    public class GuildPrivateMessage : BowlFrame.Event.Message.PrivateMessage
    {
        //private readonly TencentQQ _tencentQQ;

        private readonly DIRECT_MESSAGE_CREATE _message;

        private readonly Permission _permission;

        private readonly IPlatform _platform;

        public GuildPrivateMessage(TencentQQ tencentQQ, DIRECT_MESSAGE_CREATE message) : base(tencentQQ.Platform[1])
        {
            //_tencentQQ = tencentQQ;
            _message = message;
            _platform = tencentQQ.Platform[1];
            _permission = new() { Platform = _platform };
            Messages = Tools.MsgHelper.GetMessages(tencentQQ, message.Data).Result;

            DbPlatformID platformID = _permission.FindTarget(message.Data.Author.ID).Result ?? throw new NullReferenceException();

            //用户
            string uuid = platformID == DbPlatformID.Empty
                ? _permission.CreateTarget(message.Data.Author.ID, TargetType.User).Result ?? throw new NullReferenceException()
                : platformID.UUID;

            user = new(tencentQQ.AccountID, uuid, data: message.Data);

            permission = user.permission;
        }

        private readonly Permission permission;

        public override Permission Permission => permission;

        private readonly GuildUser user;

        public override GuildUser User => user;

        public override string RawMessage => _message.Data.Content;

        public override string Target => _message.Data.Author.ID;

        public override async Task<bool> SendAsync(Messages messages) => await user.tencentQQApi.GuildSendMessage(messages, _message.Data.ID) == true;
    }
}