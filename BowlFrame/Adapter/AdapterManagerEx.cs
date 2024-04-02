using Newtonsoft.Json.Linq;

namespace BowlFrame.Adapter
{
    internal static class AdapterManagerEx
    {
        public static bool CreateAdapterFromFile(string filePath)
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
                if (!await adapter.Start())
                    return false;
            }
            return true;
        }
    }
}