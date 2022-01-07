using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class AddViewModel : IncidentViewModelBase
    {
        public AddViewModel()
        {
            Heading = "Add Incident Report";
        }

        public Models.Entities.Incident Incident { get; set; }
        public IEnumerable<SelectListItem> IncidentTypes { get; set; }
        public IEnumerable<SelectListItem> Locations { get; set; }
    }
}
