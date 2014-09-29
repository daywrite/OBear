using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using OBear.Plugin.Mvc;
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
        protected override void Application_Start(object sender, EventArgs e)
        {
            base.Application_Start(sender, e);
        }

        public override void RegisterResourceBundles()
        {
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
