// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 18:52</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace OBear.Plugin.Mvc
{
    public class DefaultConfig
    {
        private static readonly IDictionary<string, HashSet<string>> NamespacesDictionary;
        
        public const string HostingName = "Web_Hosting";

        static DefaultConfig()
        {
            NamespacesDictionary = new Dictionary<string, HashSet<string>>();
        }

        /// <summary>
        /// 注册宿主程序集的Controller命名空间
        /// </summary>
        /// <param name="assembly">宿主程序集</param>
        public static void RegisterHostingNamespaces(Assembly assembly)
        {
            RegisterAssemblyNamespaces(HostingName, assembly);
        }

        /// <summary>
        /// 注册插件程序集的Controller命名空间
        /// </summary>
        /// <param name="bundleSymbolicName">插件特征名称</param>
        /// <param name="assembly">插件程序集</param>
        public static void RegisterBundleNamespaces(string bundleSymbolicName, Assembly assembly)
        {
            RegisterAssemblyNamespaces(bundleSymbolicName, assembly);
        }

        /// <summary>
        /// 获取宿主的Controller命名空间集合
        /// </summary>
        public static IEnumerable<string> GetHostingNamespaces()
        {
            return GetBundleNamespaces(HostingName);
        }

        /// <summary>
        /// 获取插件的Controller命名空间集合
        /// </summary>
        public static IEnumerable<string> GetBundleNamespaces(string bundleSymbolicName)
        {
            HashSet<string> namespaces;
            if (NamespacesDictionary.TryGetValue(bundleSymbolicName, out namespaces))
            {
                return namespaces;
            }
            return new HashSet<string>();
        }

        private static void RegisterAssemblyNamespaces(string name, Assembly assembly)
        {
            HashSet<string> namespaces;
            if (!NamespacesDictionary.TryGetValue(name, out namespaces))
            {
                namespaces = new HashSet<string>();
                NamespacesDictionary.Add(name, namespaces);
            }
            Type[] types = assembly.GetExportedTypes();
            foreach (Type type in types)
            {
                if (!type.IsAbstract && typeof(IController).IsAssignableFrom(type) && type.Namespace != null)
                {
                    namespaces.Add(type.Namespace);
                }
            }
        }
    }
}