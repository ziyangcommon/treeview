using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Treeview.Data.Exceptions;
using Treeview.Entity;
using Treeview.Util;

namespace Treeview.Data
{
    /// <summary>
    /// 数据处理辅助类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal static class DBHelper<TEntity> where TEntity : EntityBase
    {
        static DBHelper()
        {
            //DataSource = GetDatasource();
        }

        /// <summary>
        /// 获取数据集中的所有元素.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TEntity> FindAll()
        {
            TreeviewContext context = new TreeviewContext();
            return context.GetDatasource<TEntity>();
        }

        /// <summary>
        /// 通过UUID查找数据集中的某一个对象
        /// 没找到则返回null
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public static TEntity Find(Guid uuid)
        {
            TreeviewContext context = new TreeviewContext();
            return context.GetDatasource<TEntity>().FirstOrDefault(o => o.UUID == uuid);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="pindex"></param>
        /// <param name="psize"></param>
        /// <param name="orderbyCol"></param>
        /// <param name="sortOrder"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public static IEnumerable<TEntity> FindPage(int pindex, int psize, string orderbyCol, SortOrder sortOrder, out int total)
        {
            TreeviewContext context = new TreeviewContext();
            //return (from e in DataSource orderby GetPropertyValue(e, orderbyCol) select e).Take(psize);
            var temp = context.GetDatasource<TEntity>()
                .AsEnumerable()
                .OrderBy(o => Common.GetPropertyValue(o, orderbyCol));
            total = temp.Count();
            return temp.Take(pindex * psize).Skip(psize * (pindex - 1));
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="pindex"></param>
        /// <param name="psize"></param>
        /// <param name="keyword"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortOrder"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public static IEnumerable<TEntity> FindPage(int pindex, int psize, string keyword, string sortColumn,
            SortOrder sortOrder, out int total)
        {
            TreeviewContext context = new TreeviewContext();
            if (string.IsNullOrEmpty(keyword)) return FindPage(pindex, psize, sortColumn, sortOrder, out total);
            var temp = context.GetDatasource<TEntity>()
                .AsEnumerable()
                .Where(e => ContansKeyword(e, keyword))
                .OrderBy(o => Common.GetPropertyValue(o, sortColumn));
            total = temp.Count();
            return temp.Take(pindex * psize).Skip(psize * (pindex - 1));
        }

        public static bool ContansKeyword(TEntity e, string keyword)
        {
            var properties = e.GetType().GetProperties();
            foreach (var property in properties)
            {
                var customAtts = property.GetCustomAttributes(typeof(DataSearchContainAttribute), true);
                if (customAtts.Any())
                {
                    return property.GetValue(e, null).ToString().Contains(keyword);
                }
            }
            return false;
        }

        /// <summary>
        /// 通过对象的属性名和指定的值搜寻对象集合
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<TEntity> FindBy(string propertyName, object value)
        {
            var valueType = value.GetType();
            if (valueType.IsPrimitive || valueType == typeof(string) || valueType == typeof(Guid))
            {
                TreeviewContext context = new TreeviewContext();
                return context.GetDatasource<TEntity>().Where(o => Common.GetPropertyValue(o, null) == value);
            }
            throw new NoSupportCompareTypeException("不支持查询的属性");
        }

        /// <summary>
        /// 通过实体属性名称和其对应的值来搜索并且返回
        /// 指定页码和条数以及排序规则下的数据集合
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <param name="pindex"></param>
        /// <param name="psize"></param>
        /// <param name="orderbyCol"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public static IEnumerable<TEntity> FindPageBy(string propertyName, object value, int pindex, int psize,
            string orderbyCol, string sortOrder = "asc")
        {
            var valueType = value.GetType();
            if (valueType.IsPrimitive || valueType == typeof(string) || valueType == typeof(Guid))
            {
                TreeviewContext context = new TreeviewContext();
                return
                    context.GetDatasource<TEntity>()
                        .Where(o => Common.GetPropertyValue(o, null) == value)
                        .AsEnumerable()
                        .OrderBy(o => Common.GetPropertyValue(o, orderbyCol))
                        .Take(pindex * psize)
                        .Skip(psize * (pindex - 1));
            }
            throw new NoSupportCompareTypeException("不支持查询的属性");
        }

        /// <summary>
        /// 添加一个新的对象,并且该函数会给对象自动赋予UUID,
        /// 如果外部已经有生成一个UUID,执行该函数后UUID会
        /// 被重写,如果数据插入成功,将返回该对象,如果出错,那么
        /// 则返回一个空对象.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static TEntity Add(TEntity e)
        {
            if (Exists(e)) throw new EntityExistsException("Entity has exists.");
            if (e.UUID == Guid.Empty) e.UUID = Guid.NewGuid();
            TreeviewContext context = new TreeviewContext();
            context.GetDatasource<TEntity>().Add(e);
            int flag = context.SaveChanges();
            if (flag > 0) return e;
            return null;
        }

        /// <summary>
        /// 删除一个元素,如果元素不存在,则直接返回true
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns>
        /// 如果元素不存在,返回true
        /// 删除成功返回true
        /// 如果删除失败,则返回false
        /// </returns>
        public static bool Delete(Guid uuid)
        {
            TreeviewContext context = new TreeviewContext();
            var dataSource = context.GetDatasource<TEntity>();
            var oldObj = dataSource.FirstOrDefault(o => o.UUID == uuid);
            if (oldObj == null) return true;
            dataSource.Remove(oldObj);
            if (context.SaveChanges() > 0) return true;
            return true;
        }

        /// <summary>
        /// 删除一组元素,如果所有元素已经被删除了,则返回true
        /// 如果集合中的uuid对应的数据全部都是已删除(即数据库
        /// 中找不到的)那也返回true,视为删除成功了.
        /// </summary>
        /// <param name="uuids"></param>
        /// <returns></returns>
        public static bool Delete(List<Guid> uuids)
        {
            TreeviewContext context = new TreeviewContext();
            var dataSource = context.GetDatasource<TEntity>();
            foreach (var uuid in uuids)
            {
                var oldObj = dataSource.FirstOrDefault(o => o.UUID == uuid);
                if (oldObj == null) continue;
                dataSource.Remove(oldObj);
            }
            if (context.SaveChanges() > 0) return true;
            return true;
        }

        /// <summary>
        /// 通过一个新的对象来更新旧对象,
        /// 具体做法是将新对象中的属性的
        /// 值全部复制到旧对象中更新
        /// </summary>
        /// <param name="newobj"></param>
        /// <returns></returns>
        public static TEntity Update(TEntity newobj)
        {
            TreeviewContext context = new TreeviewContext();
            var oldObj = context.GetDatasource<TEntity>().FirstOrDefault(o => o.UUID == newobj.UUID);
            if (oldObj == null) throw new Exception("未找到需要更新的元素");
            Common.UpdateObj(ref oldObj, newobj);
            if (context.SaveChanges() > 0) return newobj;
            return null;
        }

        /// <summary>
        /// 查看元素是否已经存在,默认通过UUID
        /// 来判断元素是否有重复
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool Exists(TEntity entity)
        {
            TreeviewContext context = new TreeviewContext();
            return context.GetDatasource<TEntity>().Any(e => e.UUID == entity.UUID);
        }

        /// <summary>
        /// 检查数据仓储中是否存在某个类型的对象
        /// </summary>
        /// <returns></returns>
        public static bool HasData()
        {
            TreeviewContext context = new TreeviewContext();
            return context.GetDatasource<TEntity>().Any();
        }

        /// <summary>
        /// 获取数据仓储中某个类型的对象的总条目数量
        /// </summary>
        /// <returns></returns>
        public static int Count()
        {
            TreeviewContext context = new TreeviewContext();
            return context.GetDatasource<TEntity>().Count();
        }
    }
}