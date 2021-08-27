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
    public class PermissionGroupSectionManagerRepository
        : GenericRepository<OpsContext, PermissionGroupSectionManager>,
        IPermissionGroupSectionManagerRepository
    {
        public PermissionGroupSectionManagerRepository(Repository<OpsContext> repositoryFacade,
            ILogger<PermissionGroupSectionManagerRepository> logger)
            : base(repositoryFacade, logger)
        {
        }

        public async Task AddSaveAsync(int sectionId, int permissionGroupId)
        {
            await DbSet.AddAsync(new PermissionGroupSectionManager
            {
                SectionId = sectionId,
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

        public async Task<ICollection<PermissionGroupSectionManager>>
            GetByPermissionGroupId(int permissionGroupId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PermissionGroupId == permissionGroupId)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>>
            GetByPermissionGroupIdsAsync(IEnumerable<int> permissionGroupIds)
        {
            return await DbSet
                .Where(_ => permissionGroupIds.Contains(_.PermissionGroupId))
                .AsNoTracking()
                .Select(_ => _.SectionId)
                .ToListAsync();
        }

        public async Task<ICollection<PermissionGroupSectionManager>>
                    GetBySectionIdAsync(int sectionId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId)
                .ToListAsync();
        }

        public async Task RemoveSaveAsync(int sectionId, int permissionGroupId)
        {
            var item = await DbSet.SingleOrDefaultAsync(_ => _.SectionId == sectionId
                && _.PermissionGroupId == permissionGroupId);
            if (item == null)
            {
                throw new OcudaException($"Unable to find permission for section id {sectionId} and permission group {permissionGroupId}");
            }
            DbSet.Remove(item);
            await SaveAsync();
        }
    }
}