using System.Collections.Generic;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class ConfigurationViewModel : IncidentViewModelBase
    {
        public ConfigurationViewModel()
        {
            Heading = "Incident Report";
            SecondaryHeading = "Configuration";
        }

        public bool CanConfigureIncidents { get; set; }
        public ICollection<Models.Entities.IncidentType> IncidentTypes { get; set; }
    }
}
