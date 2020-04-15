﻿using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ISiteSettingPromRepository : IGenericRepository<SiteSetting>
    {
        Task<SiteSetting> FindAsync(string id);
    }
}
