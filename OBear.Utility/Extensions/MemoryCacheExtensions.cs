﻿// -----------------------------------------------------------------------
//  <copyright file="MemoryCacheExtensions.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-08-29 15:51</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;


namespace OBear.Utility.Extensions
{
    /// <summary>
    /// 内存缓存扩展操作类
    /// </summary>
    public static class MemoryCacheExtensions
    {
        /// <summary>
        /// 获取指定键值的强类型数据
        /// </summary>
        /// <typeparam name="T">强类型</typeparam>
        /// <param name="cache"></param>
        /// <param name="key">缓存键值</param>
        /// <param name="regionName">区域名称，默认不支持</param>
        /// <returns></returns>
        public static T Get<T>(this MemoryCache cache, string key, string regionName = null)
        {
            object value = cache.Get(key, regionName);
            if (value is T)
            {
                return (T)value;
            }
            return default(T);
        }
    }
}