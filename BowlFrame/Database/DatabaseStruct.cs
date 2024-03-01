using Newtonsoft.Json.Linq;
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

        public object Value
        {
            readonly get => _int ?? _double ?? 0;
            set
            {
                Type type = value.GetType();

                if (type == typeof(short) || type == typeof(int) || type == typeof(long))
                {
                    _int = Convert.ToInt64(value);
                    _double = null;
                    IsDouble = false;
                }
                else if (type == typeof(float) || type == typeof(double))
                {
                    _int = null;
                    _double = Convert.ToDouble(value);
                    IsDouble = true;
                }
            }
        }

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