using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Exceptions.Permission
{
    internal class NotFoundTargetPlatform : PermissionException
    {
        public NotFoundTargetPlatform() : base("未找到某个对象映射的平台信息")
        {
        }

        public NotFoundTargetPlatform(string? uuid) : base($"未找到 {uuid} 对象映射的平台信息 ")
        {
        }
    }
}