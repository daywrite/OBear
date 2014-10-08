using OBear.Web.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;

using UIShell.OSGi;

namespace OBear.Plugin.Admin.Controllers
{
    public class BundleManagementController : Controller
    {
        public ActionResult GetGridData()
        {
            List<IBundle> bundles = BundleActivator.BundleManagementServiceTracker.DefaultOrFirstService.GetLocalBundles();
            var data = bundles.Select(m => new
            {
                m.Name,
                m.SymbolicName,
                Version = m.Version.ToString(),
                m.BundleType,
                m.StartLevel,
                State = m.State.ToString()
            }).ToList();
            return Json(new GridData<object>(data, bundles.Count), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}