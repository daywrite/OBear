using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Threading.Tasks;
using OBear.Plugin.Admin.ViewModels;
namespace OBear.Plugin.Admin.Controllers
{
    public class HomeController : Controller
    {
        private const string NavStartFlag = "后台管理.导航菜单";
        private const string LocationKey = "Location";

        #region Ajax功能

        #region 获取数据

        //获取导航菜单数据
        public ActionResult GetNavData()
        {
            List<TreeNode> data = new List<TreeNode>()
            {
                new TreeNode()
                {
                    Id = "node-系统管理",
                    Text = "系统管理",
                    Order = 98,
                    IconCls = "pic_100",
                    Children = new List<TreeNode>()
                    {
                        new TreeNode()
                        {
                            Id = "node-插件管理",
                            Text = "插件管理",
                            Order = 1,
                            IconCls = "pic_27",
                            Url = Url.Action("Index", "BundleManagement")
                        }
                    }
                },
                new TreeNode() { Id = "node-其他", Text = "其他", Order = 99, IconCls = "pic_428" }
            };
            //await Task.Run(() =>
            //{
            //    foreach (NavigationNode navigatioNode in BundleActivator.NavigationModel.NavigatioNodes.Where(m =>
            //    m.Attributes.ContainsKey(LocationKey) && m.Attributes[LocationKey].StartsWith(NavStartFlag)))
            //    {
            //        string catalogName = navigatioNode.Attributes[LocationKey].Replace(NavStartFlag + ".", "").Replace(NavStartFlag, "");
            //        if (string.IsNullOrEmpty(catalogName))
            //        {
            //            catalogName = "其他";
            //        }
            //        TreeNode catalog = GetCatalog(data, catalogName);
            //        AddNode(catalog, navigatioNode);
            //    }
            //});
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #region 视图功能

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Welcome()
        {
            return View();
        }

        #endregion
    }
}