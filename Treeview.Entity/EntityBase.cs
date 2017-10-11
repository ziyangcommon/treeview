using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Treeview.Entity
{
    /// <summary>
    /// 实体层的基类
    /// </summary>
    public abstract class EntityBase
    {
        private DateTime _lastModifyDate = DateTime.Now;
        private DateTime _createdDate = DateTime.Now;

        /// <summary>
        /// 实体主键
        /// </summary>
        [Key]
        public virtual Guid UUID { get; set; }

        /// <summary>
        /// 描述信息
        /// </summary>
        [DataSearchContainAttribute]
        public virtual string Description { get; set; }


        /// <summary>
        /// 用户创建时间,精度yyyy-MM-dd hh:mm:ss.fff
        /// </summary>
        public virtual DateTime CreatedDate
        {
            get { return _createdDate; }
            set { _createdDate = value; }
        }

        /// <summary>
        /// 最后修改时间,精度yyyy-MM-dd hh:mm:ss.fff
        /// </summary>
        public virtual DateTime LastModifyDate
        {
            get { return _lastModifyDate; }
            set { _lastModifyDate = value; }
        }
    }
}