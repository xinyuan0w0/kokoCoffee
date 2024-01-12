using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter
{
    public interface IAdapter
    {
        AdapterInfo AdapterInfo { get; }
        bool IsConnected { get; }
        string ConnectID { internal set; get; }

        public ValueTask<bool> Start();

        public ValueTask<bool> Stop();

        public ValueTask<bool> Restart(); //QQ频道具有重连功能的此方法不等同于先Stop再Start

        public delegate void ConnectedEventHandler(string connectID, AdapterInfo adapterInfo);

        public event ConnectedEventHandler ConnectedEvent;

        public delegate void DisconnectEventHandler(string connectID, AdapterInfo adapterInfo, Exception? exception);

        public event DisconnectEventHandler DisconnectEvent;

        public delegate void ErrorEventHandler(string connectID, AdapterInfo adapterInfo, Exception exception);

        public event ErrorEventHandler ErrorEvent;
    }
}