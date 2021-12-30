using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class HistoricalDetailsViewModel
    {
        public bool FileExists { get; set; }

        public HistoricalIncident HistoricalIncident { get; set; }
        public int Page { get; set; }
        public string SearchText { get; set; }
    }
}
