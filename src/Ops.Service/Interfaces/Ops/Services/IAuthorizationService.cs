﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IAuthorizationService
    {
        Task EnsureSiteManagerGroupAsync(int currentUserId, string group);
        Task<ICollection<ClaimGroup>> GetClaimGroupsAsync();
        Task<IEnumerable<SectionManagerGroup>> GetSectionManagerGroupsAsync();
        Task<ICollection<PermissionGroup>> GetPermissionGroupsAsync();
    }
}
