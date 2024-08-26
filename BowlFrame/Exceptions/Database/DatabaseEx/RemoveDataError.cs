namespace BowlFrame.Exceptions.Database.DatabaseEx
{
    internal class RemoveDataError : DatabaseException
    {
        public RemoveDataError() : base("从数据库删除Data结构数据出现错误")
        {
        }

        public RemoveDataError(string key, string? subkey) : base($"从数据库删除Data结构数据出现错误 key: {key} ${(subkey is not null ? $"subkey: {subkey}" : "")}")
        {
        }
    }
}