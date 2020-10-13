using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ICarouselRepository : IGenericRepository<Carousel>
    {
        Task<Carousel> FindAsync(int id);
        Task<Carousel> GetIncludingChildrenWithLabelsAsync(int id);
        Task<Carousel> GetIncludingChildrenAsync(int id);
        Task<DataWithCount<ICollection<Carousel>>> GetPaginatedListAsync(BaseFilter filter);
        Task<ICollection<Carousel>> GetAllAsync();
        Task<int?> GetPageHeaderIdForCarouselAsync(int id);
        Task<int?> GetPageLayoutIdForCarouselAsync(int id);
    }
}
