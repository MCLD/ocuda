using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Service.Models.Navigation;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Navigations
{
    public class IndexViewModel
    {
        public IndexViewModel()
        {
            Navigations = new List<Navigation>();
            OpenRoles = new List<SelectListItem>();
        }

        public bool HasEditPermission { get; set; }
        public Navigation Navigation { get; set; }
        public IFormFile NavigationJsonImport { get; set; }
        public NavigationRoles NavigationRoles { get; set; }
        public ICollection<Navigation> Navigations { get; }
        public ICollection<SelectListItem> OpenRoles { get; }
        public string Role { get; set; }

        public static string NavigationRole(int navigationId, NavigationRoles roles)
        {
            if (roles == null)
            {
                return string.Empty;
            }

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