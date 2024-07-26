namespace BowlFrame.Adapter.CocoaPlugin
{
    public interface ICocoaPlugin : IDisposable
    {
        public bool Init(CocoaPluginpath cocoaPluginpath);

        public bool Enable();

        public bool Disable();
    }
}