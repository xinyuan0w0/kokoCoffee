using BowlFrame.Message;
using BowlFrame.Target;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.TencentQQAdapter.Target
{
    internal class User(string uuid) : BowlFrame.Target.User(uuid)
    {
        public override string Nickname => "稻谷";

        public override byte[] Avatar => ;

        public override Task<bool> SendAsync(Messages messages)
        {
            throw new NotImplementedException();
        }
    }

    internal class ChannelUser(string uuid) : BowlFrame.Target.User(uuid)
    {
        public override string Nickname => throw new NotImplementedException();

        public override byte[] Avatar => throw new NotImplementedException();

        public override Task<bool> SendAsync(Messages messages)
        {
            throw new NotImplementedException();
        }
    }
}