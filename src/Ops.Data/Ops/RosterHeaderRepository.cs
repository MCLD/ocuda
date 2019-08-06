﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class RosterHeaderRepository
        : GenericRepository<OpsContext, RosterHeader, int>, IRosterHeaderRepository
    {
        public RosterHeaderRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<RosterHeaderRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<int?> GetLatestIdAsync()
        {
            var latest = await DbSet
                .AsNoTracking()
                .OrderByDescending(_ => _.CreatedAt)
                .FirstOrDefaultAsync();

            if (latest != null)
            {
                return latest.Id;
            }
            else
            {
                return null;
            }
        }
    }
}