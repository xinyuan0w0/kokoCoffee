using BowlFrame.Adapter.TencentQQAdapter.Struct;
using BowlFrame.Adapter.TencentQQAdapter.Target;
using BowlFrame.Database.TableStruct;
using BowlFrame.Message;
using BowlFrame.Perm;
using System.Runtime.InteropServices;

namespace BowlFrame.Adapter.TencentQQAdapter.Event.Message
{
    public class ChannelMessage : BowlFrame.Event.Message.ChannelMessage
    {
        //private readonly TencentQQ _tencentQQ;

        private readonly AT_MESSAGE_CREATE _message;

        private readonly Permission _permission;

        private readonly IPlatform _platform;

        public ChannelMessage(TencentQQ tencentQQ, AT_MESSAGE_CREATE message) : base(tencentQQ.Platform[1])
        {
            //_tencentQQ = tencentQQ;
            _message = message;
            _platform = tencentQQ.Platform[1];
            _permission = new() { Platform = _platform };
            Messages = Tools.MsgHelper.GetMessages(tencentQQ, message.Data).Result;

            //小屎山
            string uuid;

            //用户
            //从数据库寻找对象
            DbPlatformID platformID = _permission.FindTarget(message.Data.Author.ID).Result ?? throw new NullReferenceException();

            uuid = platformID == DbPlatformID.Empty
                //创建对象
                ? _permission.CreateTarget(message.Data.Author.ID, TargetType.User).Result ?? throw new NullReferenceException()
                : platformID.UUID;

            //初始化对象
            user = new(tencentQQ.AccountID, uuid, message.Data.GuildID, data: message.Data);

            //频道
            platformID = _permission.FindTarget(message.Data.GuildID).Result ?? throw new NullReferenceException();

            uuid = platformID == DbPlatformID.Empty
                ? _permission.CreateTarget(message.Data.GuildID, TargetType.Guild).Result ?? throw new NullReferenceException()
                : platformID.UUID;

            guild = new(tencentQQ.AccountID, uuid, data: message.Data);

            //子频道
            platformID = _permission.FindTarget(message.Data.ChannelID).Result ?? throw new NullReferenceException();
            uuid = platformID == DbPlatformID.Empty
                ? _permission.CreateTarget(message.Data.ChannelID, TargetType.Channel, fatherUUID: guild.UUID).Result ?? throw new NullReferenceException()
                : platformID.UUID;

            channel = new(tencentQQ.AccountID, uuid, guild.UUID, data: message.Data);

            permission = new(user.UUID, linkTarget: new(channel.UUID, linkTarget: new(guild.UUID) { Platform = _platform }) { Platform = _platform }) { Platform = _platform };
        }

        public override string Room => _message.Data.ChannelID;

        private readonly Permission permission;

        public override Permission Permission => permission;

        private readonly GuildUser user;

        public override GuildUser User => user;

        private readonly Guild guild;

        public override Guild Guild => guild;

        private readonly Channel channel;

        public override Channel Channel => channel;

        public override string RawMessage => _message.Data.Content;

        public override string Target => _message.Data.Author.ID;

        public override async Task<bool> SendAsync(Messages messages) => await channel.tencentQQApi.GuildSendMessage(messages, _message.Data.ID) == true;
    }
}