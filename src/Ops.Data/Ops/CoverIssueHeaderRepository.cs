using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class CoverIssueHeaderRepository
        : OpsRepository<OpsContext, CoverIssueHeader, int>, ICoverIssueHeaderRepository
    {
        public CoverIssueHeaderRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<CoverIssueHeaderRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CoverIssueHeader> GetByBibIdAsync(int BibId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.BibId == BibId)
                .SingleOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<CoverIssueHeader>>> GetPaginiatedListAsync(
            CoverIssueFilter filter)
        {
            var query = ApplyFilters(filter);

            return new DataWithCount<ICollection<CoverIssueHeader>>
            {
                Count = await query.CountAsync(),
                Data = await query
                .OrderByDescending(_ => _.HasPendingIssue)
                .ThenByDescending(_ => _.LastResolved)
                .ApplyPagination(filter)
                .ToListAsync()
            };
        }
        private IQueryable<CoverIssueHeader> ApplyFilters(CoverIssueFilter filter)
        {
            var items = DbSet.AsNoTracking();

            return filter.CoverIssueType == CoverIssueType.Open ?
                items.Where(_ => _.HasPendingIssue)
                : items.Where(_ => !_.HasPendingIssue);
        }
    }
}
