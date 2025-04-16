using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class CoverIssueDetailRepository
        : OpsRepository<OpsContext, CoverIssueDetail, int>, ICoverIssueDetailRepository
    {
        public CoverIssueDetailRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<CoverIssueDetailRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<CoverIssueDetail>> GetByHeaderIdAsync(int headerId,
            bool includeCreatedByUser = false,
            bool? resolved = null)
        {
            var query = DbSet
                .AsNoTracking()
                .Where(_ => _.CoverIssueHeaderId == headerId);

            if (resolved.HasValue)
            {
                query = query.Where(_ => _.IsResolved == resolved);
            }

            if (includeCreatedByUser)
            {
                query = query.Include(_ => _.CreatedByUser);
            }

            return await query
                .OrderBy(_ => _.IsResolved)
                .ThenByDescending(_ => _.CreatedAt)
                .ToListAsync();
        }
    }
}