// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 19:12</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

using UIShell.OSGi;


namespace OBear.Plugin.Mvc
{
    public class BundleRazorViewEngine : RazorViewEngine, IBundleViewEngine
    {
        public BundleRazorViewEngine(IBundle bundle)
        {
            Bundle = bundle;
            string bundleRelativePath = MvcPathUtility.MapPathReverse(bundle.Location);
            AreaViewLocationFormats = MvcPathUtility.RedirectToBundlePath(AreaViewLocationFormats, bundleRelativePath).ToArray();
            AreaMasterLocationFormats = MvcPathUtility.RedirectToBundlePath(AreaMasterLocationFormats, bundleRelativePath).ToArray();
            AreaPartialViewLocationFormats = MvcPathUtility.RedirectToBundlePath(AreaPartialViewLocationFormats, bundleRelativePath).ToArray();
            ViewLocationFormats = MvcPathUtility.RedirectToBundlePath(ViewLocationFormats, bundleRelativePath).ToArray();
            MasterLocationFormats = MvcPathUtility.RedirectToBundlePath(MasterLocationFormats, bundleRelativePath).ToArray();
            PartialViewLocationFormats = MvcPathUtility.RedirectToBundlePath(PartialViewLocationFormats, bundleRelativePath).ToArray();
        }

        public IBundle Bundle { get; private set; }

        public string SymbolicName
        {
            get { return Bundle.SymbolicName; }
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            RouteValueDictionary tokens = controllerContext.RouteData.DataTokens;
            RouteValueDictionary values = controllerContext.RouteData.Values;
            if (!tokens.ContainsKey("area") && values.ContainsKey("plugin"))
            {
                tokens["area"] = values["plugin"];
            }
            
            object bundleSymbolicName = controllerContext.GetBundleSymbolicName();
            if (bundleSymbolicName != null && SymbolicName.Equals(bundleSymbolicName))
            {
                return base.FindView(controllerContext, viewName, masterName, useCache);
            }
            return new ViewEngineResult(new string[0]);
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            RouteValueDictionary tokens = controllerContext.RouteData.DataTokens;
            RouteValueDictionary values = controllerContext.RouteData.Values;
            if (!tokens.ContainsKey("area") && values.ContainsKey("plugin"))
            {
                tokens["area"] = values["plugin"];
            }
            
            object bundleSymbolicName = controllerContext.GetBundleSymbolicName();
            if (bundleSymbolicName != null && SymbolicName.Equals(bundleSymbolicName))
            {
                return base.FindPartialView(controllerContext, partialViewName, useCache);
            }
            return new ViewEngineResult(new string[0]);
        }
    }
}