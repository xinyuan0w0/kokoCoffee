using BowlFrame.Adapter;
using BowlFrame.Message;
using BowlFrame.Perm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Target
{
    public abstract class Guild(string uuid) : ITarget
    {
        public string UUID { get; } = string.IsNullOrEmpty(uuid) ? throw new ArgumentNullException() : uuid;

        public abstract string ID { get; }

        public abstract IPlatform Platform { get; }

        public abstract Permission Permission { get; }
    }
}