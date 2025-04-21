using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class HistoricalIncidentService
        : BaseService<HistoricalIncidentService>, IHistoricalIncidentService
    {
        private readonly IHistoricalIncidentRepository _historicalIncidentRepository;

        public HistoricalIncidentService(ILogger<HistoricalIncidentService> logger,
            IHttpContextAccessor httpContextAccessor,
            IHistoricalIncidentRepository historicalIncidentRepository)
            : base(logger, httpContextAccessor)
        {
            _historicalIncidentRepository = historicalIncidentRepository
                ?? throw new ArgumentNullException(nameof(historicalIncidentRepository));
        }

        public async Task<HistoricalIncident> GetAsync(int id)
        {
            return await _historicalIncidentRepository.GetAsync(id);
        }

        public async Task<DataWithCount<ICollection<HistoricalIncident>>>
                    GetPaginatedAsync(SearchFilter filter)
        {
            return await _historicalIncidentRepository.GetPaginatedAsync(filter);
        }
    }
}
