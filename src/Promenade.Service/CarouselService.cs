using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class CarouselService : BaseService<CarouselService>
    {
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICarouselButtonLabelTextRepository _carouselButtonLabelTextRepository;
        private readonly ICarouselItemTextRepository _carouselItemTextRepository;
        private readonly ICarouselRepository _carouselRepository;
        private readonly ICarouselTextRepository _carouselTextRepository;
        private readonly LanguageService _languageService;

        public CarouselService(ILogger<CarouselService> logger,
            IDateTimeProvider dateTimeProvider,
            IDistributedCache cache,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            ICarouselButtonLabelTextRepository carouselButtonLabelTextRepository,
            ICarouselItemTextRepository carouselItemTextRepository,
            ICarouselRepository carouselRepository,
            ICarouselTextRepository carouselTextRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _carouselButtonLabelTextRepository = carouselButtonLabelTextRepository
                ?? throw new ArgumentNullException(nameof(carouselButtonLabelTextRepository));
            _carouselItemTextRepository = carouselItemTextRepository
                ?? throw new ArgumentNullException(nameof(carouselItemTextRepository));
            _carouselRepository = carouselRepository
                ?? throw new ArgumentNullException(nameof(carouselRepository));
            _carouselTextRepository = carouselTextRepository
                ?? throw new ArgumentNullException(nameof(carouselTextRepository));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
        }

        public async Task<Carousel> GetByIdAsync(int carouselId, bool forceReload)
        {
            Carousel carousel = null;

            var cachePagesInHours = GetPageCacheDuration(_config);
            string carouselCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromCarousel,
                carouselId);

            if (cachePagesInHours != null && !forceReload)
            {
                carousel = await GetFromCacheAsync<Carousel>(_cache, carouselCacheKey);
            }

            if (carousel == null)
            {
                carousel = await _carouselRepository.GetIncludingChildrenAsync(carouselId);

                if (carousel != null)
                {
                    carousel.Items = carousel.Items?.OrderBy(_ => _.Order).ToList();
                    foreach (var item in carousel.Items)
                    {
                        item.Carousel = null;
                        item.Buttons = item.Buttons?.OrderBy(_ => _.Order).ToList();

                        foreach (var button in item.Buttons)
                        {
                            button.CarouselItem = null;
                        }
                    }
                }

                await SaveToCacheAsync(_cache, carouselCacheKey, carousel, cachePagesInHours);
            }

            if (carousel != null)
            {
                var currentCultureName = _httpContextAccessor
                    .HttpContext
                    .Features
                    .Get<IRequestCultureFeature>()
                    .RequestCulture
                    .UICulture?
                    .Name;

                int defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
                int? currentLangaugeId = null;
                if (!string.IsNullOrWhiteSpace(currentCultureName))
                {
                    currentLangaugeId = await _languageService.GetLanguageIdAsync(
                        currentCultureName);
                }

                if (currentLangaugeId.HasValue)
                {
                    var carouselTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                        Utility.Keys.Cache.PromCarouselText,
                        currentLangaugeId,
                        carousel.Id);

                    if (cachePagesInHours != null && !forceReload)
                    {
                        carousel.CarouselText = await GetFromCacheAsync<CarouselText>(_cache,
                            carouselTextCacheKey);
                    }

                    if (carousel.CarouselText == null)
                    {
                        carousel.CarouselText = await _carouselTextRepository.GetByIdsAsync(
                            carousel.Id, currentLangaugeId.Value);

                        await SaveToCacheAsync(_cache,
                            carouselTextCacheKey,
                            carousel.CarouselText,
                            cachePagesInHours);
                    }
                }

                if (carousel.CarouselText == null)
                {
                    var carouselTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                        Utility.Keys.Cache.PromCarouselText,
                        defaultLanguageId,
                        carousel.Id);

                    if (cachePagesInHours != null && !forceReload)
                    {
                        carousel.CarouselText = await GetFromCacheAsync<CarouselText>(_cache,
                            carouselTextCacheKey);
                    }

                    if (carousel.CarouselText == null)
                    {
                        carousel.CarouselText = await _carouselTextRepository.GetByIdsAsync(
                            carousel.Id, defaultLanguageId);

                        await SaveToCacheAsync(_cache,
                            carouselTextCacheKey,
                            carousel.CarouselText,
                            cachePagesInHours);
                    }
                }

                var invalidItems = new List<CarouselItem>();

                foreach (var item in carousel.Items)
                {
                    if (currentLangaugeId.HasValue)
                    {
                        var carouselItemTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                            Utility.Keys.Cache.PromCarouselItemText,
                            currentLangaugeId,
                            item.Id);

                        if (cachePagesInHours != null && !forceReload)
                        {
                            item.CarouselItemText = await GetFromCacheAsync<CarouselItemText>(
                                _cache,
                                carouselItemTextCacheKey);
                        }

                        if (item.CarouselItemText == null)
                        {
                            item.CarouselItemText = await _carouselItemTextRepository.GetByIdsAsync(
                                item.Id, currentLangaugeId.Value);

                            await SaveToCacheAsync(_cache,
                                carouselItemTextCacheKey,
                                item.CarouselItemText,
                                cachePagesInHours);
                        }
                    }

                    if (item.CarouselItemText == null)
                    {
                        var carouselItemTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                            Utility.Keys.Cache.PromCarouselItemText,
                            defaultLanguageId,
                            item.Id);

                        if (cachePagesInHours != null && !forceReload)
                        {
                            item.CarouselItemText = await GetFromCacheAsync<CarouselItemText>(
                                _cache,
                                carouselItemTextCacheKey);
                        }

                        if (item.CarouselItemText == null)
                        {
                            item.CarouselItemText = await _carouselItemTextRepository.GetByIdsAsync(
                                item.Id, defaultLanguageId);

                            await SaveToCacheAsync(_cache,
                                carouselItemTextCacheKey,
                                item.CarouselItemText,
                                cachePagesInHours);
                        }
                    }

                    if (item.CarouselItemText == null)
                    {
                        invalidItems.Add(item);
                        continue;
                    }

                    foreach (var button in item.Buttons)
                    {
                        if (currentLangaugeId.HasValue)
                        {
                            var carouselButtonLabelTextCacheKey = string.Format(
                                CultureInfo.InvariantCulture,
                                Utility.Keys.Cache.PromCarouselButtonLabelText,
                                currentLangaugeId,
                                button.LabelId);

                            if (cachePagesInHours != null && !forceReload)
                            {
                                button.LabelText = await GetFromCacheAsync<CarouselButtonLabelText>(
                                    _cache,
                                    carouselButtonLabelTextCacheKey);
                            }

                            if (button.LabelText == null)
                            {
                                button.LabelText = await _carouselButtonLabelTextRepository
                                    .GetByIdsAsync(button.LabelId, currentLangaugeId.Value);

                                await SaveToCacheAsync(_cache,
                                    carouselButtonLabelTextCacheKey,
                                    button.LabelText,
                                    cachePagesInHours);
                            }
                        }

                        if (button.LabelText == null)
                        {
                            var carouselButtonLabelTextCacheKey = string.Format(
                                CultureInfo.InvariantCulture,
                                Utility.Keys.Cache.PromCarouselButtonLabelText,
                                defaultLanguageId,
                                button.LabelId);

                            if (cachePagesInHours != null && !forceReload)
                            {
                                button.LabelText = await GetFromCacheAsync<CarouselButtonLabelText>(
                                    _cache,
                                    carouselButtonLabelTextCacheKey);
                            }

                            if (button.LabelText == null)
                            {
                                button.LabelText = (await _carouselButtonLabelTextRepository
                                    .GetByIdsAsync(button.LabelId, defaultLanguageId));

                                await SaveToCacheAsync(_cache,
                                    carouselButtonLabelTextCacheKey,
                                    button.LabelText,
                                    cachePagesInHours);
                            }
                        }
                    }
                }

                foreach (var item in invalidItems)
                {
                    carousel.Items.Remove(item);
                }
            }

            return carousel;
        }
    }
}
