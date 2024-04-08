namespace BowlFrame.Adapter.CocoaPlugin
{
    public interface ICocoaPlugin
    {
        public bool Init();

        public bool Enable();

        public bool Disable();
    }
}