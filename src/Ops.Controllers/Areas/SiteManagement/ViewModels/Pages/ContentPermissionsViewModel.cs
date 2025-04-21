using System.Collections.Generic;
using System.ComponentModel;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Pages
{
    public class ContentPermissionsViewModel
    {
        [DisplayName("Page Name")]
        public string HeaderName { get; set; }

        [DisplayName("Stub")]
        public string HeaderStub { get; set; }

        [DisplayName("Type")]
        public PageType HeaderType { get; set; }

        public int HeaderId { get; set; }

        public IDictionary<int, string> AvailableGroups { get; set; }
        public IDictionary<int, string> AssignedGroups { get; set; }
    }
}
