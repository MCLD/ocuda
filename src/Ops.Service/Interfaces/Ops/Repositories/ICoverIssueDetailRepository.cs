using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ICoverIssueDetailRepository : IOpsRepository<CoverIssueDetail, int>
    {
        Task<ICollection<CoverIssueDetail>> GetByHeaderIdAsync(int headerId,
            bool includeCreatedByUser = false,
            bool? resolved = null);
    }
}
