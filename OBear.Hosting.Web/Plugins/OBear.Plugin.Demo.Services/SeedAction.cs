using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OBear.Core.Data.Entity.Migrations;
using OBear.Plugin.Demo.Models;


namespace OBear.Plugin.Demo.Services
{
    public class SeedAction : ISeedAction
    {
        /// <summary>
        /// 获取 操作排序，数值越小越先执行
        /// </summary>
        public int Order { get { return 1; } }

        /// <summary>
        /// 定义种子数据初始化过程
        /// </summary>
        /// <param name="context">数据上下文</param>
        public void Action(DbContext context)
        {
            DemoEntity entity = new DemoEntity()
            {
                Name = "示例实体",
                DemoDetails = new List<DemoDetail>()
                {
                    new DemoDetail(){Content = "示例子项一"},
                    new DemoDetail(){Content = "示例子项二"}
                }
            };
            context.Set<DemoEntity>().Add(entity);
        }
    }
}
