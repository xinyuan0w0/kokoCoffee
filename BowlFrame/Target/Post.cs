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
    /// <summary>
    /// 预留功能
    /// </summary>
    /// <param name="uuid"></param>
    public abstract class Post(string uuid) : ITarget
    {
        public string UUID { get; } = string.IsNullOrEmpty(uuid) ? throw new ArgumentNullException() : uuid;

        public abstract string ID { get; }

        public abstract IPlatform Platform { get; }

        public abstract Permission Permission { get; }

        /// <summary>
        /// 主动发送
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public abstract Task<bool> SendAsync(Messages messages);
    }
}