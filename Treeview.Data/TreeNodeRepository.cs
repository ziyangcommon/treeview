using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Treeview.Entity;

namespace Treeview.Data
{
    /// <summary>
    /// 树节点仓储
    /// </summary>
    public class TreeNodeRepository : IRepository<TreeNode>
    {

        /// <summary>
        /// 获取所有树节点,以列表非树形结构的方式返回.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TreeNode> FindAll()
        {
            return DBHelper<TreeNode>.FindAll();
        }

        /// <summary>
        /// 通过父级节点的UUID来获取树,顶级节点的UUID为
        /// Guid.Empty
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TreeNode> FindTree(TreeNode parentNode)
        {
            TreeviewContext context=new TreeviewContext();
            return FindTree(ref context, null);
        }

        /// <summary>
        /// 搜索并且构建树.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="parentNode">The parent TreeNode.</param>
        /// <returns>IEnumerable&lt;TreeNode&gt;.</returns>
        private IEnumerable<TreeNode> FindTree(ref TreeviewContext context, TreeNode parentNode)
        {
            if (parentNode == null)
            {
                IEnumerable<TreeNode> nodeList = context.TreeNodeSource.Where(m => m.ParentUUID == Guid.Empty);
                var treeRoot = nodeList as IList<TreeNode> ?? nodeList.ToList();
                if (treeRoot.Any())
                {
                    foreach (var TreeNode in treeRoot)
                    {
                        TreeNode.Level = 1;
                        TreeNode.Children = FindTree(ref context, TreeNode).OrderBy(m => m.SortSeed).ToList();
                    }
                }
                return treeRoot;
            }
            else
            {
                IEnumerable<TreeNode> menuList = context.TreeNodeSource.Where(m => m.ParentUUID == parentNode.UUID);
                var children = menuList as IList<TreeNode> ?? menuList.ToList();
                if (children.Any())
                {
                    foreach (TreeNode TreeNode in children)
                    {
                        TreeNode.Level = parentNode.Level + 1;
                        TreeNode.ParentName = parentNode.Name;
                        TreeNode.Children = FindTree(ref context, TreeNode).OrderBy(m => m.SortSeed).ToList();
                    }
                }
                return children;
            }
        }

        /// <summary>
        /// 树节点不支持分页,调用该函数会直接抛出异常.
        /// </summary>
        /// <param name="pindex">The pindex.</param>
        /// <param name="psize">The psize.</param>
        /// <param name="keyword">The keyword.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="orderby">The orderby.</param>
        /// <param name="count">The count.</param>
        /// <returns>IEnumerable&lt;TreeNode&gt;.</returns>
        /// <exception cref="Exception">树节点不支持分页.</exception>
        public IEnumerable<TreeNode> FindPage(int pindex, int psize, string keyword, string sortColumn, SortOrder @orderby,
            out int count)
        {
            throw new Exception("树节点不支持分页.");
        }

        /// <summary>
        /// Finds the specified UUID.
        /// </summary>
        /// <param name="uuid">The UUID.</param>
        /// <returns>TreeNode.</returns>
        public TreeNode Find(Guid uuid)
        {
            TreeviewContext context = new TreeviewContext();
            var TreeNode = context.TreeNodeSource.FirstOrDefault(m => m.UUID == uuid);
            if (TreeNode != null)
            {
                var firstOrDefault = context.TreeNodeSource.FirstOrDefault(m => TreeNode.ParentUUID == m.UUID);
                if (firstOrDefault != null)
                    TreeNode.ParentName = firstOrDefault.Name;
            }
            return TreeNode;
        }

        /// <summary>
        /// 通过关键字分页查询
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        /// <returns>IEnumerable&lt;TreeNode&gt;.</returns>
        /// <exception cref="Exception">不支持如此的查询.</exception>
        public IEnumerable<TreeNode> FindBy(string propertyName, object value)
        {
            //throw new Exception("暂不支持如此的查询.");
            return DBHelper<TreeNode>.FindBy(propertyName, value);
        }

        /// <summary>
        /// 更新对象
        /// </summary>
        /// <param name="newEntity"></param>
        /// <returns></returns>
        public TreeNode Update(TreeNode newEntity)
        {
            return DBHelper<TreeNode>.Update(newEntity);
        }

        /// <summary>
        /// 检查对象是否存在
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Exists(TreeNode entity)
        {
            return DBHelper<TreeNode>.Exists(entity);
        }

        /// <summary>
        /// 新增对象
        /// </summary>
        /// <param name="newEntity"></param>
        /// <returns></returns>
        public TreeNode Add(TreeNode newEntity)
        {
            return DBHelper<TreeNode>.Add(newEntity);
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public bool Delete(Guid uuid)
        {
            TreeviewContext context = new TreeviewContext();
            var dataSource = context.TreeNodeSource;
            var oldObj = dataSource.FirstOrDefault(o => o.UUID == uuid);
            if (oldObj == null) return true;
            List<Guid> needDeleteUUIDs=new List<Guid>();
            if (HasChildren(uuid, ref context))
            {
                GetDeleteChildren(uuid,ref context,ref needDeleteUUIDs);
                if (needDeleteUUIDs.Count > 0)
                {
                    foreach (var id in needDeleteUUIDs)
                    {
                        var tempObj = dataSource.FirstOrDefault(o => o.UUID == id);
                        if (tempObj == null) continue;
                        dataSource.Remove(tempObj);
                    }
                }
            }
            dataSource.Remove(oldObj);
            if (context.SaveChanges() > 0) return true;
            return true;
        }

        /// <summary>
        /// 当删除树节点时,递归查找其子集,一并删除.
        /// </summary>
        /// <param name="parentUUID"></param>
        /// <param name="context"></param>
        /// <param name="needDeleteUUIDs"></param>
        private void GetDeleteChildren(Guid parentUUID, ref TreeviewContext context, ref List<Guid> needDeleteUUIDs)
        {
            var children = context.TreeNodeSource.Where(m => m.ParentUUID == parentUUID).Select(m => m.UUID);
            if (children.Any())
            {
                foreach (Guid child in children)
                {
                    if (HasChildren(child, ref context))
                    {
                        GetDeleteChildren(child, ref context, ref needDeleteUUIDs);
                    }
                }
                needDeleteUUIDs.AddRange(children);
            }
        }

        /// <summary>
        /// 检查树节点是否存在子树节点
        /// </summary>
        /// <param name="parentUUID"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool HasChildren(Guid parentUUID, ref TreeviewContext context)
        {
            return context.TreeNodeSource.Any(m => m.ParentUUID == parentUUID);
        }

        /// <summary>
        /// 查看数据库中是否还有树节点数据
        /// </summary>
        /// <returns></returns>
        public bool HasData()
        {
            return DBHelper<TreeNode>.HasData();
        }

        /// <summary>
        /// 获取树节点的总数
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return DBHelper<TreeNode>.Count();
        }

        /// <summary>
        /// 以递归的方式删除树节点及其子树节点的数据
        /// </summary>
        /// <param name="uuids"></param>
        /// <returns></returns>
        public bool Delete(List<Guid> uuids)
        {
            return DBHelper<TreeNode>.Delete(uuids);
        }
    }
}