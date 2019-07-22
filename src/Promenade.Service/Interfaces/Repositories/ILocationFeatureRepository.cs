using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationFeatureRepository : IRepository<LocationFeature, int>
    {
        Task<List<LocationFeature>> GetLocationFeaturesById(int locationId);
    }
}

