using BowlFrame.Adapter.TencentQQAdapter.Target;
using BowlFrame.Adapter.TencentQQAdapter.Tools;
using BowlFrame.Exceptions.Permission;
using BowlFrame.Message;
using BowlFrame.Message.MsgBlocks;
using BowlFrame.Perm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using System.Text;
using static BowlFrame.Message.Struct.MsgBlock;

namespace BowlFrame.Adapter.TencentQQAdapter
{
    public class TencentQQApi(TencentQQ tencentQQ, BowlFrame.Target.ITarget target, bool isChannel = false)
    {
        private readonly TencentQQ tencentQQ = tencentQQ;
        private readonly BowlFrame.Target.ITarget target = target;
        private readonly Permission permission = new() { Platform = isChannel ? tencentQQ.Platform[1] : tencentQQ.Platform[0] };

        private readonly JsonSerializerSettings jsonSerializerSettings = new()
        {
            MissingMemberHandling = MissingMemberHandling.Error
        };

        private int count = 0;

        public async Task<bool?> SendMessage(Messages messages, string? msgid = null)
        {
            //判断是否为支持的对象
            if (target is not Group && target is not User)
                throw new NotSupportedException(target.GetType().Name);

            var contents = new List<JObject>();
            var textContent = await BuildTextContent(messages);

            if (!string.IsNullOrEmpty(textContent))
            {
                contents.Add(new JObject
                {
                    { "msg_type", 0 },
                    { "content", textContent }
                });
            }

            contents.AddRange(await BuildMediaContents(messages));

            return await SendContents(contents, msgid);
        }

        private async Task<string> BuildTextContent(Messages messages)
        {
            var text = new StringBuilder();

            foreach (var messageBlock in messages.MessageBlocks)
            {
                switch (messageBlock.MetaType)
                {
                    case MetaType.Normal:
                        switch (messageBlock.Name)
                        {
                            case "Text":
                                text.Append(messageBlock.Value as string);
                                break;

                            case "AtBot":
                                text.Append($"@{tencentQQ.Nickname} ");
                                break;

                            case "At":
                                var ats = (At[]?)messageBlock.Value;
                                if (ats != null)
                                {
                                    foreach (var at in ats)
                                    {
                                        var platformId = await permission.GetPlatformID(at.UUID)
                                                        ?? throw new NotFoundTargetPlatform(at.UUID);
                                        text.Append(target is User
                                            ? $"@{platformId.ID} "
                                            : $"<@{platformId.ID}> ");
                                    }
                                }
                                break;

                            case "AtAll":
                                text.Append($"@everyone ");
                                break;
                        }
                        break;
                }
            }

            return text.ToString();
        }

        private async Task<IEnumerable<JObject>> BuildMediaContents(Messages messages)
        {
            var mediaContents = new List<JObject>();

            foreach (var messageBlock in messages.MessageBlocks)
            {
                if (messageBlock.MetaType != MetaType.Normal)
                    continue;

                if (messageBlock.Value is null || (byte[])messageBlock.Value == Array.Empty<byte>())
                    continue;

                short mediaType = messageBlock.Name switch
                {
                    "Voice" => 3,
                    "Picture" => 1,
                    "Vidio" => 2,
                    _ => 0
                };

                if (mediaType > 0)
                {
                    mediaContents.Add(new JObject
                    {
                        { "msg_type", 7 },
                        { "content", " " },
                        { "media", JObject.FromObject(await UploadMedia.GetFiles(tencentQQ, (byte[])messageBlock.Value, mediaType, target)) }
                    });
                }
            }

            return mediaContents;
        }

