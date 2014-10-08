using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using UIShell.OSGi;
using UIShell.PageFlowService;
using OBear.Plugin.Mvc;
using OBear.Plugin.Admin.Helper;
using System.Web.Optimization;
using OBear.Plugin.Admin.ViewModels;
namespace OBear.Plugin.Admin
{
    public class BundleActivator : IBundleActivator
    {
        public static IBundle Bundle { get; private set; }

        public static ServiceTracker<IPageFlowService> PageFlowServiceTracker { get; set; }

        //初始化菜单数据定义
        public static NavigationModel NavigationModel { get; private set; }

        public void Start(IBundleContext context)
        {
            Bundle = context.Bundle;
            PageFlowServiceTracker = new ServiceTracker<IPageFlowService>(context);
            DefaultConfig.RegisterBundleNamespaces(context.Bundle.SymbolicName, GetType().Assembly);

            //初始化菜单数据
            NavigationModel = new NavigationModel(context.Bundle);
            //注册CSS和JS
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        public void Stop(IBundleContext context)
        { }
    }
}