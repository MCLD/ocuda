using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class IncidentService : BaseService<IncidentService>, IIncidentService
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IIncidentTypeRepository _incidentTypeRepository;
        private readonly ILocationRepository _locationRepository;

        public IncidentService(ILogger<IncidentService> logger,
            IHttpContextAccessor httpContextAccessor,
            IIncidentRepository incidentRepository,
            IIncidentTypeRepository incidentTypeRepository,
            ILocationRepository locationRepository) : base(logger, httpContextAccessor)
        {
            _incidentRepository = incidentRepository
                ?? throw new ArgumentNullException(nameof(incidentRepository));
            _incidentTypeRepository = incidentTypeRepository
                ?? throw new ArgumentNullException(nameof(incidentTypeRepository));
            _locationRepository = locationRepository
                ?? throw new ArgumentNullException(nameof(locationRepository));
        }

        public async Task<IDictionary<int, string>> GetIncidentTypesAsync()
        {
            var incidentTypes = await _incidentTypeRepository.GetAllAsync();
            return incidentTypes.ToDictionary(k => k.Id, v => v.Description);
        }

        public async Task<CollectionWithCount<Incident>> GetPaginatedAsync(IncidentFilter filter)
        {
            if (filter == null) { filter = new IncidentFilter(); }

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                filter.LocationIds
                    = await _locationRepository.SearchIdsByNameAsync(filter.SearchText);
            }

            return await _incidentRepository.GetPaginatedAsync(filter);
        }
    }
}
