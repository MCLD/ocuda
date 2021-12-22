using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class HistoricalIncidentService
        : BaseService<HistoricalIncidentService>, IHistoricalIncidentService
    {
        public HistoricalIncidentService(ILogger<HistoricalIncidentService> logger,
            IHttpContextAccessor httpContextAccessor) : base(logger, httpContextAccessor)
        {
        }

        public Task<DataWithCount<ICollection<HistoricalIncident>>>
            GetPaginatedAsync(BaseFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<Post> GetPostByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
