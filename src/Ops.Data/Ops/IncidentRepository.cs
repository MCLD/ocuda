using System;
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
    public class IncidentRepository : OpsRepository<OpsContext, Incident, int>, IIncidentRepository
    {
        public IncidentRepository(Repository<OpsContext> repositoryFacade,
            ILogger<IncidentRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CollectionWithCount<Incident>> GetPaginatedAsync(IncidentFilter filter)
        {
            if (filter == null) { throw new ArgumentNullException(nameof(filter)); }

            var query = filter.CreatedById.HasValue
                ? DbSet.Where(_ => _.CreatedBy == filter.CreatedById.Value)
                : DbSet;

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                query = query.Where(_ => _.Description.Contains(filter.SearchText)
                    || _.IncidentType.Description.Contains(filter.SearchText)
                    || _.InjuriesDamages.Contains(filter.SearchText)
                    || _.LocationDescription.Contains(filter.SearchText)
                    || filter.LocationIds.Contains(_.LocationId)
                    || _.CreatedByUser.Name.Contains(filter.SearchText));
            }

            return new CollectionWithCount<Incident>
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
