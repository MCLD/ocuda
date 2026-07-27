using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Models;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IEmployeeCardReportingService
    {
        Task<bool> AnyAsync();

        Task<IEnumerable<DisplayReport>> GetReportAsync(ReportCriteria criteria);

        Task<DataWithCount<IDictionary<DateTime, int?>>> GetStatsAsync(BaseFilter<string> filter);

        Task RunPendingReportsAsync();
    }
}
