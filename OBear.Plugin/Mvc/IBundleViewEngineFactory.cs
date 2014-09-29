// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) OBear 2014. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014:06:28 17:41</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UIShell.OSGi;


namespace OBear.Plugin.Mvc
{
    /// <summary>
    /// 插件视图引擎工厂
    /// </summary>
    public interface IBundleViewEngineFactory
    {
        /// <summary>
        /// 创建插件视图引擎对象
        /// </summary>
        /// <param name="bundle">插件对象</param>
        /// <returns></returns>
        IBundleViewEngine CreateViewEngine(IBundle bundle);
    }
}