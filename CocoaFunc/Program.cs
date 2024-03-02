using BowlFrame;
using System.Diagnostics;

namespace CocoaFunc
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await BowlService.Initialization();
            await BowlService.Start();

            if (Debugger.IsAttached)
                BowlService.Debug();

            //EventWaitHandle _waitHandle = new AutoResetEvent(false);
            //_waitHandle.WaitOne();
            await Task.Delay(-1);
        }
    }
}