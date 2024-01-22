using BowlFrame;

namespace CocoaFunc
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BowlService.Initialization();
            BowlService.Start();

            EventWaitHandle _waitHandle = new AutoResetEvent(false);
            _waitHandle.WaitOne();
        }
    }
}