using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class CoverIssueDetailRepository
        : GenericRepository<OpsContext, CoverIssueDetail, int>, ICoverIssueDetailRepository
    {
        public CoverIssueDetailRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<CoverIssueDetailRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<CoverIssueDetail>> GetCoverIssueDetailsByHeader(int headerId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CoverIssueHeaderId == headerId)
                .OrderBy(_ => _.CreatedAt)
                .ToListAsync();
        }
    }
}
