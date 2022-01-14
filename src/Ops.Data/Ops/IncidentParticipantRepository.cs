using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using System.Linq;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ocuda.Ops.Data.Ops
{
    public class IncidentParticipantRepository 
        : OpsRepository<OpsContext, IncidentParticipant, int>, IIncidentParticipantRepository
    {
        public IncidentParticipantRepository(Repository<OpsContext> repositoryFacade,
            ILogger<IncidentParticipantRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<IncidentParticipant>> GetByIncidentIdAsync(int incidentId)
        {
            return await DbSet.AsNoTracking().Where(_ => _.IncidentId == incidentId).ToListAsync();
        }
    }
}
