using BowlFrame.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Target
{
    public abstract class Channel(string uuid)
    {
        public string UUID { get; } = uuid;
    }
}