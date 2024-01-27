using BowlFrame.Adapter;
using NanoidDotNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Net.WebSocket
{
    internal class WSManager : IDisposable
    {
        protected readonly ConcurrentDictionary<string, WSClient> websocketDictionary = new();

        public delegate void ReceiveHandler(WSClient client, byte[] bytes, WebSocketReceiveResult receiveResult);

        public event ReceiveHandler? ReceiveEvent;

        public delegate void ConnectedHandler(WSClient client);

        public event ConnectedHandler? ConnectedEvent;

        public delegate void DisconnectHandler(WSClient client, WebSocketCloseStatus closeStatus);

        public event DisconnectHandler? DisconnectEvent;

        ~WSManager()
        {
            Dispose();
        }

        public void Dispose()
        {
            //释放WSClient
            foreach (string connectID in websocketDictionary.Keys)
                _ = DisposeClient(connectID);

            websocketDictionary.Clear();
            GC.SuppressFinalize(this);
        }

        public WSClient? this[string connectID]
        {
            get
            {
                if (!websocketDictionary.TryGetValue(connectID, out WSClient? client))
                    return null;
                return client;
            }
        }

        public string? CreateClient(string name, params object[]? args)
        {
            //取类型
            Type? client = Type.GetType(name);

            if (client is null)
                return null;

            return CreateClient(client, args);
        }

        public string? CreateClient(Type client, params object[]? args)
        {
            //判断是否为WSClient的衍生类
            if (!client.IsSubclassOf(typeof(WSClient)))
                return null;

            //创建对象
            WSClient client1;
            try
            {
                client1 = (client.Assembly.CreateInstance(client.FullName ?? throw new NullReferenceException(), false, BindingFlags.Default, null, args, null, null)
                    as WSClient ?? throw new NullReferenceException());
            }
            catch (Exception e)
            {
                Log.Warn(e, $"创建 {client.FullName} WSClient时发送错误");
                return null;
            }

            return CreateClient(client1);
        }

        public string? CreateClient(WSClient client)
        {
            //注册事件
            client.ConnectedEvent += ListenConnectedEvent;
            client.DisconnectEvent += ListenDisconnectEvent;
            client.ReceiveEvent += ListenReceiveEvent;

            client.ConnectID = Nanoid.Generate(size: 8);

            if (!websocketDictionary.TryAdd(client.ConnectID, client))
            {
                client.Dispose();
                return null;
            }

            return client.ConnectID;
        }

        public bool DisposeClient(string connectID)
        {
            websocketDictionary.TryRemove(connectID, out WSClient? client);
            if (client is null)
                return false;

            //注销事件
            client.ConnectedEvent -= ListenConnectedEvent;
            client.DisconnectEvent -= ListenDisconnectEvent;
            client.ReceiveEvent -= ListenReceiveEvent;

            client.Dispose();
            return true;
        }

        public bool StartClient(string connectID)
        {
            websocketDictionary.TryGetValue(connectID, out WSClient? client);
            if (client is null)
                return false;
            return client.ConnectAsync().Result;
        }

        public void StopClient(string connectID)
        {
            websocketDictionary.TryGetValue(connectID, out WSClient? client);
            if (client is null)
                return;
            try
            {
                client.CloseAsync().Wait();
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }
        }

        private void ListenConnectedEvent(WSClient client)
        {
            Log.Info($"WebSocketClient({client.ConnectID}) 已连接至 {client.Uri}");
            ConnectedEvent?.Invoke(client);
        }

        private void ListenDisconnectEvent(WSClient client, WebSocketCloseStatus closeStatus)
        {
            Log.Info($"WebSocketClient({client.ConnectID}) 断开连接");
            DisconnectEvent?.Invoke(client, closeStatus);
        }

        private void ListenReceiveEvent(WSClient client, byte[] bytes, WebSocketReceiveResult receiveResult)
        {
            ReceiveEvent?.Invoke(client, bytes, receiveResult);
        }
    }
}