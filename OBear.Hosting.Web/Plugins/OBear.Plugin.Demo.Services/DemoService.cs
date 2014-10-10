// -----------------------------------------------------------------------
//  <copyright file="DemoService.cs" company="OBear开源团队">
//      Copyright (c) 2014 OBear. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2014-07-17 19:51</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using AutoMapper;

using OBear.Core;
using OBear.Core.Data;
using OBear.Plugin.Demo.Contracts;
using OBear.Plugin.Demo.Dtos;
using OBear.Plugin.Demo.Models;
using System.Linq.Expressions;

using OBear.Utility;
using OBear.Utility.Data;
using OBear.Utility.Extensions;


namespace OBear.Plugin.Demo.Services
{
    /// <summary>
    /// 示例业务实现
    /// </summary>
    public class DemoService : ServiceBase, IDemoContract
    {
        private readonly IRepository<DemoEntity, int> _demoEntityRepository;
        private readonly IRepository<DemoDetail, Guid> _demoDetailRepository;

        public DemoService(IRepository<DemoEntity, int> demoEntityRepository,
            IRepository<DemoDetail, Guid>demoDetailRepository )
            : base(demoEntityRepository.UnitOfWork)
        {
            _demoEntityRepository = demoEntityRepository;
            _demoDetailRepository = demoDetailRepository;
        }

        /// <summary>
        /// 获取 DemoDetail查询数据集
        /// </summary>
        public IQueryable<DemoDetail> DemoDetails
        {
            get { return _demoDetailRepository.Entities; }
        }

        #region Implementation of IDemoContract

        /// <summary>
        /// 获取 示例实体查询数据集
        /// </summary>
        public IQueryable<DemoEntity> DemoEntities
        {
            get { return _demoEntityRepository.Entities; }
        }

        /// <summary>
        /// 检查示例实体名称是否存在
        /// </summary>
        /// <param name="name">示例实体名称</param>
        /// <param name="checkType">数据存在性检查类型</param>
        /// <param name="id">更新的编号</param>
        /// <returns></returns>
        public bool CheckDemoEntityName(string name, CheckExistsType checkType, int id = 0)
        {
            name.CheckNotNull("name");
            var entity = _demoEntityRepository.Entities.Where(m => m.Name == name).Select(m => new { m.Id }).SingleOrDefault();
            return entity != null && (checkType == CheckExistsType.Insert || checkType == CheckExistsType.Update && entity.Id != id);
        }

        /// <summary>
        /// 添加示例实体
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public OperationResult AddDemoEntity(DemoEntityDto dto)
        {
            dto.CheckNotNull("dto" );
            if (CheckDemoEntityName(dto.Name, CheckExistsType.Insert))
            {
                return new OperationResult(OperationResultType.ValidError, "名称为“{0}”的示例实体已存在，不能重复添加。".FormatWith(dto.Name));
            }
            DemoEntity entity = Mapper.Map<DemoEntity>(dto);
            return _demoEntityRepository.Insert(entity) > 0
                ? new OperationResult(OperationResultType.Success, "示例实体“{0}”添加成功。".FormatWith(entity.Name))
                : new OperationResult(OperationResultType.NoChanged);
        }

        /// <summary>
        /// 修改示例实体
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public OperationResult UpdateDemoEntity(DemoEntityDto dto)
        {
            dto.CheckNotNull("dto" );
            if (CheckDemoEntityName(dto.Name, CheckExistsType.Update, dto.Id))
            {
                return new OperationResult(OperationResultType.ValidError, "名称为“{0}”的示例实体已存在，不能重复添加。".FormatWith(dto.Name));
            }
            DemoEntity entity = _demoEntityRepository.GetByKey(dto.Id);
            entity = Mapper.Map(dto, entity);
            return _demoEntityRepository.Update(entity) > 0
                ? new OperationResult(OperationResultType.Success, "示例实体“{0}”更新成功。".FormatWith(entity.Name))
                : new OperationResult(OperationResultType.NoChanged);
        }

