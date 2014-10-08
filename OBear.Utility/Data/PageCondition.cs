﻿// -----------------------------------------------------------------------
//  <copyright file="PageCondition.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-08-04 0:08</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace OBear.Utility.Data
{
    /// <summary>
    /// 分页查询条件信息
    /// </summary>
    public class PageCondition
    {
        /// <summary>
        /// 初始化一个 默认参数（第1页，每页20，排序条件为空）的分页查询条件信息类 的新实例
        /// </summary>
        public PageCondition()
        {
            PageIndex = 1;
            PageSize = 20;
            SortConditions = new SortCondition[] { };
        }

        /// <summary>
        /// 初始化一个 指定页索引与页大小的分页查询条件信息类 的新实例
        /// </summary>
        /// <param name="pageIndex"> 页索引 </param>
        /// <param name="pageSize"> 页大小 </param>
        public PageCondition(int pageIndex, int pageSize)
            : this()
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        /// <summary>
        /// 获取或设置 页索引
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 获取或设置 页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 获取或设置 排序条件组
        /// </summary>
        public SortCondition[] SortConditions { get; set; }
    }
}