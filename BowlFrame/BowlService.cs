using BowlFrame.Adapter;
using BowlFrame.Config;
using BowlFrame.Database;
using BowlFrame.Database.TableStruct;
using BowlFrame.Perm;
using NanoidDotNet;
using Newtonsoft.Json.Linq;
using NLog;
using System.Threading.Tasks;
using static BowlFrame.Tools.Logger;

namespace BowlFrame
{
    public class BowlService
    {
        public static async Task Initialization()
        {
            Console.WriteLine(PathConfig.Path);
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(PathConfig.ConfigPath, "nlog.config"));

            Console.WriteLine(" ___  __    ________  ___  __    ________  ________  ________  _________   \r\n|\\  \\|\\  \\ |\\   __  \\|\\  \\|\\  \\ |\\   __  \\|\\   __  \\|\\   __  \\|\\___   ___\\ \r\n\\ \\  \\/  /|\\ \\  \\|\\  \\ \\  \\/  /|\\ \\  \\|\\  \\ \\  \\|\\ /\\ \\  \\|\\  \\|___ \\  \\_| \r\n \\ \\   ___  \\ \\  \\\\\\  \\ \\   ___  \\ \\  \\\\\\  \\ \\   __  \\ \\  \\\\\\  \\   \\ \\  \\  \r\n  \\ \\  \\\\ \\  \\ \\  \\\\\\  \\ \\  \\\\ \\  \\ \\  \\\\\\  \\ \\  \\|\\  \\ \\  \\\\\\  \\   \\ \\  \\ \r\n   \\ \\__\\\\ \\__\\ \\_______\\ \\__\\\\ \\__\\ \\_______\\ \\_______\\ \\_______\\   \\ \\__\\\r\n    \\|__| \\|__|\\|_______|\\|__| \\|__|\\|_______|\\|_______|\\|_______|    \\|__|\r\n                                                                           \r\n                                                                           \r\n                                                                           ");
            Log.Info("Hello, I'm kokoBot!");

            await ConfigLoad.AddFileFromPath("Target", Path.Combine(PathConfig.ConfigPath, "Target.json"));
            await ConfigLoad.AddFileFromPath("S3.json", Path.Combine(PathConfig.ConfigPath, "S3.json"));
        }

        public static async Task Start()
        {
            AdapterManagerEx.CreateAdapterFromFile(Path.Combine(PathConfig.ConfigPath, "Adapter.json"));
            await AdapterManagerEx.StartAllAdapter();
        }

        public static void Stop()
        { }

        public static async void Debug()
        {
            
        }
    }
}