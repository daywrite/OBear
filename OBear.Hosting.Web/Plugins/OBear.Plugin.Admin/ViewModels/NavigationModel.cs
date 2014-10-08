using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using UIShell.NavigationService;
using UIShell.OSGi;

namespace OBear.Plugin.Admin.ViewModels
{
    public class NavigationModel
    {
        private readonly IBundle _bundle;

        public NavigationModel(IBundle bundle)
        {
            _bundle = bundle;
        }

        public SortedSet<NavigationNode> NavigatioNodes
        {
            get
            {
                var service = _bundle.Context.GetFirstOrDefaultService<INavigationService>();
                if (service == null)
                {
                    return new SortedSet<NavigationNode>();
                }
                return service.NavgationNodes;
            }
        }
    }
}