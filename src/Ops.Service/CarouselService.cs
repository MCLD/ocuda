using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class CarouselService : BaseService<CarouselService>, ICarouselService
    {
        private readonly ICarouselItemRepository _carouselItemRepository;
        private readonly ICarouselItemTextRepository _carouselItemTextRepository;
        private readonly ICarouselRepository _carouselRepository;
        private readonly ICarouselTextRepository _carouselTextRepository;

        public CarouselService(ILogger<CarouselService> logger,
            IHttpContextAccessor httpContextAccessor,
            ICarouselItemRepository carouselItemRepository,
            ICarouselItemTextRepository carouselItemTextRepository,
            ICarouselRepository carouselRepository,
            ICarouselTextRepository carouselTextRepository)
            : base(logger, httpContextAccessor)
        {
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

            _carouselItemRepository.Remove(carouselItem);
            await _carouselItemRepository.SaveAsync();
        }
    }
}