using BowlFrame.Adapter.TencentQQAdapter.Struct;
using BowlFrame.Exceptions.Permission;
using BowlFrame.Message;
using BowlFrame.Perm;
using System.Runtime.InteropServices;

namespace BowlFrame.Adapter.TencentQQAdapter.Target
{
    public class User : BowlFrame.Target.User
    {
        public User(string botid, string uuid, CommonMsgData? data = null) : base(uuid)
        {
            _botid = botid;
            _data = data;
            TencentQQ tencentQQ = (AdapterManager.GetAdapterWithAccount(_botid) ?? throw new NullReferenceException($"无 {_botid} ID的适配器")) as TencentQQ
                ?? throw new NullReferenceException($"{_botid} 非匹配的适配器");
            tencentQQApi = new(tencentQQ, this);
            _platform = tencentQQ.Platform[0];
            permission = new() { Platform = _platform };

            if (_data is null)
                _openid = permission.GetPlatformID(uuid).Result?.ID ?? throw new NotFoundTargetPlatform(uuid);
            else
                _openid = _data.Author.OpenID;
        }

        private readonly IPlatform _platform;

        private readonly HttpClient _client = new();
        public readonly TencentQQApi tencentQQApi;
        public readonly Permission permission;

        private readonly string _botid;
        private readonly string _openid;
        private readonly CommonMsgData? _data;

        public override string Nickname => $"稻穗 #{_openid.Remove(6)}#";

        //https://q.qlogo.cn/qqapp/{这里写你的机器人id}/{这里写你的机器人对应对方的openid}/640
        public override byte[] Avatar => _client.GetByteArrayAsync($"https://q.qlogo.cn/qqapp/{_botid}/{_openid}/640").Result;

        public override string ID => _openid;

        public override IPlatform Platform => _platform;

        public override Permission Permission => permission;

        public override async Task<bool> SendAsync(Messages messages) => await tencentQQApi.SendMessage(messages) == true;
    }
}