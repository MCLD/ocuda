using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class HistoricalDetailsViewModel
    {
        public HistoricalIncident HistoricalIncident { get; set; }
        public int? Page { get; set; }
        public string SearchText { get; set; }
    }
}
