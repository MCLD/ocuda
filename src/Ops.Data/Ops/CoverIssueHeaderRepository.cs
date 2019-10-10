using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class CoverIssueHeaderRepository
        : GenericRepository<OpsContext, CoverIssueHeader, int>, ICoverIssueHeaderRepository
    {
        public CoverIssueHeaderRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<CoverIssueHeaderRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public CoverIssueHeader GetCoverIssueHeaderByBibID(int BibID)
        {
            return DbSet
                .AsNoTracking()
                .Where(_ => _.BibID == BibID)
                .FirstOrDefault();
        }

        public async Task<List<CoverIssueHeader>> GetAllHeadersAsync()
        {
            return await DbSet
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> CountAsync(CoverIssueHeaderFilter filter)
        {
            return await ApplyFilters(filter)
                .CountAsync();
        }

        private IQueryable<CoverIssueHeader> ApplyFilters(CoverIssueHeaderFilter filter)
        {
            var items = DbSet.AsNoTracking();
            var orderBy = filter.OrderBy.ToString();
            var propertyInfo = typeof(CoverIssueHeader).GetProperty(orderBy);
            if (filter.OrderDesc)
            {
                items = items.OrderByDescending(_ => propertyInfo.GetValue(_, null));
            }
            else
            {
                items = items.OrderBy(_ => propertyInfo.GetValue(_, null));
            }
            if (!string.IsNullOrEmpty(filter.Search))
            {
                items.Where(_ => _.BibID.ToString().Contains(filter.Search) ||
                _.CreatedBy.ToString().Contains(filter.Search));
            }
            return items;
        }

        public async Task<ICollection<CoverIssueHeader>> PageAsync(CoverIssueHeaderFilter filter)
        {
            return await ApplyFilters(filter)
                .ApplyPagination(filter)
                .ToListAsync();
        }
    }
}
