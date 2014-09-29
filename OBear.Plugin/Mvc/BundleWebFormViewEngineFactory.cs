// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 19:13</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UIShell.OSGi;


namespace OBear.Plugin.Mvc
{
    public class BundleWebFormViewEngineFactory : IBundleViewEngineFactory
    {
        public IBundleViewEngine CreateViewEngine(IBundle bundle)
        {
            return new BundleWebFormViewEngine(bundle);
        }
    }
}