using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Perm
{
    public struct PermissionInfo
    {
        public int ID { get; set; }

        public string UUID { get; set; }

        public string Permission { get; set; }

        public bool Value { get; set; }

        public string Area { get; set; }

        public long? Expir { get; set; }

        public string? Content { get; set; }
    }
}