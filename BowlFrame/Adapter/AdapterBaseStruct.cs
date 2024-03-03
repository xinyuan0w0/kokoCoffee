using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter
{
    internal class BasePlatform : IPlatform
    {
        public AdapterInfo AdapterInfo { get; } = new()
        {
            Name = "BowlFrame",
            ID = "cn.kokobot",
            Platform = "BowlFrame",
            Description = "内部使用",
        };

        public string ID { get; init; } = "BowlFrame";
    }
}