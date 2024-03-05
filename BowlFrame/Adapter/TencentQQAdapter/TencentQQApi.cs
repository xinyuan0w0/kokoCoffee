using BowlFrame.Event;
using BowlFrame.Message;
using BowlFrame.Target;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.TencentQQAdapter
{
    public class TencentQQApi(TencentQQ tencentQQ, EventBase? eventBase = null)
    {
        private readonly TencentQQ tencentQQ = tencentQQ;

        private readonly EventBase? eventBase = eventBase;

        public async Task SendMessage(Messages messages, ITarget? target = null)
        {
            string msg = "";

            foreach (MessageBlock message in messages.MessageBlocks)
            {
                if (message.MetaType == MetaType.Normal)
                    switch (message.Name)
                    {
                        case "Pictrue":
                            break;
                    }
            }
        }

        private async Task UploadMedia()
        {
        }
    }
}