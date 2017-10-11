using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Treeview.Entity;

namespace Treeview.Data
{
    /// <summary>
    /// 数据仓储接口,定义数据库操作规范
    /// </summary>
    interface IRepository<T> where T : EntityBase
    {
        /// <summary>
        /// 获取列表所有数据.
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> FindAll();

        /// <summary>
        /// 获取分页数据,第一页的序号是1不是0
        /// </summary>
        /// <param name="pindex">页码</param>
        /// <param name="psize">每页大小第一页是1</param>
        /// <param name="keyword"></param>
        /// <param name="sortColumn"></param>
        /// <param name="orderby">排序列</param>
        /// <param name="count"></param>
        /// <returns></returns>
        IEnumerable<T> FindPage(int pindex, int psize, string keyword, string sortColumn, SortOrder @orderby, out int count);

        /// <summary>
        /// 通过主键ID查找一个实例对象
        /// </summary>
        /// <param name="uuid">对象的主键UUID</param>
        /// <returns></returns>
        T Find(Guid uuid);

        /// <summary>
        /// 获取总数
        /// </summary>
        /// <returns></returns>
        int Count();

        /// <summary>
        /// 通过属性名key以及属性对应的值value来搜索对象
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IEnumerable<T> FindBy(string propertyName, object value);

        /// <summary>
        /// 更新并且返回一个对象
        /// </summary>
        /// <param name="newEntity"></param>
        /// <returns></returns>
        T Update(T newEntity);

        bool Exists(T entity);

        /// <summary>
        /// 新增并且返回新的对象
        /// </summary>
        /// <param name="newEntity"></param>
        /// <returns></returns>
        T Add(T newEntity);

        /// <summary>
        /// 通过对象的UUID来删除一个对象,并且返回是否删除成功
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns>
        /// 如果删除数据成功返回true,失败返回false
        /// </returns>
        bool Delete(Guid uuid);

        /// <summary>
        /// 删除一组元素
        /// </summary>
        /// <param name="uuids"></param>
        /// <returns></returns>
        bool Delete(List<Guid> uuids);

        /// <summary>
        /// 该表中是否有任何一条数据
        /// </summary>
        /// <returns></returns>
        bool HasData();
    }
}
