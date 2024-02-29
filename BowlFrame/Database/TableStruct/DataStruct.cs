using Newtonsoft.Json.Linq;
using SqlSugar;
using static BowlFrame.Database.TableStruct.DataTypeToJTokenType;

namespace BowlFrame.Database.TableStruct
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    [SugarTable("data")]
    public class DbData
    {
        public string UUID { get; set; }

        public string Key { get; set; }

        public string SubKey { get; set; }

        public DbDataType DataType { get; set; }

        public string Value { get; set; }
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    public static class DataTypeToJTokenType
    {
        public enum DbDataType
        {
            Null = 0,
            Integer = 1,
            Float = 2,
            Boolean = 3,
            String = 4,
            Array = 5,
            Object = 6,
            Date = 7,
            Bytes = 8,
        }

        public static DbDataType? GetDataType(JTokenType tokenType)
        {
            return tokenType switch
            {
                JTokenType.Null => DbDataType.Null,
                JTokenType.Integer => DbDataType.Integer,
                JTokenType.Float => DbDataType.Float,
                JTokenType.Boolean => DbDataType.Boolean,
                JTokenType.String => DbDataType.String,
                JTokenType.Array => DbDataType.Array,
                JTokenType.Object => DbDataType.Object,
                JTokenType.Date => DbDataType.Date,
                JTokenType.Bytes => DbDataType.Bytes,
                _ => null,
            };
        }

        public static JTokenType? GetJTokenType(DbDataType DataType)
        {
            return DataType switch
            {
                DbDataType.Null => JTokenType.Null,
                DbDataType.Integer => JTokenType.Integer,
                DbDataType.Float => JTokenType.Float,
                DbDataType.Boolean => JTokenType.Boolean,
                DbDataType.String => JTokenType.String,
                DbDataType.Array => JTokenType.Array,
                DbDataType.Object => JTokenType.Object,
                DbDataType.Date => JTokenType.Date,
                DbDataType.Bytes => JTokenType.Bytes,
                _ => null,
            };
        }
    }
}