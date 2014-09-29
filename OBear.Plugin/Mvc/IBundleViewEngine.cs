// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 18:10</last-date>
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
    /// <summary>
    /// 插件视图引擎
    /// </summary>
    public interface IBundleViewEngine : IViewEngine
    {
        /// <summary>
        /// 获取 插件实例
        /// </summary>
        IBundle Bundle { get; }

        /// <summary>
        /// 获取 插件特征名称
        /// </summary>
        string SymbolicName { get; }
    }
}