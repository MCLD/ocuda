﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ISiteAlertRepository : IGenericRepository<SiteAlert>
    {
        Task<ICollection<SiteAlert>> GetCurrentSiteAlertsAsync();
    }
}
