using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace silushop.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class NoFilter : Attribute
    {
    }
}