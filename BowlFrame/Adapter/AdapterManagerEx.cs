using Newtonsoft.Json.Linq;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter
{
    public static class AdapterManagerEx
    {
        public static bool CreateAdapterFromConfigFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            JArray config = JArray.Parse(File.ReadAllText(filePath));
            foreach (JObject keyValues in config.Cast<JObject>())
            {
                foreach (JProperty property in keyValues.Properties())
                {
                    if (AdapterManager.CreateAdapter(property.Name, property.Value) is null)
                        return false;
                }
            }
            return true;
        }

        public static async Task<bool> StartAllAdapter()
        {
            foreach (IAdapter adapter in AdapterManager.adapterDictionary.Values)
            {
                if (adapter.IsStarted)
                    continue;
                try
                {
                    if (!await adapter.Start())
                        return false;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    return false;
                }
            }
            return true;
        }
    }
}