using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Treeview.Data;
using Treeview.Data.Exceptions;
using Treeview.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Treeview.Api.Controllers
{
    /// <summary>
    /// 树节点管理对应的Controller
    /// </summary>
    public class TreeviewController : ApiController
    {
        /// <summary>
        /// 树节点数据仓储
        /// </summary>
        private TreeNodeRepository _treeNodeRepo = new TreeNodeRepository();

        /// <summary>
        /// 返回树形结构的树节点列表,是一个集合
        /// 转换成的Json数组,数组中的树节点为一级
        /// 树节点,一级树节点对象的Children里面会递归
        /// 存储了所有子集.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTree()
        {
            var req = Request.CreateResponse(HttpStatusCode.OK);
            try
            {
                List<TreeNode> treeNodeList;
                treeNodeList = _treeNodeRepo.FindTree(new TreeNode() { ParentUUID = Guid.Empty, UUID = Guid.Empty }).ToList();
                string mList = JsonConvert.SerializeObject(treeNodeList, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                req.Content = new StringContent(mList, System.Text.Encoding.UTF8, "application/json");
                return req;
            }
            catch (Exception ex)
            {
                req.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "application/json");
                return req;
            }
        }

        /// <summary>
        /// 通过一个树节点的UUID,从数据仓储中检索一个树节点实体
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Get(string uuid)
        {
            var req = Request.CreateResponse(HttpStatusCode.OK);
            try
            {
                Guid muuid = new Guid(uuid);
                var treeNode = _treeNodeRepo.Find(muuid);
                string treeNodeJson = JsonConvert.SerializeObject(treeNode, Formatting.None);
                req.Content = new StringContent(treeNodeJson, System.Text.Encoding.UTF8, "application/json");
                return req;
            }
            catch (Exception ex)
            {
                req.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "application/json");
                return req;
            }
        }

        /// <summary>
        /// 更新对象,如果成功返回该对象
        /// 如果失败,返回错误信息.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage Update([FromBody] JToken value)
        {
            var req = Request.CreateResponse(HttpStatusCode.OK);
            try
            {
                string authorityAction = string.Empty;
                try
                {
                    authorityAction = value["AuthorityActionArray"].ToString();
                }
                catch
                {
                    authorityAction = string.Empty;
                }
                TreeNode treeNode = new TreeNode
                {
                    UUID = new Guid(value.Value<string>("UUID")),
                    Name = value.Value<string>("Name"),
                    Description = value.Value<string>("Description"),
                    ParentUUID = new Guid(value.Value<string>("ParentUUID")),
                    SortSeed = value.Value<int>("SortSeed"),
                    IconClassName = value.Value<string>("IconClassName")
                };
                if (string.IsNullOrEmpty(treeNode.Name))
                {
                    req.Content = new StringContent("树节点名字不能为空.", System.Text.Encoding.UTF8, "application/json");
                    return req;
                }

                string treeNodeStr = JsonConvert.SerializeObject(_treeNodeRepo.Update(treeNode), Formatting.None);
                req.Content = new StringContent(treeNodeStr, System.Text.Encoding.UTF8, "application/json");
                return req;
            }
            catch (Exception ex)
            {
                req.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "application/json");
                return req;
            }
        }

        /// <summary>
        /// 通过传入一个UUID来删除对象.
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        [HttpDelete]
        public HttpResponseMessage Delete(string uuid)
        {
            var req = Request.CreateResponse(HttpStatusCode.OK);
            try
            {
                Guid userUuid = new Guid(uuid);
                bool flag = _treeNodeRepo.Delete(userUuid);
                req.Content = new StringContent(flag ? "true" : "false", System.Text.Encoding.UTF8, "application/json");
                return req;
            }
            catch (Exception ex)
            {
                req.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "application/json");
                return req;
            }
        }

        /// <summary>
        /// 通过一个UUID数组来批量删除一组树节点
        /// 不能传入一棵树结构的数组.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage DeleteMany([FromBody] JToken value)
        {
            var req = Request.CreateResponse(HttpStatusCode.OK);
            try
            {
                List<Guid> uuids = new List<Guid>();
                if (value != null && value.Any())
                {
                    foreach (var uuidJson in value)
                    {
                        var uuid = new Guid(uuidJson.ToString());
                        uuids.Add(new Guid(uuid.ToString()));
                    }
                }
                bool flag = _treeNodeRepo.Delete(uuids);
                req.Content = new StringContent(flag ? "true" : "false", System.Text.Encoding.UTF8, "application/json");
                return req;
            }
            catch (Exception ex)
            {
                req.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "application/json");
                return req;
            }
        }

        /// <summary>
        /// 新建对象,如果成功则返回该对象
        /// 如果失败,则返回错误信息
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPut]
        public HttpResponseMessage Create([FromBody] JToken value)
        {
            var req = Request.CreateResponse(HttpStatusCode.OK);
            try
            {
                Guid parentUUID;
                int sortSeed = 0;
                try
                {
                    int.TryParse(value.Value<string>("SortSeed"), out sortSeed);
                    parentUUID = new Guid(value.Value<string>("ParentUUID"));
                }
                catch
                {
                    parentUUID = Guid.Empty;
                }
                TreeNode treeNode = new TreeNode
                {
                    Name = value.Value<string>("Name"),
                    Description = value.Value<string>("Description"),
                    ParentUUID = parentUUID,
                    SortSeed = sortSeed,
                    IconClassName = value.Value<string>("IconClassName")
                };
                if (string.IsNullOrEmpty(treeNode.Name))
                {
                    req.Content = new StringContent("树节点名字不能为空.", System.Text.Encoding.UTF8, "application/json");
                    return req;
                }

                string treeNodeStr = JsonConvert.SerializeObject(_treeNodeRepo.Add(treeNode), Formatting.None);
                req.Content = new StringContent(treeNodeStr, System.Text.Encoding.UTF8, "application/json");
                return req;
            }
            catch (EntityExistsException)
            {
                req.Content = new StringContent("树节点名字已存在", System.Text.Encoding.UTF8, "application/json");
                return req;
            }
            catch (Exception ex)
            {
                req.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "application/json");
                return req;
            }
        }
    }
}