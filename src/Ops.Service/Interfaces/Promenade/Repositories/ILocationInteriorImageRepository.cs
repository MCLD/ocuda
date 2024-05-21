using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models.Google;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ILocationInteriorImageRepository : IGenericRepository<LocationInteriorImage>
    {
        Task FixInteriorImageSortOrder(int locationId);

        Task<LocationInteriorImage> GetInteriorImageByIdAsync(int imageId);

        Task<List<LocationInteriorImage>> GetLocationInteriorImagesAsync(int locationId);

        Task<int> GetNextSortOrderAsync(int locationId);
    }
}