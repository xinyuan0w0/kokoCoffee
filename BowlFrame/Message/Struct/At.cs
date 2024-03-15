using BowlFrame.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Message.Struct
{
    public struct At
    {
        public string? UUID { get; set; }

        public IPlatform Platform { get; set; }

        public string ID { get; set; }
    }
}