using System.Collections.Generic;
using System.ComponentModel;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
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

        [DisplayName("Section")]
        public string Name { get; set; }

        [DisplayName("Stub")]
        public string Stub { get; set; }
    }
}