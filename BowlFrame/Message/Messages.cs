using static BowlFrame.Message.Struct.MsgBlock;

namespace BowlFrame.Message
{
    public class Messages
    {
        private readonly List<MessageBlock> messageBlocks = [];

        public List<MessageBlock> MessageBlocks => messageBlocks;

        public bool Add(ArraySegment<MessageBlock> messageBlocks)
        {
            foreach (MessageBlock messageBlock in messageBlocks)
                if (!Add(messageBlock))
                    return false;
            return true;
        }

        public bool Add(MessageBlock messageBlock)
        {
            messageBlock.HaveMulit = IsMulitBlock(messageBlock);

            if (!messageBlock.HaveMulit)
                if (GetIndex(messageBlock.Name) is not null)
                    return false;

            messageBlocks.Add(messageBlock);
            return true;
        }

        public List<MessageBlock> GetAll(string name)
        {
            List<MessageBlock> result = [];

            int? index = 0;
            while (true)
            {
                index = GetIndex(name, (int)index) + 1;
                if (index is not null)
                    result.Add(messageBlocks[(int)index]);
                else
                    break;
            }

            return result;
        }

        public MessageBlock? Get(int index)
        {
            if (index < messageBlocks.Count)
                return messageBlocks[index];
            return null;
        }

        public MessageBlock? Get(string name, int startIndex = 0)
        {
            int? index = GetIndex(name, startIndex);

            if (index is null) return null;
            return messageBlocks[(int)index];
        }

        public int? GetIndex(string name, int startIndex = 0)
        {
            for (int i = startIndex; i < messageBlocks.Count; i++)
            {
                if (messageBlocks[i].Name == name)
                    return i;
            }
            return null;
        }

        public bool DelAll(string name)
        {
            int? result;
            while (true)
            {
                result = GetIndex(name);
                if (result is null)
                    break;

                if (!DelIndex((int)result))
                    return false;
            }
            return true;
        }

        public bool Del(string name, int startIndex = 0)
        {
            int? result = GetIndex(name, startIndex);
            if (result is null)
                return false;
            return DelIndex((int)result);
        }

        public bool DelIndex(int index)
        {
            if (index >= messageBlocks.Count)
                return false;

            messageBlocks.RemoveAt(index);
            return true;
        }

        public static bool IsMulitBlock(MessageBlock messageBlock)
        {
            //部分字段不允许重复内容
            if (messageBlock.Name == "RawText" || messageBlock.Name == "AtAll")
                messageBlock.HaveMulit = false;

            return messageBlock.HaveMulit;
        }
    }
}