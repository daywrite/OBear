using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

//using OBear.Core.Context;
using OBear.Core.Data.Entity;
//using OBear.Core.Logging;
using OBear.Plugin.Mvc;
using OBear.Utility.Extensions;
using OBear.Web.Mvc.Filters;

using UIShell.NavigationService;
using System.Reflection;

namespace OBear.Hosting.Web
{
    public class MvcApplication : BundleRuntimeMvcApplication
    {
        //protected void Application_Start()
        //{
        //AreaRegistration.RegisterAllAreas();
        //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        //RouteConfig.RegisterRoutes(RouteTable.Routes);
        //BundleConfig.RegisterBundles(BundleTable.Bundles);
        //}
        //菜单项
        public static IEnumerable<NavigationNode> NavigationNodes { get; set; }

        protected override void Application_Start(object sender, EventArgs e)
        {
            Assembly hostAssembly = Assembly.GetExecutingAssembly();
            //
            Assembly[] hostDependencyAssemblies = hostAssembly.GetReferencedAssemblies()
                .Where(m => m.Name.Contains("OBear")).Select(Assembly.Load).ToArray();
            IoCBundleActivator.HostingAssemblyFunc = () => new[] { hostAssembly }.Union(hostDependencyAssemblies).ToArray();
            //
            DefaultConfig.RegisterHostingNamespaces(hostAssembly);
            base.Application_Start(sender, e);
            //数据库生成
            DatabaseInitializer.Initialize();
            //给菜单赋值
            NavigationNodes = NavigationInitialize();
        }

        //通过插件获取到菜单项
        private IEnumerable<NavigationNode> NavigationInitialize()
        {
            INavigationService service = BundleRuntime.GetFirstOrDefaultService<INavigationService>();
            if (service == null)
            {
                return new List<NavigationNode>();
            }
            return service.NavgationNodes;
        }

        public override void RegisterResourceBundles()
        {
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
