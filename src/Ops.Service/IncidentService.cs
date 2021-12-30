using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class IncidentService : BaseService<IncidentService>, IIncidentService
    {
        public IncidentService(ILogger<IncidentService> logger,
            IHttpContextAccessor httpContextAccessor) : base(logger, httpContextAccessor)
        {
        }
    }
}
