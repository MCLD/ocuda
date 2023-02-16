using System.Collections.Generic;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location
{
    public class LocationVolunteerFormViewModel
    {
        private string _TypeDisplayName;
        public string AlertWarning { get; set; }
        public List<LocationVolunteerMappingViewModel> FormMappings { get; set; }
        public bool IsDisabled { get; set; }
        public int TypeId { get; set; }

        public string TypeName
        {
            get { return _TypeDisplayName; }
            set { _TypeDisplayName = $"{value} Volunteer Coordinators"; }
        }
    }
}