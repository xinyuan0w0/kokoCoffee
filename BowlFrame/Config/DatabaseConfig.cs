using BowlFrame.Exceptions.Database;
using Newtonsoft.Json.Linq;
using SqlSugar;

namespace BowlFrame.Config
{
    internal static class DatabaseConfig
    {
        public static DbType Driver;
        public static readonly string ConnectionString;

        static DatabaseConfig()
        {
            string path = Path.Combine(PathConfig.ConfigPath, "Database.json");
            JObject value = JObject.Parse(File.ReadAllText(path));

            switch (((string?)value["Driver"])?.ToLower())
            {
                case "mysql":
                    Driver = DbType.MySql;
                    MySqlDriver driver = value.ToObject<MySqlDriver>();
                    ConnectionString = $"Host={driver.Host};{(driver.Port is not null ? $"Port={driver.Port};" : "")}"
                        + $"Username={driver.Username};Password={driver.Password};Database={driver.Database};"
                        + (driver.ExtraString is not null ? driver.ExtraString : "");
                    break;

                default:
                    throw new NoSupportDriver((string?)value["Driver"]);
            }
        }
    }

    internal struct MySqlDriver
    {
        public string Driver { get; set; }
        public string Host { get; set; }
        public int? Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string? ExtraString { get; set; }
    }
}