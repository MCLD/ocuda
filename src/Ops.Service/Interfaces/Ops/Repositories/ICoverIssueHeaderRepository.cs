using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ICoverIssueHeaderRepository : IRepository<CoverIssueHeader, int>
    {
        Task<CoverIssueHeader> GetByBibIdAsync(int BibId);

        Task<DataWithCount<ICollection<CoverIssueHeader>>> GetPaginiatedListAsync(
            BaseFilter filter);
    }
}