using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ICarouselButtonRepository : IGenericRepository<CarouselButton>
    {
        Task<CarouselButton> FindAsync(int id);
        Task<CarouselButton> GetByItemAndOrderAsync(int itemId, int order);
        Task<List<CarouselButton>> GetCarouselSubsequentAsync(int itemId, int order);
        Task<int?> GetMaxSortOrderForItemAsync(int itemId);
    }
}
