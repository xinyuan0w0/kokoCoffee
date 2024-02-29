using System.Net.WebSockets;
using System.Text;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Net.WebSocket
{
    internal class WSClient(Uri uri) : IDisposable
    {
        protected ClientWebSocket? socket;
        protected Task? ReceiveTask;

        //属性
        protected Uri uri = uri;

        public Uri Uri
        {
            get => uri;
            set
            {
                uri = value;

                if (socket?.State <= WebSocketState.Open)
                {
                    //重连
                    ReconnectAsync().Start();
                }
            }
        }

        private string? connectID;
        public string ConnectID { get => connectID ?? "Null"; set => connectID = value; }

        public bool IsConnected { get => socket?.State == WebSocketState.Open; }

        internal bool disposed = false;

        //private short RetryCount { get; set; }

        public delegate void ReceiveHandler(WSClient client, byte[] bytes, WebSocketReceiveResult receiveResult);

        public event ReceiveHandler? ReceiveEvent;

        public delegate void ConnectedHandler(WSClient client);

        public event ConnectedHandler? ConnectedEvent;

        public delegate void DisconnectHandler(WSClient client, WebSocketCloseStatus closeStatus);

        public event DisconnectHandler? DisconnectEvent;

        ~WSClient()
        {
            Dispose();
            ReceiveTask?.Dispose();
        }

        protected virtual void CreateNewSocket()
        {
            socket?.Dispose();
            socket = new();
        }

        public virtual async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            short retryCount = 5;
            int retryInterval = 2000;

            //可能有点疑惑,但是直觉上还是返回成功好
            if (socket?.State == WebSocketState.Connecting || socket?.State == WebSocketState.Open)
                return true;

            while (retryCount > 0)
            {
                if (socket is null || socket?.State != WebSocketState.None)
                    CreateNewSocket();

                if (socket is null)
                    continue;

                try
                {
                    await socket.ConnectAsync(uri, cancellationToken);
                    break;
                }
                catch (Exception e)
                {
                    retryCount--;
                    Log.Warn(e);
                    await Task.Delay(retryInterval, cancellationToken);
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

        public virtual async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            if (socket is null)
                return;
            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }
        }

        public virtual async Task ReconnectAsync(CancellationToken cancellationToken = default)
        {
            if (socket?.State == WebSocketState.Open || socket?.State == WebSocketState.Connecting)
                await CloseAsync(cancellationToken);

            //while (!(socket.State == WebSocketState.Aborted || socket.State == WebSocketState.None || socket.State == WebSocketState.Closed))
            //    await Task.Delay(50);

            await ConnectAsync(cancellationToken);
        }

        public virtual async Task SendAsync(string text, CancellationToken cancellationToken = default)
        {
            Log.Trace(text);
            await SendAsync(Encoding.UTF8.GetBytes(text), WebSocketMessageType.Text, true, cancellationToken);
        }

        public virtual async Task SendAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            Log.Trace(bytes);
            await SendAsync(bytes, WebSocketMessageType.Binary, true, cancellationToken);
        }

        public virtual async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType webSocketMessageType, bool endOfMessage, CancellationToken cancellationToken = default)
        {
            if (socket?.State != WebSocketState.Open)
                return;

            try
            {
                await socket.SendAsync(buffer, webSocketMessageType, endOfMessage, cancellationToken);
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
                if (socket?.State == WebSocketState.Open)
                {
                    //触发连接事件
                    ConnectedEvent?.Invoke(this);
                    break;
                }
                else if (socket?.State >= WebSocketState.CloseSent)
                {
                    //触发断开连接事件
                    DisconnectEvent?.Invoke(this, socket.CloseStatus ?? WebSocketCloseStatus.Empty);
                    return;
                }
            }

            List<byte> bytes = [];
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

                bytes.AddRange([.. buffer]);
                bytesLength += receiveResult.Count;
                if (!receiveResult.EndOfMessage)
                    continue;

                //触发接收事件
                ReceiveEvent?.Invoke(this, bytes.ToArray()[..bytesLength], receiveResult);

                bytes.Clear();
                bytesLength = 0;
            }
        }

#pragma warning disable CA1816 // Dispose 方法应调用 SuppressFinalize

        public virtual void Dispose()
        {
            if (!disposed)
            {
                socket?.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "", CancellationToken.None).Wait();

                //不能在这回收
                //ReceiveTask?.Dispose();

                socket?.Dispose();

                //没有被完全回收
                //GC.SuppressFinalize(this);
                disposed = true;
            }
        }

#pragma warning restore CA1816 // Dispose 方法应调用 SuppressFinalize
    }
}