using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using static System.Collections.Specialized.BitVector32;
using static BowlFrame.Tools.Logger;
using BowlFrame.Event.Message;

namespace BowlFrame.Event
{
    public static class MainEvent
    {
        public static async void Main(IEvent @event)
        {
            Log.Info(@event.ToString());

            switch (@event)
            {
                case MessageBase:
                    MessageEvent(@event as MessageBase ?? throw new NullReferenceException());
                    break;
                default:
                    break;
            }
        }

        public static async void MessageEvent(MessageBase MessageEvent)
        {

        }
    }
}