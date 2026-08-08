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
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class IncidentRepository(Repository<OpsContext> repositoryFacade,
        ILogger<IncidentRepository> logger,
        IDateTimeProvider dateTimeProvider)
        : OpsRepository<OpsContext, Incident, int>(repositoryFacade, logger), IIncidentRepository
    {
        public async Task<CollectionWithCount<Incident>> GetPaginatedAsync(IncidentFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var incidents = DbSet.Where(_ => _.IsVisible);

            var query = filter.CreatedById.HasValue
                ? incidents.Where(_ => _.CreatedBy == filter.CreatedById.Value)
                : incidents;

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                query = query.Where(_ => _.Id.ToString() == filter.SearchText
                    || _.Description.Contains(filter.SearchText)
                    || _.IncidentType.Description.Contains(filter.SearchText)
                    || _.InjuriesDamages.Contains(filter.SearchText)
                    || _.LocationDescription.Contains(filter.SearchText)
                    || _.CreatedByUser.Name.Contains(filter.SearchText)
                    || filter.IncludeIds.Contains(_.Id));
            }

            if (filter.LocationIds?.Count() > 0)
            {
                query = query.Where(_ => filter.LocationIds.Contains(_.LocationId));
            }

            return new CollectionWithCount<Incident>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.IncidentAt)
                    .ApplyPagination(filter)
                    .AsNoTracking()
                    .ToListAsync(),
            };
        }

        public async Task<Incident> GetRelatedAsync(int incidentId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == incidentId && _.IsVisible)
                .Select(_ => new Incident
                {
                    CreatedBy = _.CreatedBy,
                    Id = _.Id,
                    IncidentAt = _.IncidentAt,
                    IncidentTypeId = _.IncidentTypeId,
                    LocationId = _.LocationId,
                    ReportedByName = _.ReportedByName,
                })
                .SingleOrDefaultAsync();
        }

        public async Task SetVisibilityAsync(int incidentId, int userId, bool isVisible)
        {
            var incident = await DbSet
                .SingleOrDefaultAsync(_ => _.Id == incidentId);

            if (incident != null)
            {
                incident.IsVisible = isVisible;
                incident.UpdatedBy = userId;
                incident.UpdatedAt = dateTimeProvider.Now;
                _context.Update(incident);
                await _context.SaveChangesAsync();
            }
        }
    }
}
