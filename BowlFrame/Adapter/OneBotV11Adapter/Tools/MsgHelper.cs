using BowlFrame.Adapter.OneBotV11Adapter.Struct;
using BowlFrame.Message;
using BowlFrame.Message.Struct;
using BowlFrame.Perm;

namespace BowlFrame.Adapter.OneBotV11Adapter.Tools
{
    internal static class MsgHelper
    {
        //平台
        private readonly static IPlatform _platform = new TencentQQ_Common();

        //TODO: 拆！！！
        public static async Task<Messages> GetMessages(OneBotV11 oneBotV11, MessageBase message)
        {
            Messages messages = new();
            Permission permission = new()
            {
                Platform = _platform,
            };
            foreach (Struct.Message msg in message.Messages)
            {
                switch (msg.Type)
                {
                    case "text":
                        messages.Add(new MessageBlock
                        {
                            HaveMulit = true,
                            MetaType = MetaType.Normal,
                            Name = "Text",
                            Value = (string?)msg.Data["text"],
                        });
                        break;

                    case "image":
                        messages.Add(new MessageBlock
                        {
                            HaveMulit = true,
                            MetaType = MetaType.Normal,
                            Name = "Picture",
                            Value = new Filelink
                            {
                                Url = (string?)msg.Data["file"],
                            },
                        });
                        break;

                    case "record":
                        messages.Add(new MessageBlock
                        {
                            HaveMulit = true,
                            MetaType = MetaType.Normal,
                            Name = "Voice",
                            Value = new Filelink
                            {
                                Url = (string?)msg.Data["file"],
                            },
                        });
                        break;

                    case "video":
                        messages.Add(new MessageBlock
                        {
                            HaveMulit = true,
                            MetaType = MetaType.Normal,
                            Name = "Video",
                            Value = new Filelink
                            {
                                Url = (string?)msg.Data["file"],
                            },
                        });
                        break;

                    case "at":

                        if ((string?)msg.Data["qq"] == oneBotV11.AccountID)
                            messages.Add(new MessageBlock
                            {
                                HaveMulit = false,
                                MetaType = MetaType.Normal,
                                Name = "AtBot"
                            });
                        else
                            messages.Add(new MessageBlock
                            {
                                HaveMulit = true,
                                MetaType = MetaType.Normal,
                                Name = "At",
                                Value = new At
                                {
                                    ID = (string?)msg.Data["qq"] ?? throw new NullReferenceException(),
                                    Platform = _platform,
                                    UUID = (await permission.FindTarget((string?)msg.Data["qq"] ?? throw new NullReferenceException(), _platform))?.UUID
                                },
                            });
                        break;

                    default:
                        messages.Add(new MessageBlock
                        {
                            HaveMulit = true,
                            MetaType = MetaType.Extra,
                            Name = msg.Type,
                            Value = msg.Data,
                        });

                        break;
                }
            }

            messages.Add(new MessageBlock()
            {
                HaveMulit = false,
                MetaType = MetaType.Normal,
                Name = "RawText",
                Value = message.RawMessage,
            });
            return messages;
        }
    }
}