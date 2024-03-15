using SqlSugar;

namespace BowlFrame.Database.TableStruct
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    [SugarTable("id")]
    public class DbID
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int UID { get; set; }

        [SugarColumn(IsPrimaryKey = true)]
        public string UUID { get; set; }

        public TargetType Type { get; set; }

        [SugarColumn(IsIgnore = true)]
        public static DbID Empty { get; } = new();
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    public enum TargetType
    {
        Remove = -1,
        Groups = 0,
        User = 1,
        Group = 2,
        Guild = 3,
        Channel = 4,
        Func = 5,
    }
}