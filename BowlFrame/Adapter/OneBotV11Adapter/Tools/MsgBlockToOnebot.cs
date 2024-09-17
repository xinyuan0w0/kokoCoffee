using BowlFrame.Exceptions.Permission;
using BowlFrame.Message.MsgBlocks;
using BowlFrame.Perm;
using BowlFrame.Target;
using Newtonsoft.Json.Linq;
using static BowlFrame.Message.Struct.MsgBlock;

namespace BowlFrame.Adapter.OneBotV11Adapter.Tools
{
    internal class MsgBlockToOnebot(OneBotV11 oneBotV11, Permission permission, ITarget target)
    {
        private readonly OneBotV11 _oneBotV11 = oneBotV11;
        private readonly Permission _permission = permission;

        public async Task<JArray?> CreateNormalContent(MessageBlock messageBlock)
        {
            JArray content = [];

            switch (messageBlock.Name)
            {
                case "Text":
                    content.Add(CreateJsonObject("text", new { text = (string?)messageBlock.Value }));
                    break;

                case "AtBot":
                    content.Add(CreateJsonObject("at", new { qq = _oneBotV11.AccountID }));
                    break;

                case "At":
                    var ats = (At[]?)messageBlock.Value;
                    if (ats != null)
                    {
                        foreach (var at in ats)
                        {
                            var platformId = await _permission.GetPlatformID(at.UUID) ?? throw new NotFoundTargetPlatform(at.UUID);
                            content.Add(target is User
                                ? CreateJsonObject("text", new { text = $"@{platformId.ID} " })
                                : CreateJsonObject("at", new { qq = platformId.ID }));
                        }
                    }
                    break;

                case "AtAll":
                    content.Add(CreateJsonObject("at", new { qq = "all" }));
                    break;

                case "Voice" or "Picture" or "Vidio":
                    if (messageBlock.Value is byte[] data && data.Length > 0)
                    {
                        content.Add(CreateJsonObject(messageBlock.Name switch
                        {
                            "Voice" => "record",
                            "Picture" => "image",
                            "Vidio" => "video",
                            _ => ""
                        }, new { file = "base64://" + Convert.ToBase64String(data) }));
                    }
                    break;
            }

            return content.Count > 0 ? content : null;
        }

        private static JObject CreateJsonObject(string type, object data)
        {
            return JObject.FromObject(new { type, data });
        }
    }
}