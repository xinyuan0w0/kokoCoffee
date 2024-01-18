using BowlFrame.Adapter;
using BowlFrame.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BowlFrame.Net.WebSocket
{
    internal class WSClient : IDisposable
    {
        protected readonly ClientWebSocket socket = new();
        protected Task? ReceiveTask;

        //属性
        private Uri uri;

        public Uri Uri
        {
            get => uri;
            set
            {
                uri = value;
                //重连(还没写)
                if (socket.State <= WebSocketState.Open)
                {
                    //重连
                    ReconnectAsync().Start();
                }
            }
        }

        private string? connectID;
        public string ConnectID { get => connectID ?? "Null"; set => connectID = value; }

        //private short RetryCount { get; set; }

        public delegate void ReceiveHandler(WSClient client, byte[] bytes, WebSocketReceiveResult receiveResult);

        public event ReceiveHandler? ReceiveEvent;

        public delegate void ConnectedHandler(WSClient client);

        public event ConnectedHandler? ConnectedEvent;

        public delegate void DisconnectHandler(WSClient client, WebSocketCloseStatus closeStatus);

        public event DisconnectHandler? DisconnectEvent;

        public WSClient(Uri uri)
        {
            this.uri = uri;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                await socket.ConnectAsync(uri, CancellationToken.None);
            }
            catch (Exception e)
            {
                Logger.Log.Warn(e);
                return false;
            }

            //启动接收线程
            if (!ReceiveTask?.IsCompleted == true)
                ReceiveTask?.Dispose();
            ReceiveTask = Receive();
            return true;
        }

        public async Task CloseAsync()
        {
            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            catch (Exception e)
            {
                Logger.Log.Warn(e);
            }
        }

        public async Task ReconnectAsync()
        {
            if (socket.State <= WebSocketState.Open)
                await CloseAsync();
            await ConnectAsync();
        }

        public async Task SendAsync(string text)
        {
            await SendAsync(Encoding.UTF8.GetBytes(text), WebSocketMessageType.Binary, true);
        }

        public async Task SendAsync(byte[] bytes)
        {
            await SendAsync(bytes, WebSocketMessageType.Binary, true);
        }

        public async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType webSocketMessageType, bool endOfMessage)
        {
            try
            {
                await socket.SendAsync(buffer, webSocketMessageType, endOfMessage, CancellationToken.None);
            }
            catch (Exception e)
            {
                Logger.Log.Warn(e);
            }

            //没这个必要

            //    short count = 0;

            //Retry:
            //    try
            //    {
            //        await socket.SendAsync(buffer, webSocketMessageType, endOfMessage, CancellationToken.None);
            //    }
            //    catch (Exception e)
            //    {
            //        count++;
            //        Logger.Log.Warn(e, $"当前已重试: {e} 次");
            //        if (count >= RetryCount)
            //            throw;
            //        goto Retry;
            //    }
        }

        public async Task Receive()
        {
            while (true)
            {
                if (socket.State <= WebSocketState.Open)
                {
                    //触发连接事件
                    ConnectedEvent?.Invoke(this);
                    break;
                }
                else if (socket.State >= WebSocketState.CloseSent)
                {
                    //触发断开连接事件
                    DisconnectEvent?.Invoke(this, socket.CloseStatus ?? WebSocketCloseStatus.Empty);
                    return;
                }
            }

            List<byte> bytes = new();
            while (true)
            {
                byte[] buffer = new byte[1024];
                WebSocketReceiveResult receiveResult = await socket.ReceiveAsync(buffer, CancellationToken.None);

                if (receiveResult.CloseStatus is not null)
                {
                    //触发断开连接事件
                    DisconnectEvent?.Invoke(this, receiveResult.CloseStatus ?? WebSocketCloseStatus.Empty);
                    return;
                }

                bytes.AddRange(buffer.ToList());
                if (!receiveResult.EndOfMessage)
                    continue;

                //触发接收事件
                ReceiveEvent?.Invoke(this, bytes.ToArray(), receiveResult);

                bytes.Clear();
            }
        }

        public void Dispose()
        {
            socket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "", CancellationToken.None).Wait();
            ReceiveTask?.Dispose();
            socket.Dispose();
        }
    }
}