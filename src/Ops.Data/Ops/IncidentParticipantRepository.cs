using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

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

        public async Task<IEnumerable<int>> IncidentIdsSearchAsync(string searchText)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Barcode.Contains(searchText)
                    || _.Description.Contains(searchText)
                    || _.Name.Contains(searchText))
                .Select(_ => _.IncidentId)
                .ToListAsync();
        }
    }
}