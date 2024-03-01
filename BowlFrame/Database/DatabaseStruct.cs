using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Database
{
    public struct ChangeInfo()
    {
        private long? _int = null;
        private double? _double = null;

        public string UUID { get; set; }
        public string Key { get; set; }
        public string SubKey { get; set; }

        public readonly object Value => _int ?? _double ?? 0;

        public bool? IsDouble { get; private set; } = null;

        public void Change(long value)
        {
            _int = value;
            _double = null;
            IsDouble = false;
        }

        public void Change(double value)
        {
            _int = null;
            _double = value;
            IsDouble = true;
        }
    }
}