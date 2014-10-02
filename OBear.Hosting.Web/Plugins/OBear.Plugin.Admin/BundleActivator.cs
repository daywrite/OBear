using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using UIShell.OSGi;
using UIShell.PageFlowService;
using OBear.Plugin.Mvc;
namespace OBear.Plugin.Admin
{
    public class BundleActivator : IBundleActivator
    {
        public static IBundle Bundle { get; private set; }

        public static ServiceTracker<IPageFlowService> PageFlowServiceTracker { get; set; }

    
        public void Start(IBundleContext context)
        {
            Bundle = context.Bundle;
            PageFlowServiceTracker = new ServiceTracker<IPageFlowService>(context);
            DefaultConfig.RegisterBundleNamespaces(context.Bundle.SymbolicName, GetType().Assembly);
        }

        public void Stop(IBundleContext context)
        { }
    }
}