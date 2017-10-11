using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treeview.Entity
{
    /// <summary>
    /// 树节点实体类
    /// </summary>
    [Table("TreeNode")]
    public class TreeNode:EntityBase
    {
        /// <summary>
        /// 树节点的名字
        /// </summary>
        [DataSearchContain]
        public string Name { get; set; }

        /// <summary>
        /// 菜单的级别,一级菜单为1
        /// </summary>
        [NotMapped]
        public uint Level { get; set; }

        /// <summary>
        /// 父级菜单的UUID,如果是一级菜单
        /// 则其父级UUID为
        /// System.Guid.Empty.
        /// 即:00000000-0000-0000-0000-000000000000
        /// </summary>
        public virtual Guid ParentUUID { get; set; }

        /// <summary>
        /// 排序序列号,对节点按照指定的顺序排序
        /// ****该值的默认值是0,如果是0则不参与排序*****
        /// 在读取同级别树节点的时候,如果该序列不
        /// 为0,那么将视为该级别的节点下该节点
        /// 拥有有限排序
        /// 例如:
        ///     --treenode1
        ///         +treenode1-1
        ///         +treenode1-2
        ///         +treenode1-3
        /// 如果menu1下面的三个子节点treenode1-2的SortSeed为199
        /// 而其他两个treenode1-1和treenode1-3的SortSeed都为0,那么
        /// menu1-2将优先排序到前面,会变成:
        ///  --treenode1
        ///         +treenode1-2
        ///         +treenode1-1
        ///         +treenode1-3
        /// </summary>
        public virtual int SortSeed { get; set; }

        /// <summary>
        /// 菜单在浏览器中渲染的时候,树节点前面的图标所使用的
        /// 类选择器的名字.
        /// </summary>
        public virtual string IconClassName { get; set; }

        /// <summary>
        /// 其父级节点名字,该属性不会写入数据库.
        /// </summary>
        [NotMapped]
        public string ParentName { get; set; }

        /// <summary>
        /// 如果该节点不是最低级别节点
        /// 这里是它的子节点的集合.
        /// </summary>
        [NotMapped]
        public List<TreeNode> Children { get; set; }
    }
}
