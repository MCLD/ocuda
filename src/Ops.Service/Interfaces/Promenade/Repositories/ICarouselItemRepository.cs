using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ICarouselItemRepository : IGenericRepository<CarouselItem>
    {
        Task<CarouselItem> FindAsync(int id);
    }
}
