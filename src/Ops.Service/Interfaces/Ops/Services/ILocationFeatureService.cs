using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILocationFeatureService
    {
        Task<List<LocationFeature>> GetAllLocationFeaturesAsync();
        Task<List<LocationFeature>> GetLocationFeaturesByLocationAsync(Location location);
        Task<LocationFeature> AddLocationFeatureAsync(LocationFeature locationFeature);
        Task<LocationFeature> EditAsync(LocationFeature locationFeature);
        Task DeleteAsync(int id);
        Task<LocationFeature> GetLocationFeatureByIdAsync(int locationFeatureId);

    }
}
