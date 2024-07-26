namespace BowlFrame.Config
{
    public static class PathConfig
    {
        static PathConfig()
        {
            Path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "kokoBot");
            ConfigPath = System.IO.Path.Combine(Path, "Config");
            DataPath = System.IO.Path.Combine(Path, "Data");
            PluginsPath = System.IO.Path.Combine(Path, "Plugins");
            TempPath = System.IO.Path.Combine(Path, "Temp");

            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
            if (!Directory.Exists(ConfigPath))
                Directory.CreateDirectory(ConfigPath);
            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);
            if (!Directory.Exists(PluginsPath))
                Directory.CreateDirectory(PluginsPath);
            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);
        }

        public static string Path { get; }
        public static string ConfigPath { get; }
        public static string DataPath { get; }
        public static string PluginsPath { get; }
        public static string TempPath { get; }
    }
}