        /// <summary>
        /// 删除示例实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public OperationResult DeleteDemoEntity(int id)
        {
            id.CheckGreaterThan("id", 0);
            DemoEntity entity = _demoEntityRepository.GetByKey(id);
            if (entity == null)
            {
                return new OperationResult(OperationResultType.QueryNull, "编号为“{0}”的示例实体不存在。".FormatWith(id));
            }
            return _demoEntityRepository.Delete(entity) > 0
                ? new OperationResult(OperationResultType.Success, "示例实体“{0}”删除成功。".FormatWith(entity.Name), entity.Name)
                : new OperationResult(OperationResultType.NoChanged);
        }
//#if NET45
        /// <summary>
        /// 异步检查示例实体名称是否存在
        /// </summary>
        /// <param name="name">示例实体名称</param>
        /// <param name="checkType">数据存在性检查类型</param>
        /// <param name="id">更新的编号</param>
        /// <returns></returns>
        public async Task<bool> CheckDemoEntityNameAsync(string name, CheckExistsType checkType, int id = 0)
        {
            name.CheckNotNull("name" );
            var entity = await Task.Run(() => _demoEntityRepository.Entities.Where(m => m.Name == name).Select(m => new { m.Id }).SingleOrDefault());
            return entity != null && (checkType == CheckExistsType.Insert || checkType == CheckExistsType.Update && entity.Id != id);
        }

        /// <summary>
        /// 异步添加示例实体
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<OperationResult> AddDemoEntityAsync(DemoEntityDto dto)
        {
            dto.CheckNotNull("dto");
            if (await CheckDemoEntityNameAsync(dto.Name, CheckExistsType.Insert))
            {
                return new OperationResult(OperationResultType.ValidError, "名称为“{0}”的示例实体已存在，不能重复添加。".FormatWith(dto.Name));
            }
            DemoEntity entity = Mapper.Map<DemoEntity>(dto);
            return (await _demoEntityRepository.InsertAsync(entity)) > 0
                ? new OperationResult(OperationResultType.Success, "示例实体“{0}”添加成功。".FormatWith(entity.Name))
                : new OperationResult(OperationResultType.NoChanged);
        }

        /// <summary>
        /// 异步修改示例实体
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<OperationResult> UpdateDemoEntityAsync(DemoEntityDto dto)
        {
            dto.CheckNotNull("dto");
            if (await CheckDemoEntityNameAsync(dto.Name, CheckExistsType.Update, dto.Id))
            {
                return new OperationResult(OperationResultType.ValidError, "名称为“{0}”的示例实体已存在，不能重复添加。".FormatWith(dto.Name));
            }
            DemoEntity entity = await _demoEntityRepository.GetByKeyAsync(dto.Id);
            entity = Mapper.Map(dto, entity);
            return (await _demoEntityRepository.UpdateAsync(entity)) > 0
                ? new OperationResult(OperationResultType.Success, "示例实体“{0}”更新成功。".FormatWith(entity.Name))
                : new OperationResult(OperationResultType.NoChanged);
        }

        /// <summary>
        /// 异步删除示例实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<OperationResult> DeleteDemoEntityAsync(int id)
        {
            id.CheckGreaterThan("id", 0);
            DemoEntity entity = await _demoEntityRepository.GetByKeyAsync(id);
            if (entity == null)
            {
                return new OperationResult(OperationResultType.QueryNull, "编号为“{0}”的示例实体不存在。".FormatWith(id));
            }
            return (await _demoEntityRepository.DeleteAsync(entity)) > 0
                ? new OperationResult(OperationResultType.Success, "示例实体“{0}”删除成功。".FormatWith(entity.Name), entity.Name)
                : new OperationResult(OperationResultType.NoChanged);
        }
//#endif
        #endregion

    }
}