using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class HistoricalDetailsViewModel : IncidentViewModelBase
    {
        public HistoricalDetailsViewModel()
        {
            Heading = "Historical Incident Report";
        }

        public bool FileExists { get; set; }

        public HistoricalIncident HistoricalIncident { get; set; }
        public int Page { get; set; }
    }
}
