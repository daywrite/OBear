﻿// -----------------------------------------------------------------------
//  <copyright file="OperationResult.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>最后修改人</last-editor>
//  <last-date>2014-07-30 5:15</last-date>
// -----------------------------------------------------------------------

namespace OBear.Utility.Data
{
    /// <summary>
    /// 业务操作结果信息类，对操作结果进行封装
    /// </summary>
    public class OperationResult
    {
        #region 构造函数

        /// <summary>
        /// 初始化一个<see cref="OperationResult"/>类型的新实例
        /// </summary>
        public OperationResult(OperationResultType resultType)
        {
            ResultType = resultType;
        }

        /// <summary>
        /// 初始化一个<see cref="OperationResult"/>类型的新实例
        /// </summary>
        public OperationResult(OperationResultType resultType, string message)
            : this(resultType)
        {
            Message = message;
        }

        /// <summary>
        /// 初始化一个<see cref="OperationResult"/>类型的新实例
        /// </summary>
        public OperationResult(OperationResultType resultType, string message, object data)
            : this(resultType, message)
        {
            Data = data;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 获取或设置 操作结果类型
        /// </summary>
        public OperationResultType ResultType { get; set; }

        /// <summary>
        /// 获取或设置 操作返回信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 获取或设置 操作返回数据
        /// </summary>
        public object Data { get; set; }

        #endregion
    }
}