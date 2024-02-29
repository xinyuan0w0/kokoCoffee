using SqlSugar;

namespace BowlFrame.Database.TableStruct
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    [SugarTable("platformid")]
    public class DbPlatformID
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string UUID { get; set; }

        /// <summary>
        /// 取决于适配器返回的平台名
        /// </summary>
        public string Platform { get; set; }

        public string? FatherUUID { get; set; }

        public string ID { get; set; }
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
}