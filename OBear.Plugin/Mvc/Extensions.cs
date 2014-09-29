// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 18:48</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;


namespace OBear.Plugin.Mvc
{
    public static class Extensions
    {
        public static string GetBundleSymbolicName(this RequestContext requestContext)
        {
            string name = requestContext.HttpContext.GetBundleSymbolicName();
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            if (!name.ToLower().EndsWith("plugin"))
            {
                name = name + "plugin";
            }
            return BundleSymbolicNames().SingleOrDefault(m => m.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 设置 插件的标识名称的集合
        /// </summary>
        public static Func<string[]> BundleSymbolicNames { private get; set; }

        public static string GetBundleSymbolicName(this ControllerContext requestContext)
        {
            string name = requestContext.HttpContext.GetBundleSymbolicName();
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            if (!name.ToLower().EndsWith("plugin"))
            {
                name = name + "plugin";
            }
            return BundleSymbolicNames().SingleOrDefault(m => m.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetBundleSymbolicName(this HttpContextBase context)
        {
            string areaName = context.Request.RequestContext.RouteData.GetAreaName();
            if (areaName != null)
            {
                return areaName;
            }
            object obj = context.Request.RequestContext.RouteData.Values["plugin"];
            if (obj != null)
            {
                return obj.ToString();
            }
            return context.Request.QueryString["plugin"];
        }

        public static string GetAreaName(this RouteData routeData)
        {
            object obj;
            if (routeData.DataTokens.TryGetValue("area", out obj))
            {
                return obj as string;
            }
            return routeData.Route.GetAreaName();
        }

        public static string GetAreaName(this RouteBase route)
        {
            IRouteWithArea routeWithArea = route as IRouteWithArea;
            if (routeWithArea != null)
            {
                return routeWithArea.Area;
            }
            Route route2 = route as Route;
            if (route2 != null && route2.DataTokens != null)
            {
                return route2.DataTokens["area"] as string;
            }
            return null;
        }
    }
}