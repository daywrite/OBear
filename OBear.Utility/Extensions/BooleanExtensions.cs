﻿// -----------------------------------------------------------------------
//  <copyright file="AbstractBuilder.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014:07:04 8:01</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OBear.Utility.Extensions
{
    /// <summary>
    /// 布尔值<see cref="Boolean"/>类型的扩展辅助操作类
    /// </summary>
    public static class BooleanExtensions
    {
        /// <summary>
        /// 把布尔值转换为小写字符串
        /// </summary>
        public static string ToLower(this bool value)
        {
            return value.ToString().ToLower();
        }
    }
}