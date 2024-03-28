using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ILocationInteriorImageRepository : IGenericRepository<LocationInteriorImage>
    {
        Task<LocationInteriorImage> GetInteriorImageByIdAsync(int imageId);
        Task<List<LocationInteriorImage>> GetLocationInteriorImagesAsync(int locationId);
    }
}