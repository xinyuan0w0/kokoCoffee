using BowlFrame.Config;
using static BowlFrame.Tools.Logger;
using NLog;
using BowlFrame.Adapter;
using BowlFrame.Net.WebSocket;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;

namespace BowlFrame
{
    public class BowlService
    {
        public static void Initialization()
        {
            Console.WriteLine(PathConfig.Path);
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(PathConfig.ConfigPath, "nlog.config"));

            Console.WriteLine(" ___  __    ________  ___  __    ________  ________  ________  _________   \r\n|\\  \\|\\  \\ |\\   __  \\|\\  \\|\\  \\ |\\   __  \\|\\   __  \\|\\   __  \\|\\___   ___\\ \r\n\\ \\  \\/  /|\\ \\  \\|\\  \\ \\  \\/  /|\\ \\  \\|\\  \\ \\  \\|\\ /\\ \\  \\|\\  \\|___ \\  \\_| \r\n \\ \\   ___  \\ \\  \\\\\\  \\ \\   ___  \\ \\  \\\\\\  \\ \\   __  \\ \\  \\\\\\  \\   \\ \\  \\  \r\n  \\ \\  \\\\ \\  \\ \\  \\\\\\  \\ \\  \\\\ \\  \\ \\  \\\\\\  \\ \\  \\|\\  \\ \\  \\\\\\  \\   \\ \\  \\ \r\n   \\ \\__\\\\ \\__\\ \\_______\\ \\__\\\\ \\__\\ \\_______\\ \\_______\\ \\_______\\   \\ \\__\\\r\n    \\|__| \\|__|\\|_______|\\|__| \\|__|\\|_______|\\|_______|\\|_______|    \\|__|\r\n                                                                           \r\n                                                                           \r\n                                                                           ");
            Log.Info("Hello, I'm kokoBot!");
        }

        public static void Start()
        {
        }
    }
}