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
        private readonly ICarouselRepository _carouselRepository;
        public CarouselService(ILogger<CarouselService> logger,
            IHttpContextAccessor httpContextAccessor,
            ICarouselRepository carouselRepository)
            : base(logger, httpContextAccessor)
        {
            _carouselRepository = carouselRepository 
                ?? throw new ArgumentNullException(nameof(carouselRepository));
        }

        public async Task<DataWithCount<ICollection<Carousel>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _carouselRepository.GetPaginatedListAsync(filter);
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
    }
}