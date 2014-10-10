using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OBear.Core.Data;
using OBear.Plugin.Demo.Models;
using OBear.Utility.Data;

namespace OBear.Plugin.Demo.Models
{
    public class DemoDetail : EntityBase<Guid>
    {
        public DemoDetail()
        {
            Id = CombHelper.NewComb();
        }

        public string Content { get; set; }

        public bool IsLocked { get; set; }

        public virtual DemoEntity DemoEntity { get; set; }
    }
}
