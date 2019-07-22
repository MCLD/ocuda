using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationRepository : IRepository<Location, int>
    {
        Task<Location> GetLocationByStub(string stub);
        Task<List<Location>> GetAllLocations();
    }
}
