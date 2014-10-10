using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OBear.Core;
using OBear.Core.Data;
using OBear.Plugin.Demo.Dtos;
using OBear.Plugin.Demo.Models;
using System.Linq.Expressions;

using OBear.Utility.Data;
namespace OBear.Plugin.Demo.Contracts
{
    /// <summary>
    /// 示例业务契约，需继承于IDependency接口，才能被IoC组件识别并与实现类映射
    /// </summary>
    public interface IDemoContract : IDependency
    {
        #region 示例实体业务

        /// <summary>
        /// 获取 示例实体查询数据集
        /// </summary>
        IQueryable<DemoEntity> DemoEntities { get; }

        /// <summary>
        /// 检查示例实体名称是否存在
        /// </summary>
        /// <param name="name">示例实体名称</param>
        /// <param name="checkType">数据存在性检查类型</param>
        /// <param name="id">更新的编号</param>
        /// <returns></returns>
        bool CheckDemoEntityName(string name, CheckExistsType checkType, int id = 0);

        /// <summary>
        /// 添加示例实体
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        OperationResult AddDemoEntity(DemoEntityDto dto);

        /// <summary>
        /// 修改示例实体
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        OperationResult UpdateDemoEntity(DemoEntityDto dto);

        /// <summary>
        /// 删除示例实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        OperationResult DeleteDemoEntity(int id);

//#if NET45

        /// <summary>
        /// 异步检查示例实体名称是否存在
        /// </summary>
        /// <param name="name">示例实体名称</param>
        /// <param name="checkType">数据存在性检查类型</param>
        /// <param name="id">更新的编号</param>
        /// <returns></returns>
        Task<bool> CheckDemoEntityNameAsync(string name, CheckExistsType checkType, int id = 0);

        /// <summary>
        /// 异步添加示例实体
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<OperationResult> AddDemoEntityAsync(DemoEntityDto dto);

        /// <summary>
        /// 异步修改示例实体
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<OperationResult> UpdateDemoEntityAsync(DemoEntityDto dto);

        /// <summary>
        /// 异步删除示例实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<OperationResult> DeleteDemoEntityAsync(int id); 

//#endif

        #endregion
    }
}
