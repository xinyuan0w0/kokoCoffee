using BowlFrame.Adapter.TencentQQAdapter.Struct;
using BowlFrame.Database.TableStruct;
using BowlFrame.Message;
using BowlFrame.Message.MsgBlocks;
using BowlFrame.Perm;
using Newtonsoft.Json.Linq;
using System.Buffers;
using static BowlFrame.Tools.Logger;
using static BowlFrame.Message.Struct.MsgBlock;

namespace BowlFrame.Adapter.TencentQQAdapter.Tools
{
    internal static class MsgHelper
    {
        //缓存字符
        private static readonly SearchValues<char> s_myChars = SearchValues.Create("@everyone");

        public static async Task<Messages> GetMessages(TencentQQ tencentQQ, MessageData message)
        {
            //屎山警告

            //复制一份
            string content = message.Content;
            //判断平台
            IPlatform platform =
                message is GROUP_AT_MESSAGE_CREATE_Data || message is CommonMsgData
                ? tencentQQ.Platform[0]
                : tencentQQ.Platform[1];

            Messages messages = new();
            Permission permission = new()
            {
                Platform = platform,
            };

            //为群自动添加AtBot前缀
            if (message is GROUP_AT_MESSAGE_CREATE_Data)
                messages.Add(new MessageBlock()
                {
                    HaveMulit = false,
                    MetaType = MetaType.Normal,
                    Name = "AtBot",
                });

            //群和频道At全体
            if (message is GROUP_AT_MESSAGE_CREATE_Data || message is AT_MESSAGE_CREATE_Data)
                if (content.AsSpan().IndexOfAny(s_myChars) != -1)
                {
                    if (message.MentionEveryone)
                    {
                        content = content.Replace("@everyone", null);
                        messages.Add(new MessageBlock()
                        {
                            HaveMulit = false,
                            MetaType = MetaType.Normal,
                            Name = "AtAll",
                        });
                    }
                }

            //寻找<>里内容
            int head, foot = 0;
            string code;
            foot = content.IndexOf('>', foot + 1);
            while (foot != -1)
            {
                if (content.Substring(foot - 1, 1) == "\\")
                {
                    foot = content.IndexOf('>', foot + 1);
                    continue;
                }

                head = content.LastIndexOf('<', foot - 1);
                code = content.Substring(head + 1, foot - head - 1);

                string? replace = null;

                if (code.StartsWith("@!") || code.StartsWith('@') || code.StartsWith("emoji:"))
                    messages.Add(new MessageBlock()
                    {
                        HaveMulit = true,
                        MetaType = MetaType.Normal,
                        Name = "Text",
                        Value = content.Remove(head),
                    });

                //At
                if (code.StartsWith("@!") || code.StartsWith('@'))
                {
                    replace = code.Replace("@!", null).Replace("@", null);
                    if (replace == tencentQQ.ID)
                        messages.Add(new MessageBlock()
                        {
                            HaveMulit = false,
                            MetaType = MetaType.Normal,
                            Name = "AtBot",
                        });
                    else
                    {
                        DbPlatformID? platformID = await permission.FindTarget(replace);
                        messages.Add(new MessageBlock()
                        {
                            HaveMulit = true,
                            MetaType = MetaType.Normal,
                            Name = "At",
                            Value = new At()
                            {
                                UUID = platformID?.UUID,
                                Platform = platform,
                                ID = replace
                            }
                        });
                    }
                }
                //QQ黄豆Emoji
                else if (code.StartsWith("emoji:"))
                {
                    replace = "";
                    messages.Add(new MessageBlock()
                    {
                        HaveMulit = true,
                        MetaType = MetaType.Extra,
                        Name = "QQEmoji",
                        Value = code.Replace("emoji:", null),
                    });
                }

                //替换
                if (replace != null)
                {
                    content = content.Remove(0, foot + 1);
                    foot = 0;
                }

                if (foot + 1 > content.Length) break;
                foot = content.IndexOf('>', foot + 1);
            }

            if (content != "")
                messages.Add(new MessageBlock()
                {
                    HaveMulit = true,
                    MetaType = MetaType.Normal,
                    Name = "Text",
                    Value = content,
                });

            //文件转内嵌
            if (message.Attachments is not null)
                foreach (Attachments attachment in message.Attachments)
                    messages.Add(new MessageBlock()
                    {
                        HaveMulit = true,
                        MetaType = MetaType.Normal,
                        Name = "Picture", //只可能接收图片
                        Value = new Filelink()
                        {
                            MimeType = attachment.MimeType,
                            Url = attachment.Url,
                        }
                    });

            messages.Add(new MessageBlock()
            {
                HaveMulit = false,
                MetaType = MetaType.Normal,
                Name = "RawText",
                Value = message.Content,
            });

            //Log.Debug(JArray.FromObject(messages.MessageBlocks).ToString());

            return messages;
        }
    }
}