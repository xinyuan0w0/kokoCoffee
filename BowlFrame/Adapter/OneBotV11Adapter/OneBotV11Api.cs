using BowlFrame.Adapter.OneBotV11Adapter.Target;
using BowlFrame.Adapter.TencentQQAdapter;
using BowlFrame.Adapter.TencentQQAdapter.Tools;
using BowlFrame.Exceptions.Permission;
using BowlFrame.Message;
using BowlFrame.Message.MsgBlocks;
using BowlFrame.Perm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BowlFrame.Adapter.OneBotV11Adapter
{
    public class OneBotV11Api
    {
        private readonly OneBotV11 oneBotV11;
        private readonly BowlFrame.Target.ITarget target;
        private readonly Permission permission;

        //private readonly JsonSerializerSettings jsonSerializerSettings = new()
        //{
        //    MissingMemberHandling = MissingMemberHandling.Error
        //};

        public OneBotV11Api(OneBotV11 oneBotV11, BowlFrame.Target.ITarget target)
        {
            permission = new() { Platform = oneBotV11.Platform };

            if (target is not Group && target is not User)
                throw new NotSupportedException();
            this.oneBotV11 = oneBotV11;
            this.target = target;
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <box name="value">值</box>
        /// <box name="func">功能</box>
        /// <box name="WebSocket">使用 WebSocket 发送</box>
        /// <returns></returns>
        public async Task<JObject?> Send(JObject value, string? func = null, bool? WebSocket = false, CancellationToken cancellationToken = default)
        {
            JObject content;

            if (func is not null)
            {
                content = [];
                content.Add("action", JValue.CreateString(func));
                content.Add("params", value);
            }
            else
            {
                content = value;
            }

            if (WebSocket == true)
            {
                await oneBotV11.WebSocketSend(content.ToString(), cancellationToken);
                //没写
                return [];
            }
            else
            {
                HttpResponseMessage httpResponse = await oneBotV11.HttpSend(content.ToString(), cancellationToken: cancellationToken);

                if (httpResponse.IsSuccessStatusCode)
                    return new JObject(await httpResponse.Content.ReadAsStringAsync(cancellationToken));
                else
                    return null;
            }
        }

        //TODO: 拆分拆分拆分！！！！
        public async Task<bool?> SendMessage(Messages messages)
        {
            JArray mainContent = [];
            List<JArray> contents = [];

            foreach (MessageBlock messageBlock in messages.MessageBlocks)
            {
                JArray? content = null;
                switch (messageBlock.MetaType)
                {
                    case MetaType.Normal:
                        switch (messageBlock.Name)
                        {
                            case "Text":
                                mainContent.Add(JObject.FromObject(new
                                {
                                    type = "text",
                                    data = new
                                    {
                                        text = (string?)messageBlock.Value,
                                    }
                                }));
                                break;

                            case "AtBot":
                                mainContent.Add(JObject.FromObject(new
                                {
                                    type = "at",
                                    data = new
                                    {
                                        qq = oneBotV11.AccountID,
                                    }
                                }));
                                break;

                            case "At":
                                At[]? ats = (At[]?)messageBlock.Value;

                                if (ats is not null)
                                    foreach (At at in ats)
                                        if (target is User)
                                            mainContent.Add(JObject.FromObject(new
                                            {
                                                type = "text",
                                                data = new
                                                {
                                                    text = $"@{(await permission.GetPlatformID(at.UUID) ?? throw new NotFoundTargetPlatform(at.UUID)).ID} ",
                                                }
                                            }));
                                        else
                                            mainContent.Add(JObject.FromObject(new
                                            {
                                                type = "at",
                                                data = new
                                                {
                                                    qq = (await permission.GetPlatformID(at.UUID) ?? throw new NotFoundTargetPlatform(at.UUID)).ID,
                                                }
                                            }));
                                break;

                            case "AtAll":
                                mainContent.Add(JObject.FromObject(new
                                {
                                    type = "at",
                                    data = new
                                    {
                                        qq = "all",
                                    }
                                }));
                                break;

                            case "Voice":
                                if (messageBlock.Value is null || (byte[])messageBlock.Value == Array.Empty<byte>())
                                    break;
                                content = [];
                                content.Add(JObject.FromObject(new
                                {
                                    type = "record",
                                    data = new
                                    {
                                        file = "base64://" + Convert.ToBase64String((byte[])messageBlock.Value),
                                    }
                                }));
                                break;

                            case "Picture":
                                if (messageBlock.Value is null || (byte[])messageBlock.Value == Array.Empty<byte>())
                                    break;
                                content = [];
                                content.Add(JObject.FromObject(new
                                {
                                    type = "image",
                                    data = new
                                    {
                                        file = "base64://" + Convert.ToBase64String((byte[])messageBlock.Value),
                                    }
                                }));
                                break;

                            case "Vidio":
                                if (messageBlock.Value is null || (byte[])messageBlock.Value == Array.Empty<byte>())
                                    break;
                                content = [];
                                content.Add(JObject.FromObject(new
                                {
                                    type = "video",
                                    data = new
                                    {
                                        file = "base64://" + Convert.ToBase64String((byte[])messageBlock.Value),
                                    }
                                }));
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

            if (mainContent.Count > 0)
                contents.Insert(0, mainContent);

            foreach (JArray content in contents)
            {
                JObject box = [];
                if (target is User)
                {
                    box.Add("message_type", "private");
                    box.Add("user_id", target.ID);
                }
                else
                {
                    box.Add("message_type", "group");
                    box.Add("group_id", target.ID);
                }
                box.Add("message", content);

                if (await Send(box, "send_msg", true) is null)
                    return false;
            }

            return true;
        }
    }
}