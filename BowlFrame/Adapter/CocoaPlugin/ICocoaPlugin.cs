namespace BowlFrame.Adapter.CocoaPlugin
{
    public interface ICocoaPlugin : IDisposable
    {
        public bool Init(CocoaPluginPath cocoaPluginpath);

        public bool Enable();

        public bool Disable();
    }
}