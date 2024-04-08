using BowlFrame.Adapter.CocoaPlugin.Exceptions;
using BowlFrame.Config;
using BowlFrame.Event;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Reflection;
using static BowlFrame.Adapter.CocoaPlugin.CocoaEvent;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter.CocoaPlugin
{
    public class Cocoa : AdapterBase
    {
        //插件列表
        private readonly ConcurrentDictionary<string, (CocoaPluginConfig, ICocoaPlugin)> _plugins = new();

        private readonly CocoaEvent cocoaEvent = new();

        public new static readonly AdapterInfo _AdapterInfo = new()
        {
            Name = "Cocoa",
            ID = "cn.kokobot.cocoa",
            Platform = "All",
            Description = "可可插件管理器",
            Method = new()
            {
                Process = true,
            }
        };

        public Cocoa(JObject args)
        {
            //反序列化
            JsonSerializer jsonSerializer = new()
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };

            _account = args.ToObject<CocoaAccount>(jsonSerializer);

            Log.Debug($"创建了 {_AdapterInfo.Name} 适配器");

            _pluginsPath = Path.Combine(PathConfig.PluginsPath, "Cocoa");
        }

        private bool _isStarted;

        private readonly string _pluginsPath;

        public override bool IsStarted => _isStarted;

        private readonly CocoaAccount _account;

        public override string AccountID => _account.Account;

        public void Test(IEvent @event) => cocoaEvent.OnMainEvent(@event);

        public override Task<bool> Restart()
        {
            //作为重载
            throw new NotImplementedException();
        }

        public override async Task<bool> Start()
        {
            //检查路径
            if (!Path.Exists(_pluginsPath))
                //创建路径
                Directory.CreateDirectory(_pluginsPath);

            //枚举文件
            IEnumerable<string> plugins = Directory.EnumerateFiles(_pluginsPath, "*.dll");

            //滞留插件
            List<string> delayPlugin = [];

            foreach (string plugin in plugins)
                try
                {
                    LoadPlugin(plugin);
                }
                catch (NotFoundDependPlugin)
                {
                    delayPlugin.Add(plugin);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                }

            bool newPlugin = false;
            do
            {
                //再次滞留插件
                List<string> delayPlugin_2 = [];

                foreach (string plugin in delayPlugin)
                    try
                    {
                        LoadPlugin(plugin);
                        newPlugin = true;
                    }
                    catch (NotFoundDependPlugin e)
                    {
                        if (newPlugin)
                            delayPlugin_2.Add(plugin);
                        else
                            Log.Warn(e);
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e);
                    }

                delayPlugin = delayPlugin_2;
            } while (delayPlugin.Count != 0);

            Log.Info("载入了 {0} 个插件", _plugins.Count);

            return true;
        }

        public override Task<bool> Stop()
        {
            throw new NotImplementedException();
        }

        public void LoadPlugin(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("未找到文件", filePath);
            if (!File.Exists(filePath + ".json"))
                throw new FileNotFoundException("未找到文件配置文件", filePath + ".json");

            //加载配置
            CocoaPluginConfig config =
                JsonConvert.DeserializeObject<CocoaPluginConfig>(File.ReadAllText(filePath + ".json")
                , new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error });

            //是否已经载入依赖插件
            if (config.Depend is not null)
                _ = config.Depend.Any(x => _plugins.TryGetValue(x, out _) ? false : throw new NotFoundDependPlugin(config.ID, x));

            //是否已经载入
            if (_plugins.TryGetValue(config.ID, out _))
                throw new PluginAlreadyExits(config);

            Assembly assembly; ICocoaPlugin? plugin;
            try
            {   //加载插件
                assembly = Assembly.LoadFrom(filePath);
                //判断加载的插件是否支持
                Type? targetType = assembly.GetType(config.Main);

                if (targetType is null || !targetType.GetInterfaces().Any(x => x == typeof(ICocoaPlugin)))
                    throw new FileNotPlugin(filePath);

                plugin = assembly.CreateInstance(targetType.FullName ?? throw new NullReferenceException()) as ICocoaPlugin;
            }
            catch (Exception e)
            {
                Log.Warn(e);
                throw;
            }

            LoadPlugin(config, plugin ?? throw new NullReferenceException());
        }

        public void LoadPlugin(CocoaPluginConfig config, ICocoaPlugin plugin)
        {
            //是否已经载入
            if (_plugins.TryGetValue(config.ID, out _))
                throw new PluginAlreadyExits(config);

            try
            {
                foreach (MethodInfo methodInfo in plugin.GetType().GetMethods())
                    foreach (Attribute attribute in methodInfo.GetCustomAttributes(true).Cast<Attribute>())
                        if (attribute is CocoaEventAttribute EventAttribute)
                            RegisterEventWithAttribute(EventAttribute, plugin, methodInfo);

                plugin.Init();
            }
            catch (Exception e)
            {
                Log.Warn(e);
                throw new InitPluginError(e);
            }

            if (!_plugins.TryAdd(config.ID, (config, plugin)))
                throw new Exception("添加插件至列表失败，可能是同时载入多个插件");
        }

        private void RegisterEventWithAttribute(CocoaEventAttribute attribute, ICocoaPlugin plugin, MethodInfo methodInfo)
        {
            RegisterEventHandler(typeof(CocoaEventHandler), methodInfo, plugin, attribute.MainEvent, handler => cocoaEvent.MainEvent += handler as CocoaEventHandler);
            RegisterEventHandler(typeof(CocoaEventHandler), methodInfo, plugin, attribute.MetaEvent, handler => cocoaEvent.MetaEvent += handler as CocoaEventHandler);
            RegisterEventHandler(typeof(CocoaBroadcastHandler), methodInfo, plugin, attribute.Broadcast, handler => cocoaEvent.Broadcast += handler as CocoaBroadcastHandler);
            RegisterEventHandler(typeof(CocoaEventHandler), methodInfo, plugin, attribute.MessageEvent, handler => cocoaEvent.MessageEvent += handler as CocoaEventHandler);
            RegisterEventHandler(typeof(CocoaEventHandler), methodInfo, plugin, attribute.GroupMessage, handler => cocoaEvent.GroupMessage += handler as CocoaEventHandler);
            RegisterEventHandler(typeof(CocoaEventHandler), methodInfo, plugin, attribute.PrivateMessage, handler => cocoaEvent.PrivateMessage += handler as CocoaEventHandler);
            RegisterEventHandler(typeof(CocoaEventHandler), methodInfo, plugin, attribute.GuildMessage, handler => cocoaEvent.GuildMessage += handler as CocoaEventHandler);
            RegisterEventHandler(typeof(CocoaEventHandler), methodInfo, plugin, attribute.ChannelMessage, handler => cocoaEvent.ChannelMessage += handler as CocoaEventHandler);
            RegisterEventHandler(typeof(CocoaEventHandler), methodInfo, plugin, attribute.GuildPrivateMessage, handler => cocoaEvent.GuildPrivateMessage += handler as CocoaEventHandler);
            RegisterEventHandler(typeof(CocoaEventHandler), methodInfo, plugin, attribute.PostMessage, handler => cocoaEvent.PostMessage += handler as CocoaEventHandler);
        }

        private static void RegisterEventHandler(Type eventType, MethodInfo methodInfo, ICocoaPlugin plugin, bool condition, Action<Delegate> registerAction)
        {
            if (condition)
            {
                var handler = methodInfo.CreateDelegate(eventType, plugin);
                registerAction(handler);
            }
        }
    }
}