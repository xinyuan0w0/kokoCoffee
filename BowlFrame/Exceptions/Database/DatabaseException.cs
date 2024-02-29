namespace BowlFrame.Exceptions.Database
{
    public class DatabaseException : Exception
    {
        public DatabaseException() : base("发生数据库异常")
        {
        }

        public DatabaseException(string message) : base(message)
        {
        }

        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}