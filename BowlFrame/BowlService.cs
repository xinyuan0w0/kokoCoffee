using BowlFrame.Adapter;
using BowlFrame.Config;
using BowlFrame.Database;
using BowlFrame.Database.TableStruct;
using NanoidDotNet;
using Newtonsoft.Json.Linq;
using NLog;
using static BowlFrame.Tools.Logger;

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
            AdapterManagerEx.CreateAdapterFromFile(Path.Combine(PathConfig.ConfigPath, "Adapter.json"));
            AdapterManagerEx.StartAllAdapter().Wait();
        }

        public static void Stop()
        { }

        public static async void Debug()
        {
            DatabaseClient database = new();

            //List<Task> tasks = [];

            int count = await database.Client.Queryable<DbPlatformID>().CountAsync();

            Log.Trace($"数据库对象数量: {count}");

            //var query = database.Client.Queryable<DbPlatformID>();

            //var result =
            //     from n in query
            //     where n.ID == "16581252311272482171"
            //     select n;

            //JObject a = await database.ReadJsonFromData("5qySHjU2oPR_hHK0l8BGc", new[] { "Register", "Check", "Sign" });

            //for (int i = 0; i < 1000; i++)
            //{
            //    tasks.Clear();

            //    tasks.Add(database.WriteJsonIntoData(Nanoid.Generate(), a));

            //    Task.WaitAll(tasks.ToArray());
            //}
        }
    }
}