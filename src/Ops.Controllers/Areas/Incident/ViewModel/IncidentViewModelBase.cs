using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class IncidentViewModelBase : PaginateModel
    {
        public bool CanViewAll { get; set; }
        public string Heading { get; set; }
        public string SearchText { get; set; }
        public string SecondaryHeading { get; set; }
    }
}
