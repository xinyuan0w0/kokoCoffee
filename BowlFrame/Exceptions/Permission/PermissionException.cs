namespace BowlFrame.Exceptions.Permission
{
    public class PermissionException : Exception
    {
        public PermissionException() : base("发生权限异常")
        {
        }

        public PermissionException(string message) : base(message)
        {
        }

        public PermissionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}