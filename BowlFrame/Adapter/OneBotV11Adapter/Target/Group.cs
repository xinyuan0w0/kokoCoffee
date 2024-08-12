using BowlFrame.Exceptions.Permission;
using BowlFrame.Message;
using BowlFrame.Perm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.OneBotV11Adapter.Target
{
    public class Group : BowlFrame.Target.Group
    {
        public Group(string botid, string uuid, Struct.GroupMessage? data = null) : base(uuid)
        {
            _botid = botid;
            _data = data;
            OneBotV11 oneBotV11 = (AdapterManager.GetAdapterWithAccount(_botid) ?? throw new NullReferenceException($"无 {_botid} ID的适配器")) as OneBotV11
                ?? throw new NullReferenceException($"{_botid} 非匹配的适配器");
            oneBotV11Api = new(oneBotV11, this);
            _platform = oneBotV11.Platform;
            permission = new() { Platform = _platform };

            if (_data is null)
                _id = permission.GetPlatformID(uuid).Result?.ID ?? throw new NotFoundTargetPlatform(uuid);
            else
                _id = _data.GroupID;
        }

        private readonly IPlatform _platform;

        public readonly OneBotV11Api oneBotV11Api;
        public readonly Permission permission;

        private readonly string _botid;
        private readonly string _id;
        private readonly Struct.GroupMessage? _data;

        public override string ID => _id;

        public override IPlatform Platform => _platform;

        public override Permission Permission => permission;

        public override async Task<bool> SendAsync(Messages messages) => await oneBotV11Api.SendMessage(messages) == true;
    }
}