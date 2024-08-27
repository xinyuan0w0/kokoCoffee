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

        public static MessageBlock CreateMessageBlock(string name, object? value = null, MetaType metaType = MetaType.Normal, bool haveMulit = true)
        {
            return new MessageBlock
            {
                HaveMulit = haveMulit,
                MetaType = metaType,
                Name = name,
                Value = value,
            };
        }
    }
}