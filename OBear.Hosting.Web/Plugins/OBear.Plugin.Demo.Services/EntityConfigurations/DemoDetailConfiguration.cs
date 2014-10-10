// -----------------------------------------------------------------------
//  <copyright file="DemoDetailConfiguration.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-21 10:11</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OBear.Core.Data.Entity;
using OBear.Plugin.Demo.Models;


namespace OBear.Plugin.Demo.EntityConfigurations
{
    public class DemoDetailConfiguration : EntityConfigurationBase<DemoDetail, Guid>
    {
        public DemoDetailConfiguration()
        {
            HasRequired(m => m.DemoEntity).WithMany(n => n.DemoDetails);
        }
    }
}