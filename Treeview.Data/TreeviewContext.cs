using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Treeview.Entity;

namespace Treeview.Data
{
    /// <summary>
    /// 系统对应的数据库上下文
    /// 为和Entity Framework交互的核心类
    /// </summary>
    internal class TreeviewContext : DbContext
    {
        private DbSet<TreeNode> _treeNodeSource;
        private static object _treeNodeLocker = new object();

        static TreeviewContext()
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<TreeviewContext>());
            TreeviewContext context = new TreeviewContext();
            context.Database.CreateIfNotExists();
        }
        public TreeviewContext()
            : base("name=TreeviewContext")
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<TreeviewContext>(null);
            base.OnModelCreating(modelBuilder);
        }

        //private static TreeviewContext _context;
        //internal static DbSet<TEntity> DataSource<TEntity> { get { return GetDatasource(); } }

        /// <summary>
        /// 从context中查找到实体对应的数据表.
        /// 具体做法是遍历对象的属性,找到指定
        /// 类型的集合
        /// </summary>
        /// <returns></returns>
        internal DbSet<TEntity> GetDatasource<TEntity>() where TEntity : EntityBase
        {
            try
            {
                if (this.Database.Connection.State != ConnectionState.Open)
                    this.Database.Connection.Open();
                var properties = typeof(TreeviewContext).GetProperties();
                object datasource = null;
                foreach (var propertyInfo in properties)
                {
                    if (propertyInfo.PropertyType == typeof(DbSet<TEntity>))
                    {
                        datasource = propertyInfo.GetValue(this, null);
                        break;
                    }
                }
                if (Equals(datasource, null)) return null;
                return (DbSet<TEntity>)datasource;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 树形节点表映射
        /// </summary>
        public DbSet<TreeNode> TreeNodeSource
        {
            get => _treeNodeSource;
            set
            {
                lock (_treeNodeLocker)
                {
                    _treeNodeSource = value;
                }
            }
        }
    }
}
