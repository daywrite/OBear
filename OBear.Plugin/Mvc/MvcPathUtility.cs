// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 18:43</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;


namespace OBear.Plugin.Mvc
{
    public static class MvcPathUtility
    {
        public static string RedirectToBundlePath(string locationFormat, string bundleRelativePath)
        {
            return locationFormat.Replace("~", bundleRelativePath);
        }

        public static IEnumerable<string> RedirectToBundlePath(IEnumerable<string> locationFormats, string bundleRelativePath)
        {
            return (from item in locationFormats
                select RedirectToBundlePath(item, bundleRelativePath));
        }

        public static string MapPathReverse(string fullServerPath)
        {
            return "~/" + fullServerPath.Replace(HostingEnvironment.ApplicationPhysicalPath, string.Empty).Replace("\\", "/");
        }
    }
}