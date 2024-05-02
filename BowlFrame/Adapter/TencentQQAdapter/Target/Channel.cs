using BowlFrame.Adapter.TencentQQAdapter.Struct;
using BowlFrame.Exceptions.Permission;
using BowlFrame.Message;
using BowlFrame.Perm;

namespace BowlFrame.Adapter.TencentQQAdapter.Target
{
    public class Channel : BowlFrame.Target.Channel
    {
        public Channel(string botid, string uuid, string fatheruuid, AT_MESSAGE_CREATE_Data? data = null) : base(uuid, fatheruuid)
        {
            _botid = botid;
            _data = data;
            TencentQQ tencentQQ = (AdapterManager.GetAdapterWithAccount(_botid) ?? throw new NullReferenceException($"无 {_botid} ID的适配器")) as TencentQQ
                 ?? throw new NullReferenceException($"{_botid} 非匹配的适配器");
            tencentQQApi = new(tencentQQ, this);
            permission = new() { Platform = platform };

            if (_data is null)
                _id = permission.GetPlatformID(uuid).Result?.ID ?? throw new NotFoundTargetPlatform(uuid);
            else
                _id = _data.ChannelID;
        }

        private static readonly IPlatform platform = new TencentQQ_Offical_Guild();

        public readonly Permission permission;
        public readonly TencentQQApi tencentQQApi;

        private readonly string _id;
        private readonly string _botid;
        private readonly AT_MESSAGE_CREATE_Data? _data;

        public override string ID => _id;

        public override IPlatform Platform => platform;

        public override Permission Permission => permission;

        public override async Task<bool> SendAsync(Messages messages) => await tencentQQApi.GuildSendMessage(messages) == true;
    }
}