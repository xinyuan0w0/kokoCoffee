using BowlFrame.Adapter.TencentQQAdapter.Target;
using BowlFrame.Adapter.TencentQQAdapter.Tools;
using BowlFrame.Exceptions.Permission;
using BowlFrame.Message;
using BowlFrame.Message.Struct;
using BowlFrame.Perm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using System.Text;

namespace BowlFrame.Adapter.TencentQQAdapter
{
    public class TencentQQApi(TencentQQ tencentQQ, BowlFrame.Target.ITarget target, bool isChannel = false)
    {
        private readonly TencentQQ tencentQQ = tencentQQ;
        private readonly BowlFrame.Target.ITarget target = target;
        private readonly Permission permission = new() { Platform = isChannel ? new TencentQQ_Offical_Guild() : new TencentQQ_Offical_Common() };

        private readonly JsonSerializerSettings jsonSerializerSettings = new()
        {
            MissingMemberHandling = MissingMemberHandling.Error
        };

        private static JsonSerializerSettings settings = new()
        {
            StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
        };

        private int count = 0;

        public async Task<bool?> Send(Messages messages, string? msgid = null)
        {
            //判断是否为支持的对象
            if (target is not Group && target is not User)
                throw new NotSupportedException(target.GetType().Name);

            List<JObject> contents = [];
            StringBuilder text = new();

            foreach (MessageBlock messageBlock in messages.MessageBlocks)
            {
                JObject? content = null;
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
                                At[]? ats = (At[]?)messageBlock.Value;

                                if (ats is not null)
                                    foreach (At at in ats)
                                        if (target is User)
                                            text.Append($"@{(await permission.GetPlatformID(at.UUID) ?? throw new NotFoundTargetPlatform(at.UUID)).ID} ");
                                        else
                                            text.Append($"<@{(await permission.GetPlatformID(at.UUID) ?? throw new NotFoundTargetPlatform(at.UUID)).ID}> ");

                                break;

                            case "AtAll":
                                text.Append($"@everyone ");
                                break;

                            case "Voice":
                                if (messageBlock.Value is null || (byte[])messageBlock.Value == Array.Empty<byte>())
                                    break;
                                content = JObject.FromObject(new
                                {
                                    msg_type = 7,
                                    content = " ",
                                    media = await UploadMedia.GetFiles(tencentQQ, (byte[])messageBlock.Value, 3, target),
                                });
                                break;

                            case "Picture":
                                if (messageBlock.Value is null || (byte[])messageBlock.Value == Array.Empty<byte>())
                                    break;
                                content = JObject.FromObject(new
                                {
                                    msg_type = 7,
                                    content = " ",
                                    media = await UploadMedia.GetFiles(tencentQQ, (byte[])messageBlock.Value, 1, target),
                                });
                                break;

                            case "Vidio":
                                if (messageBlock.Value is null || (byte[])messageBlock.Value == Array.Empty<byte>())
                                    break;
                                content = JObject.FromObject(new
                                {
                                    msg_type = 7,
                                    content = " ",
                                    media = await UploadMedia.GetFiles(tencentQQ, (byte[])messageBlock.Value, 2, target),
                                });
                                break;

                            default:
                                break;
                        }
                        break;

                    case MetaType.Extra:
                        //施工中
                        break;
                }

                if (content is not null)
                    contents.Add(content);
            }

            if (text.Length != 0)
            {
                contents.Insert(0, JObject.FromObject(new
                {
                    msg_type = 0,
                    content = text.ToString(),
                }));
            }

            foreach (JObject content in contents)
            {
                if (msgid is not null)
                    content.Add("msg_id", msgid);

                content.Add("msg_seq", ++count);

                bool? result = await _send(new StringContent(JsonConvert.SerializeObject(content, settings), Encoding.UTF8, "application/json"));
                if (result != true)
                    return result;
            }

            return true;

