// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 19:08</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

using OBear.Plugin.Web;


namespace OBear.Plugin.Mvc
{
    public class BundleRuntimeMvcApplication : BundleHttpApplication
    {
        protected override void Application_Start(object sender, EventArgs e)
        {
            base.Application_Start(sender, e);
            ControllerBuilder.Current.SetControllerFactory(new BundleRuntimeControllerFactory());
            RegisterGlobalFilters(GlobalFilters.Filters);

            //清理原有的RazorViewEngine与WebformViewEngine两个引擎，以使View的查找从插件开始
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new BundleRuntimeViewEngine(new BundleRazorViewEngineFactory()));
            ViewEngines.Engines.Add(new RazorViewEngine());

            AreaRegistration.RegisterAllAreas();
            RegisterWebApiConfig();
            RegisterRoutes(RouteTable.Routes);
            RegisterResourceBundles();
        }

        public virtual void RegisterAllAreas()
        {
            AreaRegistration.RegisterAllAreas();
        }

        public virtual void RegisterWebApiConfig()
        { }

        public virtual void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = "(.*/)?favicon.ico(/.*)?" });
            
            routes.MapRoute("Plugin", "{plugin}/{controller}/{action}/{id}", new
            {
                controller = "Home",
                action = "Index",
                id = UrlParameter.Optional
            });

            routes.MapRoute("Default", "{controller}/{action}/{id}", new
            {
                controller = "Home",
                action = "Index",
                id = UrlParameter.Optional
            });
        }

        public virtual void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public virtual void RegisterResourceBundles()
        { }

        protected override void AddPreDefinedRefAssemblies()
        {
            base.AddPreDefinedRefAssemblies();
            AddReferencedAssembly(GetType().Assembly);
        }
    }
}