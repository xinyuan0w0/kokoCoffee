using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.CocoaPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CocoaFuncAttribute(string funcName) : Attribute
    {
        public string FuncName { get; init; } = funcName;
    }
}