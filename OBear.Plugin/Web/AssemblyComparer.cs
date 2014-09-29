// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-13 17:43</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace OBear.Plugin.Web
{
    /// <summary>
    /// 按照逆序对Assembly进行排序。
    /// </summary>
    public class AssemblyComparer : IComparer<Assembly>
    {
        public int Compare(Assembly x, Assembly y)
        {
            AssemblyName name = x.GetName();
            AssemblyName name2 = y.GetName();
            if (name.Name.Equals(name2.Name))
            {
                return -name.Version.CompareTo(name2.Version);
            }
            return -String.Compare(name.Name, name2.Name, StringComparison.Ordinal);
        }
    }
}