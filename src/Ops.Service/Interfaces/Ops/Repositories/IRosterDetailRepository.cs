﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IRosterDetailRepository : IOpsRepository<RosterDetail, int>
    {
        Task AddRangeAsync(IEnumerable<RosterDetail> rosterDetails);

        Task<RosterDetail> GetAsync(int rosterId, string email);

        Task<ICollection<RosterDetail>> GetByHeaderIdAsync(int rosterHeaderId);

        Task<int> GetCountAsync(int rosterHeaderId);
    }
}