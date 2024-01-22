using BowlFrame.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter
{
    internal class AdapterBase : IAdapter
    {
        private static readonly AdapterInfo _adapterInfo = new()
        {
            Name = "Test",
            ID = "0",
            Platform = "Test",
            Description = "测试",
        };

        public AdapterInfo AdapterInfo { get => _adapterInfo; }

        private short status = 0;

        public bool IsConnected { get => status >= 2; }

        protected string? connectID;

        string IAdapter.ConnectID { get => connectID ?? "Null"; set => connectID = value; }

        public event IAdapter.ConnectedEventHandler? ConnectedEvent;

        public event IAdapter.DisconnectEventHandler? DisconnectEvent;

        public event IAdapter.ErrorEventHandler? ErrorEvent;

        public AdapterBase()
        {
            //Logger.Log.Debug($"创建了 {_adapterInfo.Name} 适配器");
            status = 1;
        }

        public virtual void Dispose()
        {
            Logger.Log.Debug($"释放了 {_adapterInfo.Name}({connectID}) 适配器");
            status = 0;
        }

        public virtual ValueTask<bool> Restart()
        {
            status = 1;
            Logger.Log.Debug($"重启了 {_adapterInfo.Name}({connectID}) 适配器");
            OnDisconnectEvent(connectID ?? "Null", _adapterInfo);
            OnConnectedEvent(connectID ?? "Null", _adapterInfo);
            status = 2;
            return new ValueTask<bool>(true);
        }

        public virtual ValueTask<bool> Start()
        {
            status = 2;
            Logger.Log.Debug($"启动了 {_adapterInfo.Name}({connectID}) 适配器");
            OnConnectedEvent(connectID ?? "Null", _adapterInfo);
            return new ValueTask<bool>(true);
        }

        public virtual ValueTask<bool> Stop()
        {
            status = 1;
            Logger.Log.Debug($"停止了 {_adapterInfo.Name}({connectID}) 适配器");
            OnDisconnectEvent(connectID ?? "Null", _adapterInfo);
            return new ValueTask<bool>(true);
        }

        protected virtual void OnConnectedEvent(string connectID, AdapterInfo adapterInfo)
        {
            ConnectedEvent?.Invoke(connectID, adapterInfo);
        }

        protected virtual void OnDisconnectEvent(string connectID, AdapterInfo adapterInfo, Exception? exception = null)
        {
            DisconnectEvent?.Invoke(connectID, adapterInfo, exception);
        }

        protected virtual void OnErrorEvent(string connectID, AdapterInfo adapterInfo, Exception exception)
        {
            ErrorEvent?.Invoke(connectID, adapterInfo, exception);
        }
    }
}