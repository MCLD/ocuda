using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IEmployeeCardStatsRepository
    {
        public Task<bool> AnyAsync();

        public Task<int> GetDateCountAsync(BaseFilter<string> filter);

        public Task<IDictionary<DateTime, int?>> GetDatesAsync(BaseFilter<string> filter);

        public Task<DateTime?> GetLatestDateAsync();

        public Task<IEnumerable<EmployeeCardStats>> GetReportAsync(DateTime period);

        public Task SaveStatsAsync(EmployeeCardStats stats);
    }
}
