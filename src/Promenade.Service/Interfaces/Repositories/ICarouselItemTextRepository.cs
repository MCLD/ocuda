using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ICarouselItemTextRepository : IGenericRepository<CarouselItemText>
    {
        Task<CarouselItemText> GetByIdsAsync(int itemId, int languageId);
    }
}
