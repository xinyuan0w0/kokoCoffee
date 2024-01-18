using BowlFrame;

namespace CocoaFunc
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await BowlService.Initialization();
        }
    }
}