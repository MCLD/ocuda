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
        private readonly ICarouselItemRepository _carouselItemRepository;
        private readonly ICarouselItemTextRepository _carouselItemTextRepository;
        private readonly ICarouselRepository _carouselRepository;
        private readonly ICarouselTextRepository _carouselTextRepository;
        private readonly IPageLayoutRepository _pageLayoutRepository;
        private readonly LanguageService _languageService;

        public CarouselService(ILogger<CarouselService> logger,
            IDateTimeProvider dateTimeProvider,
            IDistributedCache cache,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            ICarouselButtonLabelTextRepository carouselButtonLabelTextRepository,
            ICarouselItemRepository carouselItemRepository,
            ICarouselItemTextRepository carouselItemTextRepository,
            ICarouselRepository carouselRepository,
            ICarouselTextRepository carouselTextRepository,
            IPageLayoutRepository pageLayoutRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _carouselButtonLabelTextRepository = carouselButtonLabelTextRepository
                ?? throw new ArgumentNullException(nameof(carouselButtonLabelTextRepository));
            _carouselItemRepository = carouselItemRepository
                ?? throw new ArgumentNullException(nameof(carouselItemRepository));
            _carouselItemTextRepository = carouselItemTextRepository
                ?? throw new ArgumentNullException(nameof(carouselItemTextRepository));
            _carouselRepository = carouselRepository
                ?? throw new ArgumentNullException(nameof(carouselRepository));
            _carouselTextRepository = carouselTextRepository
                ?? throw new ArgumentNullException(nameof(carouselTextRepository));
            _pageLayoutRepository = pageLayoutRepository
                ?? throw new ArgumentNullException(nameof(pageLayoutRepository));
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
                int? currentLanguageId = null;
                if (!string.IsNullOrWhiteSpace(currentCultureName))
                {
                    currentLanguageId = await _languageService.GetLanguageIdAsync(
                        currentCultureName);
                }

                if (currentLanguageId.HasValue)
                {
                    var carouselTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                        Utility.Keys.Cache.PromCarouselText,
                        currentLanguageId,
                        carousel.Id);

                    if (cachePagesInHours != null && !forceReload)
                    {
                        carousel.CarouselText = await GetFromCacheAsync<CarouselText>(_cache,
                            carouselTextCacheKey);
                    }

                    if (carousel.CarouselText == null)
                    {
                        carousel.CarouselText = await _carouselTextRepository.GetByIdsAsync(
                            carousel.Id, currentLanguageId.Value);

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
                    item.CarouselItemText = await GetItemTextAsync(item.Id,
                        defaultLanguageId,
                        currentLanguageId,
                        cachePagesInHours,
                        forceReload);

                    if (item.CarouselItemText == null)
                    {
                        invalidItems.Add(item);
                        continue;
                    }

                    foreach (var button in item.Buttons)
                    {
                        button.LabelText = await GetButtonLabelTextAsync(button.LabelId,
                        defaultLanguageId,
                        currentLanguageId,
                        cachePagesInHours,
                        forceReload);
                    }
                }

                foreach (var item in invalidItems)
                {
                    carousel.Items.Remove(item);
                }
            }

            return carousel;
        }

        public async Task<CarouselItem> GetItemForHeaderAsync(int headerId, int itemId,
            bool forceReload)
        {
            var currentPageLayout = await _pageLayoutRepository.GetCurrentLayoutIdForHeaderAsync(
                headerId);

            if (!currentPageLayout.HasValue)
            {
                return null;
            }

            CarouselItem carouselItem = null;

            var cachePagesInHours = GetPageCacheDuration(_config);
            string carouselItemCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromCarouselItemForPageLayout,
                itemId,
                currentPageLayout.Value);

            if (cachePagesInHours != null && !forceReload)
            {
                carouselItem = await GetFromCacheAsync<CarouselItem>(_cache, carouselItemCacheKey);
            }

            if (carouselItem == null)
            {
                carouselItem = await _carouselItemRepository.GetForLayoutIncludingChildrenByIdAsync(
                    itemId, currentPageLayout.Value);

                if (carouselItem != null)
                {
                    carouselItem.Buttons = carouselItem.Buttons?.OrderBy(_ => _.Order).ToList();

                    foreach (var button in carouselItem.Buttons)
                    {
                        button.CarouselItem = null;
                    }
                }

                await SaveToCacheAsync(_cache, carouselItemCacheKey, carouselItem,
                    cachePagesInHours);
            }

            if (carouselItem != null)
            {
                var currentCultureName = _httpContextAccessor
                    .HttpContext
                    .Features
                    .Get<IRequestCultureFeature>()
                    .RequestCulture
                    .UICulture?
                    .Name;

                int defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
                int? currentLanguageId = null;
                if (!string.IsNullOrWhiteSpace(currentCultureName))
                {
                    currentLanguageId = await _languageService.GetLanguageIdAsync(
                        currentCultureName);
                }

                carouselItem.CarouselItemText = await GetItemTextAsync(carouselItem.Id,
                    defaultLanguageId,
                    currentLanguageId,
                    cachePagesInHours,
                    forceReload);

                if (carouselItem.CarouselItemText == null)
                {
                    return null;
                }

                foreach (var button in carouselItem.Buttons)
                {
                    button.LabelText = await GetButtonLabelTextAsync(button.LabelId,
                        defaultLanguageId,
                        currentLanguageId,
                        cachePagesInHours,
                        forceReload);
                }
            }

            return carouselItem;
        }

        private async Task<CarouselItemText> GetItemTextAsync(int itemId,
            int defaultLanguageId,
            int? currentLanguageId,
            int? cachePagesInHours,
            bool forceReload)
        {
            CarouselItemText itemText = null;

            if (currentLanguageId.HasValue)
            {
                var carouselItemTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromCarouselItemText,
                    currentLanguageId,
                    itemId);

                if (cachePagesInHours != null && !forceReload)
                {
                    itemText = await GetFromCacheAsync<CarouselItemText>(
                        _cache,
                        carouselItemTextCacheKey);
                }

                if (itemText == null)
                {
                    itemText = await _carouselItemTextRepository.GetByIdsAsync(
                        itemId, currentLanguageId.Value);

                    await SaveToCacheAsync(_cache,
                        carouselItemTextCacheKey,
                        itemText,
                        cachePagesInHours);
                }
            }

            if (itemText == null)
            {
                var carouselItemTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromCarouselItemText,
                    defaultLanguageId,
                    itemId);

                if (cachePagesInHours != null && !forceReload)
                {
                    itemText = await GetFromCacheAsync<CarouselItemText>(
                        _cache,
                        carouselItemTextCacheKey);
                }

                if (itemText == null)
                {
                    itemText = await _carouselItemTextRepository.GetByIdsAsync(
                        itemId, defaultLanguageId);

                    await SaveToCacheAsync(_cache,
                        carouselItemTextCacheKey,
                        itemText,
                        cachePagesInHours);
                }
            }

            return itemText;
        }

        private async Task<CarouselButtonLabelText> GetButtonLabelTextAsync(int labelId,
            int defaultLanguageId,
            int? currentLanguageId,
            int? cachePagesInHours,
            bool forceReload)
        {
            CarouselButtonLabelText labelText = null;

            if (currentLanguageId.HasValue)
            {
                var carouselButtonLabelTextCacheKey = string.Format(
                    CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromCarouselButtonLabelText,
                    currentLanguageId,
                    labelId);

                if (cachePagesInHours != null && !forceReload)
                {
                    labelText = await GetFromCacheAsync<CarouselButtonLabelText>(
                        _cache,
                        carouselButtonLabelTextCacheKey);
                }

                if (labelText == null)
                {
                    labelText = await _carouselButtonLabelTextRepository
                        .GetByIdsAsync(labelId, currentLanguageId.Value);

                    await SaveToCacheAsync(_cache,
                        carouselButtonLabelTextCacheKey,
                        labelText,
                        cachePagesInHours);
                }
            }

            if (labelText == null)
            {
                var carouselButtonLabelTextCacheKey = string.Format(
                    CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromCarouselButtonLabelText,
                    defaultLanguageId,
                    labelId);

                if (cachePagesInHours != null && !forceReload)
                {
                    labelText = await GetFromCacheAsync<CarouselButtonLabelText>(
                        _cache,
                        carouselButtonLabelTextCacheKey);
                }

                if (labelText == null)
                {
                    labelText = (await _carouselButtonLabelTextRepository
                        .GetByIdsAsync(labelId, defaultLanguageId));

                    await SaveToCacheAsync(_cache,
                        carouselButtonLabelTextCacheKey,
                        labelText,
                        cachePagesInHours);
                }
            }

            return labelText;
        }
    }
}
