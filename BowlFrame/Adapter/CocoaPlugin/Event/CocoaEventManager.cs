using System.Collections.Concurrent;
using System.Reflection;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter.CocoaPlugin.Event
{
    public class CocoaEventManager
    {
        //事件列表
        private readonly ConcurrentDictionary<string, CocoaEvent> _eventList = [];

        //注册事件列表
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, ConcurrentDictionary<int, string>>> _registerList = [];

        public bool AddEvent(string eventName, Type @delegate)
        {
            if (_eventList.ContainsKey(eventName))
                return false;

            CocoaEvent cocoaEvent = new(@delegate);

            if (_eventList.TryAdd(eventName, cocoaEvent) && _registerList.TryAdd(eventName, []))
                return true;

            _eventList.TryRemove(eventName, out _);
            _registerList.TryRemove(eventName, out _);

            return false;
        }

        public bool RemoveEvent(string eventName)
        {
            if (!_eventList.ContainsKey(eventName))
                return false;

            _eventList.TryRemove(eventName, out CocoaEvent? cocoaEvent);
            _registerList.TryRemove(eventName, out _);

            cocoaEvent?.ClearEvent();

            return true;
        }

        public bool RegisterEvent(string eventName, object @object, MethodInfo methodInfo)
        {
            if (!_eventList.TryGetValue(eventName, out CocoaEvent? cocoaEvent) || !_registerList.TryGetValue(eventName, out ConcurrentDictionary<int, ConcurrentDictionary<int, string>>? dict))
                return false;

            if (cocoaEvent is null || dict is null)
                return false;

            //先取出插件的注册列表
            dict.TryGetValue(@object.GetHashCode(), out ConcurrentDictionary<int, string>? objectDict);

            if (objectDict is null)
            {
                objectDict = [];
                if (!dict.TryAdd(@object.GetHashCode(), objectDict))
                    return false;
            }

            if (dict.ContainsKey(methodInfo.GetHashCode()))
                return false;

            string? flag = cocoaEvent.RegisterEvent(@object, methodInfo);

            if (flag is null || !objectDict.TryAdd(methodInfo.GetHashCode(), flag))
            {
                Log.Debug("注册事件 {0}({1}.{2}) 失败", eventName, @object.GetType().FullName, methodInfo.Name);
                return false;
            }

            Log.Debug("注册事件 {0}({1}.{2}) 成功", eventName, @object.GetType().FullName, methodInfo.Name);
            return true;
        }

        public bool UnregisterEvent(string eventName, object @object, MethodInfo methodInfo)
        {
            if (!_eventList.TryGetValue(eventName, out CocoaEvent? cocoaEvent) || !_registerList.TryGetValue(eventName, out ConcurrentDictionary<int, ConcurrentDictionary<int, string>>? dict))
                return false;

            if (cocoaEvent is null || dict is null)
                return false;

            //先取出插件的注册列表
            dict.TryGetValue(@object.GetHashCode(), out ConcurrentDictionary<int, string>? objectDict);

            if (objectDict is null || !dict.ContainsKey(methodInfo.GetHashCode()))
                return false;

            objectDict.TryRemove(methodInfo.GetHashCode(), out string? flag);

            if (flag is null)
                return false;

            return cocoaEvent.UnregisterEvent(flag);
        }

        public bool Invoke(string eventName, object?[]? args, out object?[]? returns)
        {
            returns = null;

            if (!_eventList.TryGetValue(eventName, out CocoaEvent? cocoaEvent))
                return false;

            return cocoaEvent.Invoke(args, out returns);
        }
    }
}