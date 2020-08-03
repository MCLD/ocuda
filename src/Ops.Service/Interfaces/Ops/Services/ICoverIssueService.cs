using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ICoverIssueService
    {
        Task<DataWithCount<ICollection<CoverIssueHeader>>> GetPaginatedHeaderListAsync(
            CoverIssueFilter filter);

        Task<CoverIssueHeader> GetHeaderByIdAsync(int id);
        Task<ICollection<CoverIssueDetail>> GetDetailsByHeaderIdAsync(int headerId);
        Task AddCoverIssueAsync(int bibId);
        Task ResolveCoverIssueAsnyc(int headerId);
        Task<CoverIssueHeader> GetHeaderByBibIdAsync(int bibId);
    }
}
