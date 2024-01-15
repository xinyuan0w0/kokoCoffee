using BowlFrame.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter
{
    internal class AdapterSample : IAdapter
    {
        private static readonly AdapterInfo _adapterInfo = new AdapterInfo()
        {
            Name = "Test",
            ID = "0",
            Platform = "Test",
            Description = "测试",
        };

        public AdapterInfo AdapterInfo { get => _adapterInfo; }

        private short status = 0;

        public bool IsConnected { get => status >= 2; }

        private string? connectID;

        string IAdapter.ConnectID { get => connectID ?? "Null"; set => connectID = value; }

        public event IAdapter.ConnectedEventHandler? ConnectedEvent;

        public event IAdapter.DisconnectEventHandler? DisconnectEvent;

        public event IAdapter.ErrorEventHandler? ErrorEvent;

        public AdapterSample()
        {
            Logger.Log.Debug($"创建了 {_adapterInfo.Name} 适配器");
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
            DisconnectEvent?.Invoke(connectID ?? "Null", _adapterInfo, null);
            ConnectedEvent?.Invoke(connectID ?? "Null", _adapterInfo);
            status = 2;
            return new ValueTask<bool>(true);
        }

        public virtual ValueTask<bool> Start()
        {
            status = 2;
            Logger.Log.Debug($"启动了 {_adapterInfo.Name}({connectID}) 适配器");
            ConnectedEvent?.Invoke(connectID ?? "Null", _adapterInfo);
            return new ValueTask<bool>(true);
        }

        public virtual ValueTask<bool> Stop()
        {
            status = 1;
            Logger.Log.Debug($"停止了 {_adapterInfo.Name}({connectID}) 适配器");
            DisconnectEvent?.Invoke(connectID ?? "Null", _adapterInfo, null);
            return new ValueTask<bool>(true);
        }
    }
}