namespace BowlFrame.Exceptions.Database.DatabaseEx
{
    internal class WriteDataError : DatabaseException
    {
        public WriteDataError() : base("从数据库写入Data结构数据出现错误")
        {
        }

        public WriteDataError(string key, string? subkey) : base($"从数据库写入Data结构数据出现错误 key: {key} ${(subkey is not null ? $"subkey: {subkey}" : "")}")
        {
        }
    }
}