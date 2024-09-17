using BowlFrame.Message.MsgBlocks;
using BowlFrame.Message;
using BowlFrame.Perm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BowlFrame.Message.Struct.MsgBlock;
using Newtonsoft.Json.Linq;

namespace BowlFrame.Adapter.OneBotV11Adapter.Tools
{
    internal class OnebotToMsgBlock(OneBotV11 oneBotV11, IPlatform platform, Permission permission)
    {
        private readonly OneBotV11 _oneBotV11 = oneBotV11;
        private readonly IPlatform _platform = platform;
        private readonly Permission _permission = permission;

        public async Task<MessageBlock> GetMessageBlock(Struct.Message msg) => msg switch
        {
            { Type: "text", Data: JObject data } => CreateMessageBlock("Text", data["text"]?.ToString()),
            { Type: "image", Data: JObject data } => CreateMessageBlock("Picture", new Filelink { Url = data["file"]?.ToString() }),
            { Type: "record", Data: JObject data } => CreateMessageBlock("Voice", new Filelink { Url = data["file"]?.ToString() }),
            { Type: "video", Data: JObject data } => CreateMessageBlock("Video", new Filelink { Url = data["file"]?.ToString() }),
            { Type: "at", Data: JObject data } when (data["qq"]?.ToString() ?? "Null") == _oneBotV11.AccountID => CreateMessageBlock("AtBot"),
            { Type: "at", Data: JObject data } => CreateMessageBlock("At", new At { ID = data["qq"]?.ToString() ?? "Null", Platform = _platform, UUID = (await _permission.FindTarget(data["qq"]?.ToString() ?? "Null", _platform))?.UUID }),
            _ => CreateMessageBlock(msg.Type, msg.Data, MetaType.Extra)
        };
    }
}