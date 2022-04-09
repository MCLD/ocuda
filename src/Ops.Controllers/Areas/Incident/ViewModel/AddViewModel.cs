using System;
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

        public string AffectedJson { get; set; }
        public Models.Entities.Incident Incident { get; set; }
        public DateTime? IncidentDate { get; set; }
        public DateTime? IncidentTime { get; set; }
        public IEnumerable<SelectListItem> IncidentTypes { get; set; }
        public IEnumerable<SelectListItem> Locations { get; set; }
        public bool MultiUserAccount { get; set; }
        public string WitnessJson { get; set; }
    }
}
