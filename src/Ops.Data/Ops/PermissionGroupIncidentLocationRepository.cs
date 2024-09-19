using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Data.Ops
{
    public class PermissionGroupIncidentLocationRepository :
        GenericRepository<OpsContext, PermissionGroupIncidentLocation>,
        IPermissionGroupIncidentLocationRepository
    {
        public PermissionGroupIncidentLocationRepository(Repository<OpsContext> repositoryFacade,
            ILogger<PermissionGroupIncidentLocationRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddSaveAsync(int locationId, int permissionGroupId)
        {
            await DbSet.AddAsync(new PermissionGroupIncidentLocation
            {
                LocationId = locationId,
                PermissionGroupId = permissionGroupId
            });
            await SaveAsync();
        }

        public async Task<bool> AnyPermissionGroupIdAsync(IEnumerable<int> permissionGroupIds)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => permissionGroupIds.Contains(_.PermissionGroupId))
                .AnyAsync();
        }

        public async Task<ICollection<PermissionGroupIncidentLocation>> GetByLocationIdAsync(int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> GetByPermissionGroupIdsAsync(IEnumerable<int> permissionGroupIds)
        {
            return await DbSet
                .Where(_ => permissionGroupIds.Contains(_.PermissionGroupId))
                .AsNoTracking()
                .Select(_ => _.LocationId)
                .ToListAsync();
        }

        public async Task RemoveSaveAsync(int locationId, int permissionGroupId)
        {
            var item = await DbSet.SingleOrDefaultAsync(_ => _.LocationId == locationId
                && _.PermissionGroupId == permissionGroupId)
                ?? throw new OcudaException($"Unable to find permission for location id {locationId} and permission group {permissionGroupId}");
            DbSet.Remove(item);
            await SaveAsync();
        }
    }
}