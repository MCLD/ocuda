using System.Collections.Generic;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class IndexViewModel : IncidentViewModelBase
    {
        public IndexViewModel()
        {
            Heading = "Incident Reports";
        }
        public ICollection<Models.Entities.Incident> Incidents { get; set; }
        public IDictionary<int, string> Locations { get; set; }
        public int Page { get; set; }
        public bool ViewingAll { get; set; }
    }
}
