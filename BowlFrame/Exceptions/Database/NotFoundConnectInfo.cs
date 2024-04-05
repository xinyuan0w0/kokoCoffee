namespace BowlFrame.Exceptions.Database
{
    internal class NotFoundConnectInfo : DatabaseException
    {
        public NotFoundConnectInfo() : base("未找到数据库连接信息")
        {
        }

        public NotFoundConnectInfo(string? driver) : base($"未找到数据库连接信息 {driver}")
        {
        }
    }
}