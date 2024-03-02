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
            IPlatform platform = new BasePlatform();
            Permission database = new(platform);
            Permission database2 = new(platform);
            Permission database3 = new(platform);
            Permission database4 = new(platform);

            List<Task> tasks = [];

            //database.CreateTarget("test", TargetType.User, platform).Wait();

            //int count = await database.Client.Queryable<DbPlatformID>().CountAsync();

            //Log.Trace($"数据库对象数量: {count}");

            for (int i = 0; i < 80; i += 4)
            {
                tasks.Clear();

                tasks.Add(database.CreateTarget(i.ToString(), TargetType.User, platform));
                tasks.Add(database2.CreateTarget((i - 1).ToString(), TargetType.User, platform));
                tasks.Add(database3.CreateTarget((i - 2).ToString(), TargetType.User, platform));
                tasks.Add(database4.CreateTarget((i - 3).ToString(), TargetType.User, platform));

                Task.WaitAll([.. tasks]);
            }

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