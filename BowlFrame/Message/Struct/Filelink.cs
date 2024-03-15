using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Message.Struct
{
    public struct Filelink
    {
        public string MimeType { get; set; }

        public byte[]? Bytes { get; set; }

        public string? Path { get; set; }

        public string? Url { get; set; }
    }
}
