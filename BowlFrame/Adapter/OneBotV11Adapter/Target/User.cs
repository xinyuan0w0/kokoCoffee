using BowlFrame.Adapter.OneBotV11Adapter.Struct;
using BowlFrame.Adapter.TencentQQAdapter;
using BowlFrame.Exceptions.Permission;
using BowlFrame.Message;
using BowlFrame.Perm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.OneBotV11Adapter.Target
{
    public class User : BowlFrame.Target.User
    {
        public User(string botid, string uuid, MessageBase? data = null) : base(uuid)
        {
            _botid = botid;
            _data = data;
            OneBotV11 oneBotV11 = (AdapterManager.GetAdapterWithAccount(_botid) ?? throw new NullReferenceException($"无 {_botid} ID的适配器")) as OneBotV11
                ?? throw new NullReferenceException($"{_botid} 非匹配的适配器");

            oneBotV11Api = new(oneBotV11, this);
            permission = new() { Platform = platform };

            if (_data is null)
                _id = permission.GetPlatformID(uuid).Result?.ID ?? throw new NotFoundTargetPlatform(uuid);
            else
                _id = _data.UserID;
        }

        private static readonly IPlatform platform = new TencentQQ_Common();

        private readonly HttpClient _client = new();
        public readonly OneBotV11Api oneBotV11Api;
        public readonly Permission permission;

        private readonly string _botid;
        private readonly string _id;
        private readonly MessageBase? _data;

        public override string ID => _id;

        public override IPlatform Platform => platform;

        public override Permission Permission => permission;

        public override string Nickname => _data is not null ? _data.Sender.Nickname : $"稻穗 #{_id.Remove(6)}#";

        public override byte[] Avatar => _client.GetByteArrayAsync($"http://q.qlogo.cn/headimg_dl?dst_uin={_id}&spec=640&img_type=jpg").Result;

        public override async Task<bool> SendAsync(Messages messages) => await oneBotV11Api.SendMessage(messages) == true;
    }
}