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
using static BowlFrame.Message.Struct.MsgBlock;
using static BowlFrame.Adapter.OneBotV11Adapter.Tools.MsgBlockToOnebot;
using BowlFrame.Adapter.OneBotV11Adapter.Tools;

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

        public async Task<bool?> SendMessage(Messages messages)
        {
            List<JArray> contents = [];

            MsgBlockToOnebot msgBlockToOnebot = new(oneBotV11, permission, target);

            foreach (MessageBlock messageBlock in messages.MessageBlocks)
            {
                JArray? content = messageBlock.MetaType switch
                {
                    MetaType.Normal => await msgBlockToOnebot.CreateNormalContent(messageBlock),
                    MetaType.Extra => null, //施工中
                    _ => null
                };

                if (content != null)
                    contents.Add(content);
            }

            return await SendContents(contents);
        }

        private async Task<bool?> SendContents(List<JArray> contents)
        {
            foreach (JArray content in contents)
            {
                JObject box = new()
                {
                    { "message_type", target is User ? "private" : "group" },
                    { target is User ? "user_id" : "group_id", target.ID },
                    { "message", content }
                };

                if (await Send(box, "send_msg", true) is null)
                    return false;
            }
            return true;
        }
    }
}