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

        /// <summary>
        /// 用于内部构造使用
        /// </summary>
        public string ID { get; init; } = "BowlFrame";

        /// <summary>
        /// 用于内部构造使用
        /// </summary>
        public string? ConnectID { get; init; } = null;
    }

    internal class AutoPlatformTarget(AdapterInfo adapterInfo, string id, string? connectID) : IPlatform
    {
        public AdapterInfo AdapterInfo { get; } = adapterInfo;

        public string ID { get; } = id;

        public string? ConnectID { get; } = connectID;
    }
}