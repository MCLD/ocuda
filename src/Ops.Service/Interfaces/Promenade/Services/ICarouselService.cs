using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ICarouselService
    {
        Task<Carousel> CreateAsync(Carousel carousel);
        Task DeleteAsync(int carouselId);
        Task<Carousel> EditAsync(Carousel carousel);
        Task<DataWithCount<ICollection<Carousel>>> GetPaginatedListAsync(BaseFilter filter);
    }
}
