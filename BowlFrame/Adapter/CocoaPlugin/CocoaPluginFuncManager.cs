using Amazon.S3.Model.Internal.MarshallTransformations;
using BowlFrame.Adapter.CocoaPlugin.Exceptions;
using BowlFrame.Target;
using static BowlFrame.Tools.Logger;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BowlFrame.Message;
using BowlFrame.Event.Message;

namespace BowlFrame.Adapter.CocoaPlugin
{
    public class CocoaPluginFuncManager(ConcurrentDictionary<string, (CocoaPluginConfig, ICocoaPlugin)> plugins)
    {
        //功能名
        private readonly ConcurrentDictionary<string, CocoaPluginFunc> _funcList = new();

        private readonly ConcurrentDictionary<string, (CocoaPluginConfig, ICocoaPlugin)> plugins = plugins;

        public CocoaPluginFunc? this[string index]
        {
            get
            {
                try
                {
                    return _funcList[index];
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public bool RegisterFunc(string funcID, CocoaPluginFunc cocoaPluginFunc)
        {
            //是否存在
            if (_funcList.ContainsKey(funcID))
                throw new FuncAlreadyExits(funcID);

            //添加到列表
            if (_funcList.TryAdd(funcID, cocoaPluginFunc))
            {
                Log.Debug("注册功能 {0} 成功", funcID);
                return true;
            }

            Log.Warn("注册功能 {0} 失败，可能是同时注册多个功能", funcID);
            return false;
        }

        public void UnregisterFunc(string funcID)
        {
            //是否存在
            if (!_funcList.ContainsKey(funcID))
                return;

            //移出列表

            if (_funcList.TryRemove(funcID, out _))
            {
                Log.Debug("注销功能 {0} 成功", funcID);
                return;
            }
            Log.Warn("注销功能 {0} 失败同时注销多个功能", funcID);
        }

        public void UnregisterPlugin(string pluginID)
        {
            foreach (CocoaPluginFunc func in _funcList.Values.ToList())
            {
                if (func.PluginID == pluginID)
                    UnregisterFunc(func.PluginID);
            }
        }

        public void MatchMessage(MessageBase message)
        {
            //拼接字符串进行正则匹配
            StringBuilder sb = new();
            if (message.Messages is not null)
                foreach (MessageBlock messageBlock in message.Messages.MessageBlocks)
                {
                    if (messageBlock.MetaType != MetaType.Normal)
                        continue;
                    if (messageBlock.Name == "Text")
                        sb.Append(((string?)messageBlock.Value ?? "Null")
                            .Replace("[", @"\[").Replace("]", @"\]").Replace(@"\", @"\\"));
                    else
                        sb.Append($"[{messageBlock.Name}]");
                }

            string content = sb.ToString();

            foreach (KeyValuePair<string, CocoaPluginFunc> func in _funcList)
            {
                Dictionary<string, string> funcArgs = [];

                if (func.Value.FuncConfig.Platform is not null)
                    if (!func.Value.FuncConfig.Platform.Any(x => x.ID == message.Platform.ID && x.AdapterInfo.ID == message.Platform.AdapterInfo.ID))
                        continue;

                if (func.Value.FuncConfig.SourceType is not null)
                    if (!func.Value.FuncConfig.SourceType.Any(x => x == message.SourceType))
                        continue;

                if (func.Value.FuncConfig.Regex is not null)
                {
                    if (string.IsNullOrEmpty(content))
                        continue;

                    Match match = Regex.Match(content, func.Value.FuncConfig.Regex, func.Value.FuncConfig.RegexOptions ?? RegexOptions.None);

                    if (!match.Success)
                        continue;

                    foreach (var item in match.Groups.Values)
                    {
                        if (item.Value != "")
                            // 添加参数
                            funcArgs.Add(item.Name, item.Value);
                    }
                }

                if (func.Value.FuncConfig.Content is not null)
                {
                    if (message.Messages is null)
                        continue;

                    bool flag = false;

                    foreach (OtherContent item in func.Value.FuncConfig.Content)
                    {
                        int count = message.Messages.MessageBlocks.Count(x => x.Name == item.Name && x.MetaType == item.MetaType);

                        if (count == 0 || (item.HaveMulit.HasValue && item.HaveMulit.Value ^ (count != 1)))
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (flag)
                        continue;
                }

                //广播其他功能插件判断是否拦截

                InvokeFunc(func.Value, message, funcArgs);
            }
        }

        private void InvokeFunc(CocoaPluginFunc func, MessageBase message, Dictionary<string, string> args)
        {
            if (!plugins.TryGetValue(func.PluginID, out (CocoaPluginConfig, ICocoaPlugin) plugin))
                throw new NotFoundPlugin(func.PluginID);

            func.MethodInfo.Invoke(plugin.Item2, [message, args]);
        }
    }

    public readonly struct CocoaPluginFunc(string pluginID, CocoaPluginFuncConfig funcConfig, MethodInfo method)
    {
        public string PluginID { get; } = pluginID;

        public CocoaPluginFuncConfig FuncConfig { get; } = funcConfig;

        internal MethodInfo MethodInfo { get; } = method;
    }

    public struct CocoaPluginFuncConfig
    {
        /// <summary>
        /// 正则文本
        /// </summary>
        public string? Regex { get; set; }

        public RegexOptions? RegexOptions { get; set; }

        /// <summary>
        /// 其他内容要求
        /// </summary>
        public OtherContent[]? Content { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public SourceType[]? SourceType { get; set; }

        /// <summary>
        /// 来源平台
        /// </summary>
        public IPlatform[]? Platform { get; set; }

        /// <summary>
        /// 权限节点
        /// </summary>
        public string? Permission { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string[]? OtherName { get; set; }

        /// <summary>
        /// 其他内容
        /// </summary>
        public JObject? OtherContent { get; set; }
    }

    public struct OtherContent
    {
        public string Name { get; set; }

        public MetaType MetaType { get; set; }

        /// <summary>
        /// 存在多个同样的字段
        /// </summary>
        public bool? HaveMulit { get; set; }
    }
}