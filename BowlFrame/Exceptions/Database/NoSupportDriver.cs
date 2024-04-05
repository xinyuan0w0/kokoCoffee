namespace BowlFrame.Exceptions.Database
{
    internal class NoSupportDriver : DatabaseException
    {
        public NoSupportDriver() : base("不支持的数据库驱动")
        {
        }

        public NoSupportDriver(string? driver) : base($"不支持的数据库驱动 {driver}")
        {
        }
    }
}