using System.ComponentModel;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Volunteer
{
    public class DetailsViewModel
    {
        public int FormId { get; set; }

        [DisplayName("Volunteer Type")]
        public int FormTypeId { get; set; }

        [DisplayName("Header Segment")]
        public int HeaderSegmentId { get; set; }

        public string HeaderSegmentName { get; set; }

        public bool IsDisabled { get; set; }

        [DisplayName("Volunteer Type")]
        public string TypeName { get; set; }
    }
}