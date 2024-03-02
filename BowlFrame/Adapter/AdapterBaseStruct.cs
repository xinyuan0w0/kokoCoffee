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
            Name = "Test",
            ID = "cn.kokobot.test",
            Platform = "Test",
            Description = "测试",
        };

        public string ID => "Test";
    }
}