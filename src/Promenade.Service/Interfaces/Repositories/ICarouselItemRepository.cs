using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ICarouselItemRepository : IGenericRepository<CarouselItem>
    {
        Task<CarouselItem> GetForLayoutIncludingChildrenByIdAsync(int itemId, int layoutId);
    }
}
