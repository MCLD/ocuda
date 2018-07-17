using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IAuthorizationService
    {
        Task EnsureSiteManagerGroupAsync(int currentUserId, string group);
        Task<ICollection<ClaimGroup>> GetClaimGroupsAsync();
        Task<IEnumerable<SectionManagerGroup>> GetSectionManagerGroupsAsync();
    }
}
