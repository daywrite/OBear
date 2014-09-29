// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 18:56</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

using UIShell.OSGi;
using UIShell.OSGi.Utility;


namespace OBear.Plugin.Mvc
{
    public class BundleUrlHelper
    {
        private static readonly string WebRootPath;

        static BundleUrlHelper()
        {
            WebRootPath = HostingEnvironment.MapPath("~");
        }

        public static string Content(IBundle bundle, string url)
        {
            AssertUtility.ArgumentNotNull(bundle, "bundle");
            AssertUtility.ArgumentHasText(url, "url");
            while (url.StartsWith("~") || url.StartsWith("/") || url.StartsWith("\\"))
            {
                url = url.Remove(0, 1);
            }
            return Path.Combine(bundle.Location.Replace(WebRootPath, "~\\"), url).Replace("\\", "/");
        }

        public static string Content(string symbolicName, string url)
        {
            IBundle bundleBySymbolicName = BundleRuntime.Instance.Framework.Bundles.GetBundleBySymbolicName(symbolicName);
            if (bundleBySymbolicName == null)
            {
                throw new Exception(string.Format("Bundle {0} doesn't exists.", symbolicName));
            }
            return Content(bundleBySymbolicName, url);
        }
    }
}