using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ICarouselItemTextRepository : IGenericRepository<CarouselItemText>
    {
        Task<CarouselItemText> GetByCarouselItemAndLanguageAsync(int carouselItemId,
            int languageId);
    }
}
