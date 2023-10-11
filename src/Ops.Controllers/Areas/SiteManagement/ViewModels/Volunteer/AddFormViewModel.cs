using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Volunteer
{
    public class AddFormViewModel
    {
        public int FormId { get; set; }

        public int HeaderSegmentId { get; set; }

        [DisplayName("Volunteer Type")]
        public string TypeName { get; set; }

        public IEnumerable<SelectListItem> VolunteerFormsSelectList { get; set; }
    }
}