        private async Task<bool?> SendContents(List<JObject> contents, string? msgid = null)
        {
            foreach (var content in contents)
            {
                if (msgid is not null)
                    content.Add("msg_id", msgid);

                content.Add("msg_seq", ++count);

                StringContent requestContent = new(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
                HttpRequestMessage requestMessage = new(HttpMethod.Post,
                    new Uri(tencentQQ.BaseUrl, $"/v2/{(target is User ? "users" : "groups")}/{target.ID}/messages"))
                {
                    Content = requestContent
                };

                HttpResponseMessage? responseMessage = await tencentQQ.Send(requestMessage);

                if (responseMessage == null)
                    return null;
            }

            return true;
        }

        public async Task<bool?> GuildSendMessage(Messages messages, string? msgid = null)
        {
            //判断是否为支持的对象
            if (target is not Channel && target is not GuildUser)
                throw new NotSupportedException(target.GetType().Name);

            List<HttpContent> contents = [];
            byte[]? firstPicture = null;
            string textContent = await BuildGuildTextContent(messages);

            foreach (MessageBlock messageBlock in messages.MessageBlocks)
            {
                if (messageBlock.MetaType != MetaType.Normal || messageBlock.Name != "Picture")
                    continue;

                byte[]? pictureData = (byte[]?)messageBlock.Value;

                if (firstPicture == null)
                    firstPicture = pictureData ?? [];
                else if (pictureData != null && pictureData.Length > 0)
                    contents.Add(CreatePictureContent(pictureData, msgid));
            }

            contents.Insert(0, CreateContent(textContent, firstPicture, msgid));

            return await SendContents(contents);
        }

        private async Task<string> BuildGuildTextContent(Messages messages)
        {
            var text = new StringBuilder();

            foreach (var messageBlock in messages.MessageBlocks)
            {
                if (messageBlock.MetaType != MetaType.Normal)
                    continue;

                switch (messageBlock.Name)
                {
                    case "Text":
                        text.Append(messageBlock.Value as string);
                        break;

                    case "AtBot":
                        text.Append($"@{tencentQQ.Nickname} ");
                        break;

                    case "At":
                        var ats = (At[]?)messageBlock.Value;
                        if (ats != null)
                        {
                            foreach (var at in ats)
                            {
                                var platformId = await permission.GetPlatformID(at.UUID)
                                                    ?? throw new NotFoundTargetPlatform(at.UUID);
                                text.Append(target is User
                                    ? $"@{platformId.ID} "
                                    : $"<@{platformId.ID}> ");
                            }
                        }
                        break;

                    case "AtAll":
                        text.Append($"@everyone ");
                        break;
                }
            }

            return text.ToString();
        }

        private static HttpContent CreateContent(string text, byte[]? pictureData, string? msgid)
        {
            if (pictureData != null && pictureData.Length > 0)
            {
                var content = new MultipartFormDataContent
                {
                    { new StringContent(text), "content" },
                    { new ByteArrayContent(pictureData), "file_image",
                        $"{BowlFrame.Tools.Tools.GetMD5Hex(pictureData)}.{BowlFrame.Tools.Tools.GetMimeType(pictureData).Item2}" }
                };

                if (msgid != null)
                    content.Add(new StringContent(msgid), "msg_id");

                return content;
            }
            else
            {
                return JsonContent.Create(new
                {
                    content = text,
                    msg_id = msgid
                });
            }
        }

        private static MultipartFormDataContent CreatePictureContent(byte[] pictureData, string? msgid)
        {
            var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(pictureData), "file_image",
                    $"{BowlFrame.Tools.Tools.GetMD5Hex(pictureData)}.{BowlFrame.Tools.Tools.GetMimeType(pictureData).Item2}" }
            };

            if (msgid != null)
                content.Add(new StringContent(msgid), "msg_id");

            return content;
        }

        private async Task<bool?> SendContents(List<HttpContent> contents)
        {
            foreach (var content in contents)
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, GetRequestUri())
                {
                    Content = content
                };

                var responseMessage = await tencentQQ.Send(requestMessage);

                if (responseMessage == null)
                {
                    return null;
                }
            }

            return true;
        }

        private Uri GetRequestUri()
        {
            if (target is GuildUser user)
            {
                if (user.GuildID == null)
                {
                    throw new InvalidOperationException("GuildUser's GuildID cannot be null.");
                }

                return new Uri(tencentQQ.BaseUrl, $"/dms/{user.GuildID}/messages");
            }
            else if (target is Channel channel)
            {
                return new Uri(tencentQQ.BaseUrl, $"/channels/{channel.ID}/messages");
            }
            else
            {
                throw new NotSupportedException(target.GetType().Name);
            }
        }

        public async Task<DMS?> CreateGuildPriavte(string guildid)
        {
            if (target is not User)
                throw new NotSupportedException(target.GetType().Name);

            HttpRequestMessage requestMessage = new(HttpMethod.Post, new Uri(tencentQQ.BaseUrl, "/users/@me/dms"))
            {
                Content = JsonContent.Create(new
                {
                    recipient_id = target.ID,
                    source_guild_id = guildid,
                })
            };

            HttpResponseMessage? responseMessage = await tencentQQ.Send(requestMessage);

            if (responseMessage is null)
                return null;

            return JsonConvert.DeserializeObject<DMS>(await responseMessage.Content.ReadAsStringAsync(), jsonSerializerSettings);
        }
    }
}