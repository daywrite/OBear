using OBear.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OBear.Hosting.Web.Controllers
{
    public class HomeController : Controller
    {
        public ITestContract Contract { get; set; }

        // GET: Default
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Data()
        {
            string value = Contract.GetString();
            return Content("宿主中从业务契约（IoC映射）中获取的数据：" + value);
        }

        public interface ITestContract : IDependency
        {
            string GetString();
        }


        public class TestService : ITestContract
        {
            public string GetString()
            {
                return "这是从 TestService 中获取到的字符串";
            }
        }
    }
}