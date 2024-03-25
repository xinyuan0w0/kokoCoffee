namespace BowlFrame.Event
{
    public enum EventType
    {
        System = 0,
        Event = 1,
        Message = 2,
        Timer = 3,
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