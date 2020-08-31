using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ICarouselItemTextRepository : IGenericRepository<CarouselItemText>
    {
        Task<ICollection<CarouselItemText>> GetAllForCarouselAsync(int carouselId);
        Task<ICollection<CarouselItemText>> GetAllForCarouselItemAsync(int itemId);
        Task<CarouselItemText> GetByCarouselItemAndLanguageAsync(int carouselItemId,
            int languageId);
    }
}
