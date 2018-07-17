using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class RosterHeaderRepository
        : GenericRepository<Models.RosterHeader, int>, IRosterHeaderRepository
    {
        public RosterHeaderRepository(OpsContext context, ILogger<RosterHeaderRepository> logger)
            : base(context, logger)
        {
        }
    }
}