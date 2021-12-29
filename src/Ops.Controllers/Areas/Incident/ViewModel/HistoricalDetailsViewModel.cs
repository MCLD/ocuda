using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class HistoricalDetailsViewModel
    {
        public string BackAction
        {
            get
            {
                return string.IsNullOrEmpty(SearchText)
                     ? nameof(HistoricalController.Index)
                     : nameof(HistoricalController.Search);
            }
        }

        public HistoricalIncident HistoricalIncident { get; set; }
        public int Page { get; set; }
        public string SearchText { get; set; }
    }
}
