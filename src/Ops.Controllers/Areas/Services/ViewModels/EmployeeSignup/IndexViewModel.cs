using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Services.ViewModels.EmployeeSignup
{
    public class IndexViewModel : PaginateModel
    {
        public ICollection<EmployeeCardRequest> CardRequests { get; set; }
        public ICollection<EmployeeCardResult> CardResults { get; set; }
        public bool IsProcessed { get; set; }
        public int PendingCount { get; set; }
        public int ProcessedCount { get; set; }
    }
}
