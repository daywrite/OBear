// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 19:03</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Web.Mvc;
using System.Web.Routing;

using OBear.Plugin.Web;

using UIShell.OSGi;
using UIShell.OSGi.Configuration.BundleManifest;
using UIShell.OSGi.Core.Service;
using UIShell.OSGi.Loader;
using UIShell.OSGi.Utility;


namespace OBear.Plugin.Mvc
{
    public class BundleRuntimeRunner
    {
        public BundleRuntime BundleRuntime { get; private set; }

        protected virtual BundleRuntime CreateBundleRuntime()
        {
            return new BundleRuntime();
        }

        public virtual void StartBundleRuntime()
        {
            FileLogUtility.Debug("WebSite is starting.");
            AddPreDefinedRefAssemblies();
            AppDomain.CurrentDomain.SetData("SQLServerCompactEditionUnderWebHosting", true);
            BundleRuntime = CreateBundleRuntime();
            BundleRuntime.Instance.Framework.EventManager.AddBundleEventListener(new EventHandler<BundleStateChangedEventArgs>(
                BundleRuntimeHttpHostHelper.RefreshBundleTopLevelReferencedAssembliesByEvent),
                true);
            FileLogUtility.Debug("Framework is starting.");
            BundleRuntime.Start();
            ControllerBuilder.Current.SetControllerFactory(new BundleRuntimeControllerFactory());
            RegisterGlobalFilters(GlobalFilters.Filters);
            FileLogUtility.Debug("Framework is started.");
        }

        public virtual void Initialize()
        {
            ViewEngines.Engines.Add(new BundleRuntimeViewEngine(new BundleRazorViewEngineFactory()));
            ViewEngines.Engines.Add(new BundleRuntimeViewEngine(new BundleWebFormViewEngineFactory()));
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
            routes.MapRoute("Default", "{plugin}/{controller}/{action}/{id}", new
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

        protected virtual void AddPreDefinedRefAssemblies()
        {
            BundleRuntimeHttpHostHelper.AddReferencedAssembly(base.GetType().Assembly);
            BundleRuntimeHttpHostHelper.AddReferencedAssembly(typeof(BundleRuntime).Assembly);
            BundleRuntimeHttpHostHelper.AddReferencedAssembly(base.GetType().Assembly);
        }

        protected virtual void StopBundleRuntime()
        {
            FileLogUtility.Debug("Framework is stopping.");
            BundleRuntime.Stop();
            FileLogUtility.Debug("Framework is stopped.");
            FileLogUtility.Debug("WebSite is stopped.");
        }
    }
}