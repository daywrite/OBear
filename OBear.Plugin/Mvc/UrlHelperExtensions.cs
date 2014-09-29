// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 18:44</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

using UIShell.OSGi;


namespace OBear.Plugin.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string BundleContent(this UrlHelper helper, IBundle bundle, string url)
        {
            return helper.Content(BundleUrlHelper.Content(bundle, url));
        }

        public static string BundleContent(this UrlHelper helper, string symbolicName, string url)
        {
            return helper.Content(BundleUrlHelper.Content(symbolicName, url));
        }
    }
}