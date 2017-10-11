using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Treeview.Util
{
    /// <summary>
    /// 工具类
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// 更新对象的值,将一个新的对象中的值
        /// 全部复制到旧对象中.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elder"></param>
        /// <param name="newer"></param>
        public static void UpdateObj<T>(ref T elder, T newer) where T : class
        {
            var objProperties = typeof(T).GetProperties();
            foreach (var objProperty in objProperties)
            {
                if (objProperty.GetCustomAttributes(typeof(NotMappedAttribute),true).Any())continue;
                objProperty.SetValue(elder, objProperty.GetValue(newer, null), null);
            }
        }

        /// <summary>
        /// 通过属性名获取对象该属性的值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object GetPropertyValue(object obj, string property)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(property);
            return propertyInfo.GetValue(obj, null);
        }
    }
}
