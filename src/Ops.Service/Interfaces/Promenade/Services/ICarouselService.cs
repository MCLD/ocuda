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
        Task<CarouselItem> CreateItemAsync(CarouselItem carouselItem);
        Task DeleteAsync(int carouselId);
        Task DeleteItemAsync(int carouselItemId);
        Task<Carousel> EditAsync(Carousel carousel);
        Task<CarouselItem> EditItemAsync(CarouselItem carouselItem);
        Task<Carousel> GetCarouselDetailsAsync(int id, int languageId);
        Task<DataWithCount<ICollection<Carousel>>> GetPaginatedListAsync(BaseFilter filter);
        Task<CarouselItemText> SetItemTextAsync(CarouselItemText itemText);
        Task UpdateItemSortOrder(int id, bool increase);
    }
}
