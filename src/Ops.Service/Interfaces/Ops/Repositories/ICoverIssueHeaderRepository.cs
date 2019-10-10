using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ICoverIssueHeaderRepository : IRepository<CoverIssueHeader, int>
    {
        CoverIssueHeader GetCoverIssueHeaderByBibID(int BibID);
        Task<List<CoverIssueHeader>> GetAllHeadersAsync();
        Task<ICollection<CoverIssueHeader>> PageAsync(CoverIssueHeaderFilter filter);
        Task<int> CountAsync(CoverIssueHeaderFilter filter);
    }
}