using System.Collections.Generic;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class PermissionsViewModel
    {
        public PermissionsViewModel()
        {
            AssignedGroups = new Dictionary<int, string>();
            AvailableGroups = new Dictionary<int, string>();
        }

        public IDictionary<int, string> AssignedGroups { get; }

        public IDictionary<int, string> AvailableGroups { get; }

        public int LocationId { get; set; }

        public string LocationName { get; set; }
    }
}