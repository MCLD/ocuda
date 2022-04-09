using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class HistoricalIncidentRepository
        : OpsRepository<OpsContext, HistoricalIncident, int>, IHistoricalIncidentRepository
    {
        public HistoricalIncidentRepository(Repository<OpsContext> repositoryFacade,
            ILogger<HistoricalIncidentRepository> logger)
            : base(repositoryFacade, logger)
        {
        }

        public async Task<HistoricalIncident> GetAsync(int id)
        {
            return await DbSet.AsNoTracking().SingleOrDefaultAsync(_ => _.Id == id);
        }

        public async Task<DataWithCount<ICollection<HistoricalIncident>>>
            GetPaginatedAsync(SearchFilter filter)
        {
            if (filter == null) { throw new ArgumentNullException(nameof(filter)); }

            var query = string.IsNullOrEmpty(filter.SearchText)
                ? DbSet.AsNoTracking()
                : DbSet.Where(_ => _.ActionTaken.Contains(filter.SearchText)
                    || _.Branch.Contains(filter.SearchText)
                    || _.Comments.Contains(filter.SearchText)
                    || _.Description.Contains(filter.SearchText)
                    || _.IncidentAtString.Contains(filter.SearchText)
                    || _.Location.Contains(filter.SearchText)
                    || _.PeopleInvolved.Contains(filter.SearchText)
                    || _.PhoneNumber.Contains(filter.SearchText)
                    || _.ReportedBy.Contains(filter.SearchText)
                    || _.Witnesses.Contains(filter.SearchText));

            return new DataWithCount<ICollection<HistoricalIncident>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.IncidentAt)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
