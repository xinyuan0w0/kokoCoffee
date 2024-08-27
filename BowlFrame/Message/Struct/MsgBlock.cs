namespace BowlFrame.Message.Struct
{
    public static class MsgBlock
    {
        public struct MessageBlock
        {
            public string Name { get; set; }

            public MetaType MetaType { get; set; }

            public object? Value { get; set; }

            /// <summary>
            /// 可能存在多个同样的字段
            /// </summary>
            public bool HaveMulit { get; set; }
        }
    }
}