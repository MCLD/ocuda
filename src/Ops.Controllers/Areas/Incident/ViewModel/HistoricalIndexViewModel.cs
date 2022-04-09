using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class HistoricalIndexViewModel : IncidentViewModelBase
    {
        public HistoricalIndexViewModel()
        {
            Heading = "Historical Incident Reports";
        }

        public ICollection<HistoricalIncident> HistoricalIncidents { get; set; }
    }
}
