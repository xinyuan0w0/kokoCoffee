using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NanoidDotNet;
using static BowlFrame.Tools.Logger;
using BowlFrame.Net.WebSocket;

namespace BowlFrame.Adapter
{
    public static class AdapterManager
    {
        private static readonly ConcurrentDictionary<string, IAdapter> adapterDictionary = new();

        public static string? CreateAdapter(string name)
        {
            Type? adapter = Type.GetType(name);

            //判断是否为空
            if (adapter is null)
                return null;

            return CreateAdapter(adapter);
        }

        public static string? CreateAdapter(Type adapter)
        {
            //判断是否支持接口
            if (!typeof(IAdapter).IsAssignableFrom(adapter))
                return null;

            IAdapter adapter1 = (adapter.Assembly.CreateInstance(adapter.FullName ?? throw new NullReferenceException())
                as IAdapter ?? throw new NullReferenceException());

            return CreateAdapter(adapter1);
        }

        public static string? CreateAdapter(IAdapter adapter)
        {
            adapter.ConnectID = Nanoid.Generate(size: 8);

            //注册事件
            adapter.ConnectedEvent += ConnectedEvent;
            adapter.DisconnectEvent += DisconnectEvent;
            adapter.ErrorEvent += ErrorEvent;

            if (!adapterDictionary.TryAdd(adapter.ConnectID, adapter))
            {
                adapter.Dispose();
                return null;
            }

            return adapter.ConnectID;
        }

        public static bool DisposeAdapter(string connectID)
        {
            adapterDictionary.TryRemove(connectID, out IAdapter? adapter);
            if (adapter is null)
                return false;

            //注销事件
            adapter.ConnectedEvent -= ConnectedEvent;
            adapter.DisconnectEvent -= DisconnectEvent;
            adapter.ErrorEvent -= ErrorEvent;

            adapter.Dispose();
            return true;
        }

        public static void Dispose()
        {
            foreach (string connectID in adapterDictionary.Keys)
                _ = DisposeAdapter(connectID);
        }

        public static bool StartAdapter(string connectID)
        {
            adapterDictionary.TryGetValue(connectID, out IAdapter? adapter);
            if (adapter is null)
                return false;
            return adapter.Start().Result;
        }

        public static bool StopAdapter(string connectID)
        {
            adapterDictionary.TryGetValue(connectID, out IAdapter? adapter);
            if (adapter is null)
                return false;
            return adapter.Stop().Result;
        }

        private static void ConnectedEvent(string connectID, AdapterInfo adapterInfo)
        {
            Log.Info($"{adapterInfo.Name}({connectID}) 适配器连接至平台 {adapterInfo.Platform} 成功");
        }

        private static void DisconnectEvent(string connectID, AdapterInfo adapterInfo, Exception? exception)
        {
            if (exception is null)
                Log.Info($"{adapterInfo.Name}({connectID}) 适配器断开连接");
            else
                Log.Error(exception, $"{adapterInfo.Name}({connectID}) 适配器异常断开连接");
        }

        private static void ErrorEvent(string connectID, AdapterInfo adapterInfo, Exception exception)
        {
            Log.Error(exception, $"{adapterInfo.Name}({connectID}) 适配器发生异常");
        }
    }
}