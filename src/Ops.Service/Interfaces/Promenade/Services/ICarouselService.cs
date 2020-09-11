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
        Task<CarouselButton> CreateButtonAsync(CarouselButton button);
        Task<CarouselItem> CreateItemAsync(CarouselItem carouselItem);
        Task<Carousel> CreateNoSaveAsync(Carousel carousel);
        Task DeleteAsync(int carouselId);
        Task DeleteButtonAsync(int carouselButtonId);
        Task DeleteItemAsync(int carouselItemId);
        Task DeleteNoSaveAsync(int carouselId);
        Task<Carousel> EditAsync(Carousel carousel);
        Task<CarouselButton> EditButtonAsync(CarouselButton carouselButton);
        Task<CarouselItem> EditItemAsync(CarouselItem carouselItem);
        Task<ICollection<CarouselButtonLabel>> GetButtonLabelsAsync();
        Task<Carousel> GetCarouselDetailsAsync(int id, int languageId);
        Task<int> GetCarouselIdForButtonAsync(int id);
        Task<CarouselItem> GetItemByIdAsync(int id);
        Task<int?> GetPageHeaderIdForCarouselAsync(int id);
        Task<int?> GetPageLayoutIdForCarouselAsync(int id);
        Task<DataWithCount<ICollection<Carousel>>> GetPaginatedListAsync(BaseFilter filter);
        Task<CarouselText> SetCarouselTextAsync(CarouselText carouselText);
        Task<CarouselItemText> SetItemTextAsync(CarouselItemText itemText);
        Task UpdateButtonSortOrder(int id, bool increase);
        Task UpdateItemSortOrder(int id, bool increase);
    }
}
