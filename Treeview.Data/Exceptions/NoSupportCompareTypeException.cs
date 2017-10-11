using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treeview.Data.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class NoSupportCompareTypeException : Exception
    {
        public NoSupportCompareTypeException(string msg) : base(msg) { }
    }
}
