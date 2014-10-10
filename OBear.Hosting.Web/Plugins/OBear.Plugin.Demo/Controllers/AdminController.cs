using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

//using OBear.Core.Context;
using OBear.Core.Data.Extensions;
//using OBear.Core.Logging;
using OBear.Plugin.Demo.Dtos;
using OBear.Utility;
using OBear.Utility.Data;
using OBear.Plugin.Demo.Contracts;
using OBear.Utility.Extensions;
using OBear.Utility.Filter;
using OBear.Web.Mvc.Binders;
using OBear.Plugin.Demo.Models;
using OBear.Web.Mvc.Security;
using OBear.Web.UI;

namespace OBear.Plugin.Demo.Controllers
{
    public class AdminController : Controller
    {
        private readonly IDemoContract _demoContract;

        public AdminController(IDemoContract demoContract)
        {
            _demoContract = demoContract;
        }

        #region Ajax功能

        #region 获取数据

        [AjaxOnly]
        public async Task<ActionResult> GetGridData()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<DemoEntity, bool>> predicate = FilterHelper.GetExpression<DemoEntity>(request.FilterGroup);
            GridData<object> data = await Task.Run(() =>
            {
                int total;
                var rows = _demoContract.DemoEntities.Where<DemoEntity, int>(predicate, request.PageCondition, out total)
                    .Select(m => new { m.Id, m.Name, m.Remark, m.CreatedTime }).ToList();
                return new GridData<object>(rows, total);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 验证数据

        #endregion

        #region 功能方法

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Add([ModelBinder(typeof(JsonBinder<DemoEntityDto>))] List<DemoEntityDto> models)
        {
            models.CheckNotNull("models");
            OperationResult result = new OperationResult(OperationResultType.Success);
            List<string> names = new List<string>();
            foreach (DemoEntityDto dto in models)
            {
                result = await _demoContract.AddDemoEntityAsync(dto);
                if (result.ResultType.IsError())
                {
                    break;
                }
                names.Add(dto.Name);
            }
            string msg = result.Message ?? result.ResultType.ToDescription();
            AjaxResultType msgType = result.ResultType.ToAjaxResultType();
            if (msgType != AjaxResultType.Error)
            {
                msg = "示例实体“{0}”添加成功。".FormatWith(names.ExpandAndToString());
            }
            return Json(new AjaxResult(msg, msgType));
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Update([ModelBinder(typeof(JsonBinder<DemoEntityDto>))] List<DemoEntityDto> models)
        {
            models.CheckNotNull("models");
            OperationResult result = new OperationResult(OperationResultType.Success);
            List<string> names = new List<string>();
            foreach (DemoEntityDto dto in models)
            {
                result = await _demoContract.UpdateDemoEntityAsync(dto);
                if (result.ResultType.IsError())
                {
                    break;
                }
                names.Add(dto.Name);
            }
            string msg = result.Message ?? result.ResultType.ToDescription();
            AjaxResultType msgType = result.ResultType.ToAjaxResultType();
            if (msgType != AjaxResultType.Error)
            {
                msg = "示例实体“{0}”更新成功。".FormatWith(names.ExpandAndToString());
            }
            return Json(new AjaxResult(msg, msgType));
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Delete([ModelBinder(typeof(JsonBinder<int>))] List<int> ids)
        {
            ids.CheckNotNull("ids");
            OperationResult result = new OperationResult(OperationResultType.Success);
            List<string> names = new List<string>();
            foreach (int id in ids)
            {
                result = await _demoContract.DeleteDemoEntityAsync(id);
                if (result.ResultType.IsError())
                {
                    break;
                }
                names.Add(result.Data.ToString());
            }
            string msg = result.Message ?? result.ResultType.ToDescription();
            AjaxResultType msgType = result.ResultType.ToAjaxResultType();
            if (msgType != AjaxResultType.Error)
            {
                msg = "示例实体“{0}”删除成功。".FormatWith(names.ExpandAndToString());
            }
            return Json(new AjaxResult(msg, msgType));
        }

        #endregion

        #endregion

        #region 视图功能

        public ActionResult Index()
        {
            return View();
        }

        #endregion
    }
}