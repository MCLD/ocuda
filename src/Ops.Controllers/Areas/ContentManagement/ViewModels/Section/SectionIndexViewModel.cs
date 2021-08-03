using System.Collections.Generic;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
{
    public class SectionIndexViewModel
    {
        public bool IsSiteManager { get; set; }
        public ICollection<Models.Entities.Section> UserSections { get; set; }
    }
}