using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class IncidentViewModelBase : PaginateModel
    {
        public bool CanConfigureIncidents { get; set; }
        public bool CanViewAll { get; set; }
        public string Heading { get; set; }
        public string IncidentDocumentLink { get; set; }
        public int Page { get; set; }
        public string SearchText { get; set; }
        public string SecondaryHeading { get; set; }
    }
}