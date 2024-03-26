using BowlFrame.Adapter.TencentQQAdapter.Struct;
using BowlFrame.Adapter.TencentQQAdapter.Target;
using BowlFrame.Database.TableStruct;
using BowlFrame.Message;
using BowlFrame.Perm;

namespace BowlFrame.Adapter.TencentQQAdapter.Event.Message
{
    public class ChannelMessage : BowlFrame.Event.Message.ChannelMessage
    {
        private readonly TencentQQ _tencentQQ;

        private readonly AT_MESSAGE_CREATE _message;

        private readonly Permission permission = new() { Platform = platform };

        private static readonly IPlatform platform = new TencentQQ_Offical_Guild();

        public ChannelMessage(TencentQQ tencentQQ, AT_MESSAGE_CREATE message) : base(platform)
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

            _user = new(tencentQQ.AccountID, uuid, message.Data.GuildID, data: message.Data);

            platformID = permission.FindTarget(message.Data.GuildID).Result ?? throw new NullReferenceException();
            if (platformID == DbPlatformID.Empty)
            {
                uuid = permission.CreateTarget(message.Data.GuildID, TargetType.Guild).Result ?? throw new NullReferenceException();
                if (uuid == "")
                    throw new NullReferenceException();
            }
            else
                uuid = platformID.UUID;

            _guild = new(tencentQQ.AccountID, uuid, data: message.Data);

            platformID = permission.FindTarget(message.Data.ChannelID).Result ?? throw new NullReferenceException();
            if (platformID == DbPlatformID.Empty)
            {
                uuid = permission.CreateTarget(message.Data.ChannelID, TargetType.Channel, fatherUUID: _guild.UUID).Result ?? throw new NullReferenceException();
                if (uuid == "")
                    throw new NullReferenceException();
            }
            else
                uuid = platformID.UUID;

            _channel = new(tencentQQ.AccountID, uuid, _guild.UUID, data: message.Data);
        }

        public override string Room => _message.Data.ChannelID;

        private readonly GuildUser _user;

        public override GuildUser User => _user;

        private readonly Guild _guild;

        public override Guild Guild => _guild;

        private readonly Channel _channel;

        public override Channel Channel => _channel;

        public override string RawMessage => _message.Data.Content;

        public override string Target => _message.Data.Author.ID;

        public override async Task<bool> SendAsync(Messages messages) => await _channel.tencentQQApi.GuildSend(messages, _message.Data.ID) == true;
    }
}