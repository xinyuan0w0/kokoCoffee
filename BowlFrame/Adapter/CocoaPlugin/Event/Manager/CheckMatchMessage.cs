using BowlFrame.Event.Message;

namespace BowlFrame.Adapter.CocoaPlugin.Event.Manager
{
    public class CheckMatchMessage
    {
        public delegate bool CheckMatchMessageHandler(CocoaPluginConfig pluginConfig, CocoaPluginFuncConfig funcConfig, MessageBase messageBase);

        public static bool BindEventManager(EventManager manager) => manager.AddEvent("CheckMatchMessage", typeof(CheckMatchMessageHandler));
    }
}