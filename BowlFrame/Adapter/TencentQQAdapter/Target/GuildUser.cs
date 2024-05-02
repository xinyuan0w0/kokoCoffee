using BowlFrame.Adapter.TencentQQAdapter.Struct;
using BowlFrame.Exceptions.Permission;
using BowlFrame.Message;
using BowlFrame.Perm;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter.TencentQQAdapter.Target
{
    public class GuildUser : BowlFrame.Target.User
    {
        public GuildUser(string botid, string uuid, string? guildid = null, GuildMsgData? data = null) : base(uuid)
        {
            _botid = botid;
            _data = data;
            _guildid = guildid;
            if (_data is not null && _data is DIRECT_MESSAGE_CREATE_Data data1)
                _chatguildid = data1.GuildID;
            TencentQQ tencentQQ = (AdapterManager.GetAdapterWithAccount(_botid) ?? throw new NullReferenceException($"无 {_botid} ID的适配器")) as TencentQQ
                 ?? throw new NullReferenceException($"{_botid} 非匹配的适配器");
            tencentQQApi = new(tencentQQ, this);
            permission = new() { Platform = platform };

            if (_data is null)
                _id = permission.GetPlatformID(uuid).Result?.ID ?? throw new NotFoundTargetPlatform(uuid);
            else
                _id = _data.Author.ID;
        }

        private static readonly IPlatform platform = new TencentQQ_Offical_Guild();

        private readonly HttpClient _client = new();
        public readonly TencentQQApi tencentQQApi;
        public readonly Permission permission;

        private readonly string _botid;
        private readonly string _id;
        private readonly string? _guildid;
        private readonly GuildMsgData? _data;

        public override string Nickname => _data is not null ? _data.Author.UserName : $"稻穗 #{_id.Remove(6)}#";

        public override byte[] Avatar => _data is not null ? _client.GetByteArrayAsync(_data.Author.Avatar).Result : [];

        public override string ID => _id;

        private string? _chatguildid;

        public string? GuildID
        {
            get
            {
                if (_chatguildid is null)
                {
                    if (_guildid is null)
                        return null;

                    DMS? dms;
                    try
                    {
                        dms = tencentQQApi.CreateGuildPriavte(_guildid).Result;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        return null;
                    }

                    if (dms is null)
                        return null;

                    _chatguildid = dms?.GuildID;

                    return _chatguildid;
                }
                else
                    return _chatguildid;
            }
        }

        public override IPlatform Platform => platform;

        public override Permission Permission => permission;

        public override async Task<bool> SendAsync(Messages messages) => await tencentQQApi.GuildSendMessage(messages) == true;
    }
}