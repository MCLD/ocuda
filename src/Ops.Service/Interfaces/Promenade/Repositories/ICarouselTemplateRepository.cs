using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ICarouselTemplateRepository : IGenericRepository<CarouselTemplate>
    {
        Task<ICollection<CarouselTemplate>> GetAllAsync();
        Task<CarouselTemplate> GetByCarouselItemAsync(int itemId);
        Task<CarouselTemplate> GetForPageLayoutAsync(int pageLayoutId);
    }
}