            async Task<bool?> _send(HttpContent content)
            {
                HttpRequestMessage requestMessage = new(HttpMethod.Post, new Uri(tencentQQ.BaseUrl, $"/v2/{(target is User ? "users" : "groups")}/{target.ID}/messages"))
                {
                    Content = content
                };

                HttpResponseMessage? responseMessage = await tencentQQ.Send(requestMessage);

                if (responseMessage is null)
                    return null;

                return true;
            }
        }

        public async Task<bool?> GuildSend(Messages messages, string? msgid = null)
        {
            //判断是否为支持的对象
            if (target is not Channel && target is not GuildUser)
                throw new NotSupportedException(target.GetType().Name);

            List<HttpContent> contents = [];
            byte[]? firstpic = null;
            StringBuilder text = new();

            foreach (MessageBlock messageBlock in messages.MessageBlocks)
            {
                HttpContent? content = null;
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
                                At[]? ats = (At[]?)messageBlock.Value;

                                if (ats is not null)
                                    foreach (At at in ats)
                                        if (target is User)
                                            text.Append($"@{(await permission.GetPlatformID(at.UUID) ?? throw new NotFoundTargetPlatform(at.UUID)).ID} ");
                                        else
                                            text.Append($"<@{(await permission.GetPlatformID(at.UUID) ?? throw new NotFoundTargetPlatform(at.UUID)).ID}> ");

                                break;

                            case "AtAll":
                                text.Append($"@everyone ");
                                break;

                            case "Voice":
                                //不支持
                                break;

                            case "Picture":
                                byte[]? pic = (byte[]?)messageBlock.Value;

                                //第一张图随文本发送
                                if (firstpic is null)
                                {
                                    //不随文本请置空第一张图
                                    firstpic = pic ?? [];
                                    break;
                                }

                                if (pic is null || pic == Array.Empty<byte>())
                                    break;

                                content = new MultipartFormDataContent
                                {
                                    { new ByteArrayContent(pic),"file_image",BowlFrame.Tools.Tools.GetMD5Hex(pic)+"."+BowlFrame.Tools.Tools.GetMimeType(pic).Item2 },
                                };

                                if (msgid is not null)
                                    ((MultipartFormDataContent)content).Add(new StringContent(msgid), "msg_id");
                                break;

                            case "Vidio":
                                //不支持
                                break;

                            default:
                                break;
                        }
                        break;

                    case MetaType.Extra:
                        //施工中
                        break;
                }

                //添加到列表中
                if (content is not null)
                    contents.Add(content);
            }

            if (text.Length != 0)
            {
                if (firstpic is null || firstpic == Array.Empty<byte>())
                {
                    JsonContent jsonContent;
                    jsonContent = JsonContent.Create(new
                    {
                        content = text.ToString(),
                        msg_id = msgid,
                    });
                    contents.Insert(0, jsonContent);
                }
                else
                {
                    MultipartFormDataContent content = new()
                    {
                        { new StringContent(text.ToString()), "content" },
                        { new ByteArrayContent(firstpic),"file_image",BowlFrame.Tools.Tools.GetMD5Hex(firstpic)+"."+BowlFrame.Tools.Tools.GetMimeType(firstpic).Item2 },
                    };

                    if (msgid is not null)
                        content.Add(new StringContent(msgid), "msg_id");

                    contents.Insert(0, content);
                }
            }
            else if (firstpic is not null)
            {
                MultipartFormDataContent content = new()
                    {
                        { new ByteArrayContent(firstpic),"file_image",BowlFrame.Tools.Tools.GetMD5Hex(firstpic)+"."+BowlFrame.Tools.Tools.GetMimeType(firstpic).Item2 },
                    };

                if (msgid is not null)
                    content.Add(new StringContent(msgid), "msg_id");

                contents.Insert(0, content);
            }

            foreach (HttpContent content in contents)
            {
                bool? result = await _send(content);
                if (result != true)
                    return result;
            }

            return true;

            async Task<bool?> _send(HttpContent content)
            {
                HttpRequestMessage requestMessage;

                if (target is GuildUser user)
                {
                    if (user.GuildID is null)
                        return false;

                    requestMessage = new(HttpMethod.Post, new Uri(tencentQQ.BaseUrl, $"/dms/{user.GuildID}/messages"));
                }
                else if (target is Channel channel)
                {
                    requestMessage = new(HttpMethod.Post, new Uri(tencentQQ.BaseUrl, $"/channels/{channel.ID}/messages"));
                }
                else
                {
                    throw new NotSupportedException(target.GetType().Name);
                }

                requestMessage.Content = content;

                HttpResponseMessage? responseMessage = await tencentQQ.Send(requestMessage);

                if (responseMessage is null)
                    return null;

                return true;
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