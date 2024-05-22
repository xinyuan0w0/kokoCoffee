using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.CocoaPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CocoaBindEventAttribute(string @event) : Attribute
    {
        public string Event { get; init; } = @event;
    }
}