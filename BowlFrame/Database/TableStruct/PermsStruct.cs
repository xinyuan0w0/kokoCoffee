using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BowlFrame.Database.TableStruct.DataTypeToJTokenType;

namespace BowlFrame.Database.TableStruct
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    [SugarTable("perms")]
    public class DbPerms
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int ID { get; set; }

        public string UUID { get; set; }

        public string Permission { get; set; }

        public bool Value { get; set; }

        public string Area { get; set; }

        public long? Expir { get; set; }

        public string? Content { get; set; }
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
}