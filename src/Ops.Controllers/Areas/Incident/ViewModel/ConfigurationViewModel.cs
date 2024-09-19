using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class ConfigurationViewModel : IncidentViewModelBase
    {
        public ConfigurationViewModel()
        {
            Heading = "Incident Report";
            SecondaryHeading = "Configuration";
            LocationPermissions = new Dictionary<int, int>();
        }

        public bool CanConfigureEmails { get; set; }
        public int EmailTemplateId { get; set; }
        public IEnumerable<Models.Entities.IncidentType> IncidentTypes { get; set; }

        public string LawEnforcementAddresses { get; set; }
        public IDictionary<int, int> LocationPermissions { get; }
        public IEnumerable<Location> Locations { get; set; }
    }
}