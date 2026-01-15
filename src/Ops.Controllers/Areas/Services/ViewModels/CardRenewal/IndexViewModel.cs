using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Services.ViewModels.CardRenewal
{
    public class IndexViewModel : PaginateModel
    {
        public ICollection<CardRenewalRequest> CardRequests { get; set; }
        public bool APIConfigured { get; set; }
        public bool IsProcessed { get; set; }
        public int PendingCount { get; set; }
        public int ProcessedCount { get; set; }
    }
}
