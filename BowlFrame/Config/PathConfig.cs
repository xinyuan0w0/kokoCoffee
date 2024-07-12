using System.IO;

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

            List<string> paths = new List<string> { Path, ConfigPath, DataPath, PluginsPath, TempPath };

            foreach (string dir in paths)
            {
                if (Directory.Exists(dir))
                {
                    continue;
                }
                Directory.CreateDirectory(dir);
            }
        }

        public static string Path { get; }
        public static string ConfigPath { get; }
        public static string DataPath { get; }
        public static string PluginsPath { get; }
        public static string TempPath { get; }
    }
}