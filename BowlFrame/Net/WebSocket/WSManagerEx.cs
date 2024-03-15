namespace BowlFrame.Net.WebSocket
{
    internal class WSManagerEx : WSManager
    {
        public bool StartAllClient()
        {
            foreach (KeyValuePair<string, WSClient> keyValuePair in websocketDictionary)
            {
                if (keyValuePair.Value.IsConnected)
                    continue;
                if (!StartClient(keyValuePair.Key))
                    return false;
                //等待一会
                Task.Delay(500).Wait();
            }
            return true;
        }

        public void StopAllClient()
        {
            foreach (KeyValuePair<string, WSClient> keyValuePair in websocketDictionary)
            {
                if (!keyValuePair.Value.IsConnected)
                    continue;
                StopClient(keyValuePair.Key);
            }
        }

        public void DisposeAllClient()
        {
            foreach (KeyValuePair<string, WSClient> keyValuePair in websocketDictionary)
                _ = DisposeClient(keyValuePair.Key);
        }

        public List<string> GetAllConnectID()
        {
            return [.. websocketDictionary.Keys];
        }
    }
}