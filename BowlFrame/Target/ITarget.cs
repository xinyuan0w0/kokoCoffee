using BowlFrame.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Target
{
    public interface ITarget
    {
        public string ID { get; }

        public string UUID { get; }

        public IPlatform Platform { get; }
    }
}