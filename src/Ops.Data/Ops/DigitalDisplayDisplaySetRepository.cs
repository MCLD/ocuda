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
    public class DigitalDisplayDisplaySetRepository
        : GenericRepository<OpsContext, DigitalDisplayDisplaySet>,
        IDigitalDisplayDisplaySetRepository

    {
        public DigitalDisplayDisplaySetRepository(Repository<OpsContext> repositoryFacade,
            ILogger<DigitalDisplayDisplaySetRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<DigitalDisplayDisplaySet>>
            GetByDisplayIdsAsync(IEnumerable<int> displayIds)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => displayIds.Contains(_.DigitalDisplayId))
                .ToListAsync();
        }

        public async Task<IDictionary<int, int>> GetSetsDisplaysCountsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .GroupBy(_ => _.DigitalDisplaySetId)
                .Select(_ => new { _.Key, Count = _.Count() })
                .ToDictionaryAsync(k => k.Key, v => v.Count);
        }
    }
}