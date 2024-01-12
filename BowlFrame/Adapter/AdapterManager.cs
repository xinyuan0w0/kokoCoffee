using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NanoidDotNet;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter
{
    public static class AdapterManager
    {
        private static readonly ConcurrentDictionary<string, IAdapter> adapterDictionary = new();

        public static bool CreateAdapter(string name)
        {
            Type? adapter = Type.GetType(name);

            //判断是否为空
            if (adapter is null)
                return false;

            return CreateAdapter(adapter);
        }

        public static bool CreateAdapter(Type adapter)
        {
            //判断是否支持接口
            if (!typeof(IAdapter).IsAssignableFrom(adapter))
                return false;

            IAdapter adapter1 = (IAdapter)(adapter.Assembly.CreateInstance(adapter.Name) ?? throw new NullReferenceException());

            adapter1.ConnectID = Nanoid.Generate();

            adapter1.ConnectedEvent += ConnectedEvent;
            adapter1.DisconnectEvent += DisconnectEvent;
            adapter1.ErrorEvent += ErrorEvent;

            ValueTask<bool> task = adapter1.Start();

            if (!adapterDictionary.TryAdd(adapter1.ConnectID, adapter1))
                return false;

            return true;
        }

        private static void ConnectedEvent(string connectID, AdapterInfo adapterInfo)
        {
            Log.Info($"{adapterInfo.Name}({adapterInfo.ID}) 适配器连接至平台 {adapterInfo.Platform} 成功");
        }

        private static void DisconnectEvent(string connectID, AdapterInfo adapterInfo, Exception? exception)
        {
            if (exception is null)
            {
                Log.Info($"{adapterInfo.Name}({adapterInfo.ID}) 适配器断开连接");
                adapterDictionary[connectID].Restart();
            }
            else
                Log.Error(exception, $"{adapterInfo.Name}({adapterInfo.ID}) 适配器异常断开连接");
        }

        private static void ErrorEvent(string connectID, AdapterInfo adapterInfo, Exception exception)
        {
            Log.Error(exception, $"{adapterInfo.Name}({adapterInfo.ID}) 适配器发生异常");
        }
    }
}