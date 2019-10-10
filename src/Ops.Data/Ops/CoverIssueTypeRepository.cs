using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class CoverIssueTypeRepository
        : GenericRepository<OpsContext, CoverIssueType, int>, ICoverIssueTypeRepository
    {
        public CoverIssueTypeRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<CoverIssueTypeRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<bool> IsDuplicateName(CoverIssueType issueType)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Name != issueType.Name)
                .AnyAsync();
        }

        public async Task<List<CoverIssueType>> GetAllTypesAsync()
        {
            return await DbSet
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
