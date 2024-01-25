using BowlFrame.Adapter;
using BowlFrame.Tools;
using static BowlFrame.Tools.Logger;
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
        protected ClientWebSocket socket = new();
        protected Task? ReceiveTask;

        //属性
        private Uri uri;

        public Uri Uri
        {
            get => uri;
            set
            {
                uri = value;

                if (socket.State <= WebSocketState.Open)
                {
                    //重连
                    ReconnectAsync().Start();
                }
            }
        }

        private string? connectID;
        public string ConnectID { get => connectID ?? "Null"; set => connectID = value; }

        public bool IsConnected { get => socket.State == WebSocketState.Open; }

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

        ~WSClient()
        {
            Dispose();
        }

        public async Task<bool> ConnectAsync()
        {
            short retryCount = 5;
            int retryInterval = 2000;

            //可能有点疑惑,但是直觉上还是返回成功好
            if (socket.State == WebSocketState.Connecting || socket.State == WebSocketState.Open)
                return true;

            while (retryCount > 0)
            {
                if (socket.State != WebSocketState.None)
                {
                    socket?.Dispose();
                    socket = new();
                }

                try
                {
                    await socket.ConnectAsync(uri, CancellationToken.None);
                    break;
                }
                catch (Exception e)
                {
                    retryCount--;
                    Log.Warn(e);
                    await Task.Delay(retryInterval);
                }
            }
            if (retryCount <= 0)
                return false;


            //不行
            //if (ReceiveTask?.IsCompleted != true)
            //    ReceiveTask?.Dispose();

            //启动接收线程
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
                Log.Warn(e);
            }
        }

        public async Task ReconnectAsync()
        {
            if (socket.State == WebSocketState.Open || socket.State == WebSocketState.Connecting)
                await CloseAsync();

            //while (!(socket.State == WebSocketState.Aborted || socket.State == WebSocketState.None || socket.State == WebSocketState.Closed))
            //    await Task.Delay(50);

            Log.Debug(socket.State.ToString());
            await ConnectAsync();
        }

        public async Task SendAsync(string text)
        {
            Log.Debug(text);
            await SendAsync(Encoding.UTF8.GetBytes(text), WebSocketMessageType.Text, true);
        }

        public async Task SendAsync(byte[] bytes)
        {
            Log.Debug(bytes);
            await SendAsync(bytes, WebSocketMessageType.Binary, true);
        }

        public virtual async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType webSocketMessageType, bool endOfMessage)
        {
            if (socket.State != WebSocketState.Open)
                return;

            try
            {
                await socket.SendAsync(buffer, webSocketMessageType, endOfMessage, CancellationToken.None);
            }
            catch (Exception e)
            {
                Log.Warn(e);
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

        public virtual async Task Receive()
        {
            while (true)
            {
                if (socket.State == WebSocketState.Open)
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
            int bytesLength = 0;
            while (true)
            {
                byte[] buffer = new byte[1024];
                WebSocketReceiveResult receiveResult;
                try
                {
                    receiveResult = await socket.ReceiveAsync(buffer, CancellationToken.None);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                    //触发断开连接事件
                    DisconnectEvent?.Invoke(this, WebSocketCloseStatus.ProtocolError);
                    return;
                }

                if (receiveResult.CloseStatus is not null)
                {
                    //触发断开连接事件
                    DisconnectEvent?.Invoke(this, receiveResult.CloseStatus ?? WebSocketCloseStatus.Empty);
                    return;
                }

                bytes.AddRange(buffer.ToList());
                bytesLength += receiveResult.Count;
                if (!receiveResult.EndOfMessage)
                    continue;

                //触发接收事件
                ReceiveEvent?.Invoke(this, bytes.GetRange(0, bytesLength).ToArray(), receiveResult);

                bytes.Clear();
                bytesLength = 0;
            }
        }

        public virtual void Dispose()
        {
            socket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "", CancellationToken.None).Wait();
            ReceiveTask?.Dispose();
            socket.Dispose();
        }
    }
}