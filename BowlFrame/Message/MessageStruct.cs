using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Message
{
    public enum MetaType
    {
        /// <summary>
        /// 通用字段
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 拓展字段(用于适配器传递额支持的内容)
        /// </summary>
        Extra = 1,

        /// <summary>
        /// 自定义字段(用于插件传递内容)
        /// </summary>
        Custom = 2
    }

    public struct MessageBlock
    {
        public string Name { get; set; }

        public MetaType MetaType { get; set; }

        public object Value { get; set; }

        /// <summary>
        /// 可能存在多个同样的字段
        /// </summary>
        public bool HaveMulit { get; set; }
    }
}