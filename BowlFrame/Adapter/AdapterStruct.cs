using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter
{
    public struct AdapterInfo
    {
        public string Name;
        public string ID;
        public string Platform;
        public string Description;
    }

    public interface IPlatform
    {
        public string Name { get; }

        /// <summary>
        /// 唯一ID,同平台不同适配器要求不同
        /// </summary>
        public string ID { get; }
    }
}