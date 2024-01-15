using BowlFrame.Config;
using static BowlFrame.Tools.Logger;
using NLog;
using BowlFrame.Adapter;

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

            string conn_1 = AdapterManager.CreateAdapter(typeof(AdapterSample).FullName) ?? throw new NullReferenceException();
            string conn_2 = AdapterManager.CreateAdapter(typeof(AdapterSample)) ?? throw new NullReferenceException();

            AdapterManager.StartAdapter(conn_1);
            AdapterManager.StartAdapter(conn_2);

            AdapterManager.StopAdapter(conn_1);
            AdapterManager.StopAdapter(conn_2);

            AdapterManager.StartAdapter(conn_1);

            //AdapterManager.DisposeAdapter(conn_1);
            AdapterManager.DisposeAdapter(conn_2);

            AdapterManager.Dispose();
        }
    }
}