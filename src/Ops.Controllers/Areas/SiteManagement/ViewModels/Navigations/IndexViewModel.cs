using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Service.Models.Navigation;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Navigations
{
    public class IndexViewModel
    {
        public ICollection<Navigation> Navigations { get; set; }
        public NavigationRoles NavigationRoles { get; set; }
        public List<SelectListItem> OpenRoles { get; set; }

        public Navigation Navigation { get; set; }
        public string Role { get; set; }

        public static string NavigationRole(int navigationId, NavigationRoles roles)
        {
            if (navigationId == roles.Top)
            {
                return "Top";
            }
            else if (navigationId == roles.Middle)
            {
                return "Middle";
            }
            else if (navigationId == roles.Left)
            {
                return "Left";
            }
            else if (navigationId == roles.Footer)
            {
                return "Footer";
            }

            return string.Empty;
        }
    }
}
