using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILocationService
    {
        Task<List<Location>> GetAllLocationsAsync();
        Task<Location> GetLocationByStubAsync(string locationStub);
    }
}
