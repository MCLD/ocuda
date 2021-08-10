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
    public class PermissionGroupReplaceFilesRepository
        : GenericRepository<OpsContext, PermissionGroupReplaceFiles>,
        IPermissionGroupReplaceFilesRepository
    {
        public PermissionGroupReplaceFilesRepository(Repository<OpsContext> repositoryFacade,
            ILogger<PermissionGroupReplaceFilesRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddSaveAsync(int fileLibraryId, int permissionGroupId)
        {
            await DbSet.AddAsync(new PermissionGroupReplaceFiles
            {
                FileLibraryId = fileLibraryId,
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

        public async Task<ICollection<PermissionGroupReplaceFiles>>
            GetByFileLibraryId(int fileLibraryId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.FileLibraryId == fileLibraryId)
                .ToListAsync();
        }

        public async Task<ICollection<PermissionGroupReplaceFiles>>
            GetByPermissionGroupIdAsync(int permissionGroupId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PermissionGroupId == permissionGroupId)
                .ToListAsync();
        }

        public async Task RemoveSaveAsync(int fileLibraryId, int permissionGroupId)
        {
            var item = await DbSet.SingleOrDefaultAsync(_ => _.FileLibraryId == fileLibraryId
                && _.PermissionGroupId == permissionGroupId);
            if (item == null)
            {
                throw new OcudaException($"Unable to find permission for file library id {fileLibraryId} and permission group {permissionGroupId}");
            }
            DbSet.Remove(item);
            await SaveAsync();
        }
    }
}