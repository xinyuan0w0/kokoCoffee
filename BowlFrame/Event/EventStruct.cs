namespace BowlFrame.Event
{
    public enum MsgType
    {
        Event = 0,
        Message = 1,
        Timer = 2
    }

    public enum SourceType
    {
        Private = 0,
        Group = 1,
        Channel = 2,
        Post = 3,
    }
}