using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treeview.Data.Exceptions
{
    /// <summary>
    /// 对象已存在,在插入数据库对象时,重复数据
    /// 则抛出此异常
    /// </summary>
    public class EntityExistsException : Exception
    {
        public EntityExistsException(string msg) : base(msg) { }
    }
}
