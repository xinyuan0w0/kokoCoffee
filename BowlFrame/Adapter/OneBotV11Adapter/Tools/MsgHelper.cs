using BowlFrame.Adapter.OneBotV11Adapter.Struct;
using BowlFrame.Message;
using BowlFrame.Perm;
using static BowlFrame.Message.Struct.MsgBlock;

namespace BowlFrame.Adapter.OneBotV11Adapter.Tools
{
    internal static class MsgHelper
    {
        public static async Task<Messages> GetMessages(OneBotV11 oneBotV11, MessageBase message)
        {
            IPlatform platform = oneBotV11.Platform;
            Messages messages = new();
            Permission permission = new()
            {
                Platform = platform,
            };

            OnebotToMsgBlock onebotToMsgBlock = new(oneBotV11, platform, permission);

            foreach (Struct.Message msg in message.Messages)
            {
                messages.Add(await onebotToMsgBlock.GetMessageBlock(msg));
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