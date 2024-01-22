using BowlFrame.Adapter;
using NanoidDotNet;
using System;
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
        private readonly ConcurrentDictionary<string, WSClient> websocketDictionary = new();

        public delegate void ReceiveHandler(WSClient client, byte[] bytes, WebSocketReceiveResult receiveResult);

        public event ReceiveHandler? ReceiveEvent;

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
                client1 = client.GetType().Assembly.CreateInstance(client.GetType().FullName ?? throw new NullReferenceException())
                    as WSClient ?? throw new NullReferenceException();
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
            client.ConnectedEvent += ConnectedEvent;
            client.DisconnectEvent += DisconnectEvent;
            client.ReceiveEvent += Receive;

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
            client.ConnectedEvent -= ConnectedEvent;
            client.DisconnectEvent -= DisconnectEvent;
            client.ReceiveEvent -= Receive;

            client.Dispose();
            return true;
        }

        public void Dispose()
        {
            foreach (string connectID in websocketDictionary.Keys)
                _ = DisposeClient(connectID);
        }

        public bool StartClient(string connectID)
        {
            websocketDictionary.TryGetValue(connectID, out WSClient? client);
            if (client is null)
                return false;
            return client.ConnectAsync().Result;
        }

        public bool StopClient(string connectID)
        {
            websocketDictionary.TryGetValue(connectID, out WSClient? client);
            if (client is null)
                return false;
            try
            {
                client.CloseAsync().Wait();
            }
            catch (Exception e)
            {
                Log.Warn(e);
                return false;
            }

            return true;
        }

        private void ConnectedEvent(WSClient client)
        {
            Log.Info($"WebSocketClient({client.ConnectID}) 已连接至 {client.Uri}");
        }

        private void DisconnectEvent(WSClient client, WebSocketCloseStatus closeStatus)
        {
            Log.Info($"WebSocketClient({client.ConnectID}) 断开连接");
        }

        private void Receive(WSClient client, byte[] bytes, WebSocketReceiveResult receiveResult)
        {
            ReceiveEvent?.Invoke(client, bytes, receiveResult);
        }
    }
}