namespace BowlFrame.Exceptions.Database.DatabaseEx
{
    internal class ExtraData : DatabaseException
    {
        public ExtraData() : base("从数据库读取Data结构时发现重复数据")
        {
        }

        public ExtraData(string key, string? subkey) : base($"从数据库读取Data结构时发现重复数据 key: {key} ${(subkey is not null ? $"subkey: {subkey}" : "")}")
        {
        }
    }
}