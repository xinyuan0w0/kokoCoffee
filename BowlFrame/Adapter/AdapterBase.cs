namespace BowlFrame.Adapter
{
    public abstract class AdapterBase : IAdapter
    {
        public abstract AdapterInfo AdapterInfo { get; }

        public abstract bool IsStarted { get; }

        protected string? connectID;

        string IAdapter.ConnectID { get => connectID ?? "Null"; set => connectID = value; }

        public abstract string AccountID { get; }

        public event IAdapter.ConnectedEventHandler? ConnectedEvent;

        public event IAdapter.DisconnectEventHandler? DisconnectEvent;

        public event IAdapter.ErrorEventHandler? ErrorEvent;

        public virtual void Dispose() => GC.SuppressFinalize(this);

        public abstract Task<bool> Restart();

        public abstract Task<bool> Start();

        public abstract Task<bool> Stop();

        protected virtual void OnConnected(string connectID, AdapterInfo adapterInfo) => ConnectedEvent?.Invoke(connectID, adapterInfo);

        protected virtual void OnDisconnect(string connectID, AdapterInfo adapterInfo, Exception? exception = null) => DisconnectEvent?.Invoke(connectID, adapterInfo, exception);

        protected virtual void OnError(string connectID, AdapterInfo adapterInfo, Exception exception) => ErrorEvent?.Invoke(connectID, adapterInfo, exception);
    }
}