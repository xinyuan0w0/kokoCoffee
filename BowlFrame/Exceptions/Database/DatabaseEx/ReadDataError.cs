namespace BowlFrame.Exceptions.Database.DatabaseEx
{
    public class ReadDataError : DatabaseException
    {
        public ReadDataError() : base("从数据库读取Data结构数据出现错误")
        {
        }

        public ReadDataError(string key, string? subkey) : base($"从数据库读取Data结构数据出现错误 key: {key} ${(subkey is not null ? $"subkey: {subkey}" : "")}")
        {
        }
    }
}