using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ICarouselTextRepository : IGenericRepository<CarouselText>
    {
        Task<CarouselText> GetByIdsAsync(int carouselId, int languageId);
    }
}
