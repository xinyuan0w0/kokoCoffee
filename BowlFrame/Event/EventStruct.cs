namespace BowlFrame.Event
{
    public enum EventType
    {
        Event = 0,
        Message = 1,
        Timer = 2
    }

    public enum SourceType
    {
        Private = 0,
        Group = 1,
        Guild = 2,
        Channel = 3,
        Post = 4,
    }
}