using BowlFrame.Config;
using static BowlFrame.Tools.Logger;
using NLog;
using BowlFrame.Adapter;
using BowlFrame.Net.WebSocket;
using System.Net.WebSockets;

namespace BowlFrame
{
    public class BowlService
    {
        public static async void Initialization()
        {
            Console.WriteLine(PathConfig.Path);
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(PathConfig.ConfigPath, "nlog.config"));

            Console.WriteLine(" ___  __    ________  ___  __    ________  ________  ________  _________   \r\n|\\  \\|\\  \\ |\\   __  \\|\\  \\|\\  \\ |\\   __  \\|\\   __  \\|\\   __  \\|\\___   ___\\ \r\n\\ \\  \\/  /|\\ \\  \\|\\  \\ \\  \\/  /|\\ \\  \\|\\  \\ \\  \\|\\ /\\ \\  \\|\\  \\|___ \\  \\_| \r\n \\ \\   ___  \\ \\  \\\\\\  \\ \\   ___  \\ \\  \\\\\\  \\ \\   __  \\ \\  \\\\\\  \\   \\ \\  \\  \r\n  \\ \\  \\\\ \\  \\ \\  \\\\\\  \\ \\  \\\\ \\  \\ \\  \\\\\\  \\ \\  \\|\\  \\ \\  \\\\\\  \\   \\ \\  \\ \r\n   \\ \\__\\\\ \\__\\ \\_______\\ \\__\\\\ \\__\\ \\_______\\ \\_______\\ \\_______\\   \\ \\__\\\r\n    \\|__| \\|__|\\|_______|\\|__| \\|__|\\|_______|\\|_______|\\|_______|    \\|__|\r\n                                                                           \r\n                                                                           \r\n                                                                           ");
            Log.Info("Hello, I'm kokoBot!");

            WSClient client = new(new Uri("wss://api.botfan.cn"));

            client.ReceiveEvent += (WSClient client, byte[] bytes, WebSocketReceiveResult receiveResult) => { Console.WriteLine(bytes.ToString()); };

            await client.ConnectAsync();

            string? input;

            while (true)
            {
                input = Console.ReadLine();
                if (input == "stop")
                {
                    await client.CloseAsync();
                    break;
                }
                else
                    await client.SendAsync(input ?? "Null");
            }
        }
    }
}