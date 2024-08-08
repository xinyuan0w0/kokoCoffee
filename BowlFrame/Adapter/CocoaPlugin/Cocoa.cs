using BowlFrame.Adapter.CocoaPlugin.Attributes;
using BowlFrame.Adapter.CocoaPlugin.Event;
using BowlFrame.Adapter.CocoaPlugin.Event.Manager;
using BowlFrame.Adapter.CocoaPlugin.Exceptions;
using BowlFrame.Config;
using BowlFrame.Event;
using BowlFrame.Event.Message;
using BowlFrame.Message;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Reflection;
using static BowlFrame.Adapter.CocoaPlugin.CocoaGlobalEvent;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter.CocoaPlugin
{
    public class Cocoa : AdapterBase
    {
        //插件列表
        private readonly ConcurrentDictionary<string, (CocoaPluginConfig, ICocoaPlugin)> _plugins = new();

        private ConcurrentDictionary<string, (bool, bool?)> _pluginsEnableList = new();

        private readonly CocoaPluginFuncManager pluginFuncManager;

        private readonly CocoaGlobalEvent cocoaEvent = new();

        public readonly CocoaEventManager EventManager = new();

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

            _pluginsPath = Path.Combine(PathConfig.PluginsPath, _AdapterInfo.ID);

            _configPath = Path.Combine(PathConfig.ConfigPath, _AdapterInfo.ID);

            if (!Directory.Exists(Path.Combine(_configPath, "Messages")))
                Directory.CreateDirectory(Path.Combine(_configPath, "Messages"));

            Platform = new CocoaPlatform(_account.Account);

            pluginFuncManager = new(this, _plugins);

            CheckMatchMessage.BindEventManager(EventManager);
        }

        ~Cocoa()
        {
            Dispose();
        }

        private readonly string _pluginsPath;

        private readonly string _configPath;

        private bool _isStarted;
        public override bool IsStarted => _isStarted;

        internal IPlatform Platform { get; }

        private readonly CocoaAccount _account;

        public override string AccountID => _account.Account;

        public override Task<bool> Restart()
        {
            //作为重载
            throw new NotImplementedException();
        }

        public override Task<bool> Start()
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

            AdapterManager.BroadcastEvent += ReciveEvent;

            _isStarted = true;

            //启用插件
            if (File.Exists(Path.Combine(_configPath, "EnableList.json")))
                _pluginsEnableList = JsonConvert.DeserializeObject<ConcurrentDictionary<string, (bool, bool?)>>(File.ReadAllText(Path.Combine(_configPath, "EnableList.json"))) ?? [];

            foreach (string pluginID in _plugins.Keys)
            {
                if (_pluginsEnableList.TryGetValue(pluginID, out (bool, bool?) value) && value.Item2 == false)
                {
                    _pluginsEnableList.TryUpdate(pluginID, (false, value.Item2), value);
                }
                else
                {
                    try
                    {
                        if (EnablePlugin(pluginID) == true)
                            _pluginsEnableList.AddOrUpdate(pluginID, (a) => (true, null), (a, b) => (true, b.Item2));
                        else
                            _pluginsEnableList.AddOrUpdate(pluginID, (a) => (false, null), (a, b) => (false, b.Item2));
                    }
                    catch (Exception)
                    {
                        _pluginsEnableList.AddOrUpdate(pluginID, (a) => (false, null), (a, b) => (false, b.Item2));
                    }
                }
            }

            return Task.FromResult(true);
        }

        public override Task<bool> Stop()
        {
            if (!_isStarted)
                return Task.FromResult(false);

            AdapterManager.BroadcastEvent -= ReciveEvent;

            foreach (string key in _plugins.Keys)
            {
                try
                {
                    UnregisterPlugin(key);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                }
            }

            _plugins.Clear();

            _isStarted = false;

            return Task.FromResult(true);
        }

        public override void Dispose()
        {
            foreach (string key in _plugins.Keys)
            {
                try
                {
                    UnregisterPlugin(key);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                }
            }

            _plugins.Clear();

            base.Dispose();
            GC.SuppressFinalize(this);
        }

        private void ReciveEvent(IEvent @event, AdapterInfo adapterInfo)
        {
            cocoaEvent.OnMainEvent(@event);

            switch (@event.EventType)
            {
                case EventType.System:
                    cocoaEvent.OnMetaEvent(@event);
                    break;

                case EventType.Event:
                    cocoaEvent.OnEvent(@event);
                    break;

                case EventType.Message:
                    cocoaEvent.OnMessageEvent(@event as MessageBase ?? throw new NullReferenceException());
                    switch (@event as MessageBase)
                    {
                        case GroupMessage groupMessage:
                            cocoaEvent.OnGroupMessage(groupMessage);
                            break;

                        case PrivateMessage privateMessage:
                            cocoaEvent.OnPrivateMessage(privateMessage);
                            break;

                        case ChannelMessage channelMessage:
                            cocoaEvent.OnChannelMessage(channelMessage);
                            break;

                        default:
                            break;
                    }
                    pluginFuncManager.MatchMessage(@event as MessageBase ?? throw new NullReferenceException());
                    break;

                default:
                    break;
            }
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
                CocoaPluginPath cocoaPluginpath = new()
                {
                    ConfigPath = Path.Combine(PathConfig.ConfigPath, config.ID),
                    DataPath = Path.Combine(PathConfig.DataPath, config.ID),
                    TempPath = Path.Combine(PathConfig.TempPath, config.ID)
                };

                plugin.Init(cocoaPluginpath);

                //遍历程序集方法
                foreach (MethodInfo methodInfo in plugin.GetType().Assembly.GetTypes().SelectMany(x => x.GetMethods()))
                    foreach (Attribute attribute in methodInfo.GetCustomAttributes(true).Cast<Attribute>())
                        if (attribute is CocoaEventAttribute eventAttribute)
                            RegisterEventWithAttribute(eventAttribute, plugin, methodInfo);
                        else if (attribute is CocoaFuncAttribute funcAttribute)
                        {
                            CocoaPluginFuncConfig cocoaPluginFuncConfig = JsonConvert.DeserializeObject<CocoaPluginFuncConfig>(File.ReadAllText(Path.Combine(PathConfig.ConfigPath, config.ID, funcAttribute.FuncName, "config.json")));
                            CocoaPluginFunc cocoaPluginFunc = new(config.ID, cocoaPluginFuncConfig, methodInfo);
                            pluginFuncManager.RegisterFunc(funcAttribute.FuncName, cocoaPluginFunc);
                        }
                        else if (attribute is CocoaBindEventAttribute bindEventAttribute)
                            EventManager.RegisterEvent(bindEventAttribute.Event, plugin, methodInfo);
            }
            catch (Exception e)
            {
                Log.Warn(e);
                throw new InitPluginError(e);
            }

            if (!_plugins.TryAdd(config.ID, (config, plugin)))
                throw new Exception("添加插件至列表失败，可能是同时载入插件");
        }

        public bool? EnablePlugin(string pluginID)
        {
            if (!_plugins.ContainsKey(pluginID))
                return null;

            if (_pluginsEnableList.TryGetValue(pluginID, out (bool, bool?) value) && value.Item1)
                return false;

            ICocoaPlugin cocoaPlugin = _plugins[pluginID].Item2;

            try
            {
                if (!cocoaPlugin.Enable())
                {
                    _pluginsEnableList.AddOrUpdate(pluginID, (a) => (false, null), (a, b) => (false, b.Item2));
                    return false;
                }

                _pluginsEnableList.AddOrUpdate(pluginID, (a) => (true, null), (a, b) => (true, b.Item2));
            }
            catch (Exception)
            {
                _pluginsEnableList.AddOrUpdate(pluginID, (a) => (false, null), (a, b) => (false, b.Item2));
                return false;
            }

            return true;
        }

        public bool? DisablePlugin(string pluginID)
        {
            if (!_plugins.ContainsKey(pluginID))
                return null;

            if (_pluginsEnableList.TryGetValue(pluginID, out (bool, bool?) value) && !value.Item1)
                return false;

            ICocoaPlugin cocoaPlugin = _plugins[pluginID].Item2;

            try
            {
                cocoaPlugin.Disable();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _pluginsEnableList.AddOrUpdate(pluginID, (a) => (false, null), (a, b) => (false, b.Item2));
            }

            return true;
        }

        public void SetPluginStatus(string pluginID, bool status) => _pluginsEnableList.AddOrUpdate(pluginID, (a) => (true, status), (a, b) => (b.Item1, status));

        private void RegisterEventWithAttribute(CocoaEventAttribute attribute, ICocoaPlugin plugin, MethodInfo methodInfo)
        {
            RegisterEventHandler(typeof(CocoaEventHandler), methodInfo, plugin, attribute.MainEvent, handler => cocoaEvent.MainEvent += handler as CocoaEventHandler);
            RegisterEventHandler(typeof(CocoaEventHandler), methodInfo, plugin, attribute.MetaEvent, handler => cocoaEvent.MetaEvent += handler as CocoaEventHandler);
            RegisterEventHandler(typeof(CocoaBroadcastHandler), methodInfo, plugin, attribute.Broadcast, handler => cocoaEvent.Broadcast += handler as CocoaBroadcastHandler);
            RegisterEventHandler(typeof(CocoaMessageEventHandler), methodInfo, plugin, attribute.MessageEvent, handler => cocoaEvent.MessageEvent += handler as CocoaMessageEventHandler);
            RegisterEventHandler(typeof(CocoaMessageEventHandler), methodInfo, plugin, attribute.GroupMessage, handler => cocoaEvent.GroupMessage += handler as CocoaMessageEventHandler);
            RegisterEventHandler(typeof(CocoaMessageEventHandler), methodInfo, plugin, attribute.PrivateMessage, handler => cocoaEvent.PrivateMessage += handler as CocoaMessageEventHandler);
            RegisterEventHandler(typeof(CocoaMessageEventHandler), methodInfo, plugin, attribute.ChannelMessage, handler => cocoaEvent.ChannelMessage += handler as CocoaMessageEventHandler);
            RegisterEventHandler(typeof(CocoaMessageEventHandler), methodInfo, plugin, attribute.PostMessage, handler => cocoaEvent.PostMessage += handler as CocoaMessageEventHandler);
        }

        private static void RegisterEventHandler(Type eventType, MethodInfo methodInfo, ICocoaPlugin plugin, bool condition, Action<Delegate> registerAction)
        {
            if (condition)
            {
                var handler = methodInfo.CreateDelegate(eventType, plugin);
                registerAction(handler);
            }
        }

        public void UnregisterPlugin(string pluginID)
        {
            if (!_plugins.TryGetValue(pluginID, out (CocoaPluginConfig, ICocoaPlugin) plugin))
                throw new NotFoundPlugin(pluginID);

            BroadcastEvent broadcastEvent = new(Platform);

            broadcastEvent.Messages?.Add(new MessageBlock()
            {
                HaveMulit = false,
                MetaType = MetaType.Normal,
                Name = "DisablePlugin",
            });

            cocoaEvent.OnBroadcast(broadcastEvent, plugin.Item2);

            plugin.Item2.Dispose();

            UnregisterEvent(plugin.Item2);

            if (_plugins.TryRemove(pluginID, out _))
                throw new Exception("移除插件出列表失败，可能是同时卸载插件");
        }

        private static void UnregisterEvent(ICocoaPlugin cocoaPlugin)
        {
            Type type = cocoaPlugin.GetType();

            foreach (EventInfo eventInfo in type.GetEvents())
            {
                FieldInfo? field = type.GetField(eventInfo.Name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
                if (field is null)
                    continue;

                Delegate? del = field.GetValue(cocoaPlugin) as Delegate;
                if (del is not null)
                {
                    foreach (Delegate subDel in del.GetInvocationList())
                    {
                        eventInfo.RemoveEventHandler(cocoaPlugin, subDel);
                    }
                }
            }
        }
    }
}