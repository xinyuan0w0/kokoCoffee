namespace BowlFrame.Adapter
{
    public abstract class AdapterBase : IAdapter
    {
        private static readonly AdapterInfo _adapterInfo = new()
        {
            Name = "BowlFrame",
            ID = "cn.kokobot",
            Platform = "BowlFrame",
            Description = "内部使用",
        };

        public AdapterInfo AdapterInfo { get => _adapterInfo; }

        public abstract bool IsConnected { get; }

        protected string? connectID;

        string IAdapter.ConnectID { get => connectID ?? "Null"; set => connectID = value; }

        public abstract string AccountID { get; }

        public event IAdapter.ConnectedEventHandler? ConnectedEvent;

        public event IAdapter.DisconnectEventHandler? DisconnectEvent;

        public event IAdapter.ErrorEventHandler? ErrorEvent;

        public virtual void Dispose()
        {
        }

        public abstract ValueTask<bool> Restart();

        public abstract ValueTask<bool> Start();

        public abstract ValueTask<bool> Stop();

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