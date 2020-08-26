using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ICarouselItemRepository : IGenericRepository<CarouselItem>
    {
        Task<CarouselItem> FindAsync(int id);
        Task<CarouselItem> GetByCarouselAndOrderAsync(int carouselId, int order);
        Task<List<CarouselItem>> GetCarouselSubsequentAsync(int carouselId, int order);
        Task<int?> GetMaxSortOrderForCarouselAsync(int carouselId);
    }
}
