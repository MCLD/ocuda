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
    }
}
