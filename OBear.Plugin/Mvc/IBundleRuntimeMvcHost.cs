// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 18:18</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using UIShell.OSGi;


namespace OBear.Plugin.Mvc
{
    /// <summary>
    /// MVC的插件运行时宿主
    /// </summary>
    public interface IBundleRuntimeMvcHost
    {
        /// <summary>
        /// 获取 插件运行时对象
        /// </summary>
        BundleRuntime BundleRuntime { get; }

        /// <summary>
        /// 获取 引用程序集集合
        /// </summary>
        ICollection<Assembly> TopLevelReferencedAssemblies { get; }

        /// <summary>
        /// 添加指定插件的引用程序集
        /// </summary>
        /// <param name="bundleSymbolicName"></param>
        /// <returns></returns>
        ICollection<Assembly> AddReferencedAssemblies(string bundleSymbolicName);

        /// <summary>
        /// 添加引用程序集
        /// </summary>
        /// <param name="assembly"></param>
        void AddReferencedAssembly(Assembly assembly);

        /// <summary>
        /// 移除引用程序集
        /// </summary>
        /// <param name="assembly"></param>
        void RemoveReferencedAssemlby(Assembly assembly);

        /// <summary>
        /// 批量移除引用程序集
        /// </summary>
        /// <param name="assemblies"></param>
        void RemoveReferencedAssemblies(ICollection<Assembly> assemblies);

        /// <summary>
        /// Restart current web application.
        /// </summary>
        void RestartAppDomain();

        void RestartAppDomain(WriteHtmlContentAfterReboot writeHtmlContent);
    }


    public delegate void WriteHtmlContentAfterReboot(StreamWriter sw);
}