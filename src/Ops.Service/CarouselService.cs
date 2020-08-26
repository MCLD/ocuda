using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class CarouselService : BaseService<CarouselService>, ICarouselService
    {
        private readonly ICarouselButtonLabelRepository _carouselButtonLabelRepository;
        private readonly ICarouselItemRepository _carouselItemRepository;
        private readonly ICarouselItemTextRepository _carouselItemTextRepository;
        private readonly ICarouselRepository _carouselRepository;
        private readonly ICarouselTextRepository _carouselTextRepository;

        public CarouselService(ILogger<CarouselService> logger,
            IHttpContextAccessor httpContextAccessor,
            ICarouselButtonLabelRepository carouselButtonLabelRepository,
            ICarouselItemRepository carouselItemRepository,
            ICarouselItemTextRepository carouselItemTextRepository,
            ICarouselRepository carouselRepository,
            ICarouselTextRepository carouselTextRepository)
            : base(logger, httpContextAccessor)
        {
            _carouselButtonLabelRepository = carouselButtonLabelRepository
                ?? throw new ArgumentNullException(nameof(carouselButtonLabelRepository));
            _carouselItemRepository = carouselItemRepository
                ?? throw new ArgumentNullException(nameof(carouselItemRepository));
            _carouselItemTextRepository = carouselItemTextRepository
                ?? throw new ArgumentNullException(nameof(carouselItemTextRepository));
            _carouselRepository = carouselRepository
                ?? throw new ArgumentNullException(nameof(carouselRepository));
            _carouselTextRepository = carouselTextRepository
                ?? throw new ArgumentNullException(nameof(carouselTextRepository));
        }

        public async Task<DataWithCount<ICollection<Carousel>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _carouselRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Carousel> GetCarouselDetailsAsync(int id, int languageId)
        {
            var carousel = await _carouselRepository.GetIncludingChildrenAsync(id);

            carousel.CarouselText = await _carouselTextRepository
                .GetByCarouselAndLanguageAsync(id, languageId);

            carousel.Items = carousel.Items.OrderBy(_ => _.Order).ToList();
            foreach (var item in carousel.Items)
            {
                item.CarouselItemText = await _carouselItemTextRepository
                    .GetByCarouselItemAndLanguageAsync(item.Id, languageId);
            }

            return carousel;
        }

        public async Task<Carousel> CreateAsync(Carousel carousel)
        {
            carousel.Name = carousel.Name?.Trim();

            await _carouselRepository.AddAsync(carousel);
            await _carouselRepository.SaveAsync();
            return carousel;
        }

        public async Task<Carousel> EditAsync(Carousel carousel)
        {
            var currentCarousel = await _carouselRepository.FindAsync(carousel.Id);
            currentCarousel.Name = carousel.Name?.Trim();

            _carouselRepository.Update(currentCarousel);
            await _carouselRepository.SaveAsync();
            return currentCarousel;
        }

        public async Task DeleteAsync(int carouselId)
        {
            var carousel = await _carouselRepository.FindAsync(carouselId);

            _carouselRepository.Remove(carousel);
            await _carouselRepository.SaveAsync();
        }

        public async Task<CarouselItem> CreateItemAsync(CarouselItem carouselItem)
        {
            carouselItem.Name = carouselItem.Name?.Trim();

            var maxSortOrder = await _carouselItemRepository
                .GetMaxSortOrderForCarouselAsync(carouselItem.CarouselId);
            if (maxSortOrder.HasValue)
            {
                carouselItem.Order = maxSortOrder.Value + 1;
            }

            await _carouselItemRepository.AddAsync(carouselItem);
            await _carouselItemRepository.SaveAsync();
            return carouselItem;
        }

        public async Task<CarouselItem> EditItemAsync(CarouselItem carouselItem)
        {
            var currentCarouselItem = await _carouselItemRepository.FindAsync(carouselItem.Id);
            currentCarouselItem.Name = carouselItem.Name?.Trim();

            _carouselItemRepository.Update(currentCarouselItem);
            await _carouselItemRepository.SaveAsync();
            return carouselItem;
        }

        public async Task DeleteItemAsync(int carouselItemId)
        {
            var carouselItem = await _carouselItemRepository.FindAsync(carouselItemId);

            var subsequentItems = await _carouselItemRepository.GetCarouselSubsequentAsync(
                carouselItem.CarouselId, carouselItem.Order);

            if (subsequentItems.Count > 0)
            {
                subsequentItems.ForEach(_ => _.Order--);
                _carouselItemRepository.UpdateRange(subsequentItems);
            }

            _carouselItemRepository.Remove(carouselItem);
            await _carouselItemRepository.SaveAsync();
        }

        public async Task UpdateItemSortOrder(int id, bool increase)
        {
            var item = await _carouselItemRepository.FindAsync(id);

            int newSortOrder;
            if (increase)
            {
                newSortOrder = item.Order + 1;
            }
            else
            {
                if (item.Order == 0)
                {
                    throw new OcudaException("Item is already in the first position.");
                }
                newSortOrder = item.Order - 1;
            }
            
            var itemInPosition = await _carouselItemRepository.GetByCarouselAndOrderAsync(
                item.CarouselId, newSortOrder);

            if (itemInPosition == null)
            {
                throw new OcudaException("Item is already in the last position.");
            }

            itemInPosition.Order = item.Order;
            item.Order = newSortOrder;

            _carouselItemRepository.Update(item);
            _carouselItemRepository.Update(itemInPosition);
            await _carouselItemRepository.SaveAsync();
        }

        public async Task<CarouselItemText> SetItemTextAsync(CarouselItemText itemText)
        {
            var currentText = await _carouselItemTextRepository.GetByCarouselItemAndLanguageAsync(
                itemText.CarouselItemId, itemText.LanguageId);

            if (currentText == null)
            {
                itemText.Description = itemText.Description?.Trim();
                itemText.ImageUrl = itemText.ImageUrl?.Trim();
                itemText.Label = itemText.Label?.Trim();
                itemText.Title = itemText.Title?.Trim();

                await _carouselItemTextRepository.AddAsync(itemText);
                await _carouselItemTextRepository.SaveAsync();
                return itemText;
            }
            else
            {
                currentText.Description = itemText.Description?.Trim();
                currentText.ImageUrl = itemText.ImageUrl?.Trim();
                currentText.Label = itemText.Label?.Trim();
                currentText.Title = itemText.Title?.Trim();

                _carouselItemTextRepository.Update(currentText);
                await _carouselItemTextRepository.SaveAsync();
                return currentText;
            }
        }

        public async Task<ICollection<CarouselButtonLabel>> GetButtonLabelsAsync()
        {
            return await _carouselButtonLabelRepository.GetAllAsync();
        }
    }
}