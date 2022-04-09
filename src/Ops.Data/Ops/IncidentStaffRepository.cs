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
    public class IncidentStaffRepository
        : OpsRepository<OpsContext, IncidentStaff, int>, IIncidentStaffRepository
    {
        public IncidentStaffRepository(Repository<OpsContext> repositoryFacade,
            ILogger<IncidentStaffRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<IncidentStaff>> GetByIncidentIdAsync(int incidentId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IncidentId == incidentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> IncidentIdsSearchAsync(IEnumerable<int> userIds)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => userIds.Contains(_.UserId))
                .Select(_ => _.IncidentId)
                .ToListAsync();
        }
    }
}
