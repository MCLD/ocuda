using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops
{
    public interface IClaimGroupRepository : IRepository<ClaimGroup, int>

    {
        Task<bool> IsClaimGroup(string claim, string group);
        Task<ICollection<string>> GroupsToListAsync(string claim);
    }
}
