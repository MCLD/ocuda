﻿using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IClaimGroupRepository : IOpsRepository<ClaimGroup, int>
    {
        Task<bool> IsClaimGroup(string claim, string group);
    }
}
