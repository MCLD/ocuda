using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class HistoricalIndexViewModel : PaginateModel
    {
        public ICollection<HistoricalIncident> HistoricalIncidents { get; set; }
        public string SearchText { get; set; }
    }
}
