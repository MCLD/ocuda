using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ICarouselTextRepository : IGenericRepository<CarouselText>
    {
        Task<CarouselText> GetByCarouselAndLanguageAsync(int carouselId, int languageId);
        Task<ICollection<CarouselText>> GetForCarouselAsync(int carouselId);
    }
}
