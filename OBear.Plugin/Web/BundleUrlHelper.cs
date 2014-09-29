// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 17:49</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using UIShell.OSGi;
using UIShell.OSGi.Utility;


namespace OBear.Plugin.Web
{
    /// <summary>
    /// ASP.NET插件路径辅助类。
    /// </summary>
    /// <example>
    /// <code>
    /// 以下示例用于创建一个连接到某个插件的Default.aspx页面。
    /// <![CDATA[
    /// <a href="<% =BundleUrlHelper.Content(SimpleBundle.Activator.Bundle, "~/Default.aspx")%>">Default.aspx in SimpleBundle.</a>
    /// ]]>
    /// </code>
    /// </example>
    public static class BundleUrlHelper
    {
        private static string _webRootPath;

        static BundleUrlHelper()
        {
            _webRootPath = HttpContext.Current.Server.MapPath("~");
        }

        /// <summary>
        /// 获取相对与该插件的页面文件的真实路径。
        /// </summary>
        /// <param name="bundle">插件实例。</param>
        /// <param name="url">URL路径，例如：~/Default.aspx则表示在插件下的Default.aspx页面。</param>
        /// <returns>返回真实路径。</returns>
        public static string Content(IBundle bundle, string url)
        {
            AssertUtility.ArgumentNotNull(bundle, "bundle");
            AssertUtility.ArgumentHasText(url, "url");
            while (url.StartsWith("~") || url.StartsWith("/") || url.StartsWith("\\"))
            {
                url = url.Remove(0, 1);
            }
            return Path.Combine(bundle.Location.Replace(_webRootPath, "~\\"), url);
        }

        /// <summary>
        /// 获取相对与该插件的页面文件的真实路径。
        /// </summary>
        /// <param name="symbolicName">插件特征名称。</param>
        /// <param name="url">URL路径，例如：~/Default.aspx则表示在插件下的Default.aspx页面。</param>
        /// <returns>返回真实路径。</returns>
        public static string Content(string symbolicName, string url)
        {
            IBundle bundleBySymbolicName = BundleRuntime.Instance.Framework.Bundles.GetBundleBySymbolicName(symbolicName);
            if (bundleBySymbolicName == null)
            {
                throw new Exception(string.Format(Messages.BundleNotExist, symbolicName));
            }
            return Content(bundleBySymbolicName, url);
        }
    }
}