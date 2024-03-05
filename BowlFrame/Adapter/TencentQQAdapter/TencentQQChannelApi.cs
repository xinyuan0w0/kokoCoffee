using BowlFrame.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.TencentQQAdapter
{
    public class TencentQQChannelApi(TencentQQ tencentQQ)
    {
        private readonly TencentQQ tencentQQ = tencentQQ;

        public async Task SendMessage(Messages messages)
        {
        }
    }
}