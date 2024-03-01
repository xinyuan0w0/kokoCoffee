using BowlFrame.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Target
{
    /// <summary>
    /// 预留功能
    /// </summary>
    /// <param name="uuid"></param>
    public abstract class Post(string uuid)
    {
        public string UUID { get; } = uuid;

        /// <summary>
        /// 主动发送
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public abstract Task<bool> SendAsync(Messages messages);
    }
}