﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class CarouselService : BaseService<CarouselService>
    {
        private readonly IOcudaCache _cache;
        private readonly ICarouselButtonLabelTextRepository _carouselButtonLabelTextRepository;
        private readonly ICarouselItemRepository _carouselItemRepository;
        private readonly ICarouselItemTextRepository _carouselItemTextRepository;
        private readonly ICarouselRepository _carouselRepository;
        private readonly ICarouselTemplateRepository _carouselTemplateRepository;
        private readonly ICarouselTextRepository _carouselTextRepository;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LanguageService _languageService;
        private readonly IPageLayoutRepository _pageLayoutRepository;

        public CarouselService(ILogger<CarouselService> logger,
            IDateTimeProvider dateTimeProvider,
            IOcudaCache cache,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            ICarouselButtonLabelTextRepository carouselButtonLabelTextRepository,
            ICarouselItemRepository carouselItemRepository,
            ICarouselItemTextRepository carouselItemTextRepository,
            ICarouselRepository carouselRepository,
            ICarouselTemplateRepository carouselTemplateRepository,
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
            _carouselTemplateRepository = carouselTemplateRepository
                ?? throw new ArgumentNullException(nameof(carouselTemplateRepository));
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

            if (cachePagesInHours > 0 && !forceReload)
            {
                carousel = await _cache.GetObjectFromCacheAsync<Carousel>(carouselCacheKey);
            }

            if (carousel == null)
            {
                carousel = await _carouselRepository.GetIncludingChildrenAsync(carouselId);

                if (carousel != null)
                {
                    var template = await _carouselTemplateRepository
                        .GetTemplateForCarouselAsync(carousel.Id);

                    carousel.Items = carousel.Items?.OrderBy(_ => _.Order).ToList();
                    foreach (var item in carousel.Items)
                    {
                        item.Carousel = null;
                        item.Buttons = item.Buttons?.OrderBy(_ => _.Order).ToList();

                        foreach (var button in item.Buttons)
                        {
                            button.CarouselItem = null;

                            if (!string.IsNullOrWhiteSpace(template?.ButtonUrlTemplate))
                            {
                                button.Url = template.ButtonUrlTemplate.Replace("{0}",
                                    button.Url,
                                    StringComparison.OrdinalIgnoreCase);
                            }
                        }
                    }
                }

                await _cache.SaveToCacheAsync(carouselCacheKey, carousel, cachePagesInHours);
            }

            if (carousel != null)
            {
                var languageIds = await GetCurrentDefaultLanguageIdAsync(
                        _httpContextAccessor,
                        _languageService);

                // TODO fix this logic

                int? currentLanguageId = languageIds.First();
                var defaultLanguageId = languageIds.Last();

                if (currentLanguageId.HasValue)
                {
                    var carouselTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                        Utility.Keys.Cache.PromCarouselText,
                        currentLanguageId,
                        carousel.Id);

                    if (cachePagesInHours > 0 && !forceReload)
                    {
                        carousel.CarouselText = await _cache.GetObjectFromCacheAsync<CarouselText>(
                            carouselTextCacheKey);
                    }

                    if (carousel.CarouselText == null)
                    {
                        carousel.CarouselText = await _carouselTextRepository.GetByIdsAsync(
                            carousel.Id, currentLanguageId.Value);

                        await _cache.SaveToCacheAsync(carouselTextCacheKey,
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

                    if (cachePagesInHours > 0 && !forceReload)
                    {
                        carousel.CarouselText = await _cache.GetObjectFromCacheAsync<CarouselText>(
                            carouselTextCacheKey);
                    }

                    if (carousel.CarouselText == null)
                    {
                        carousel.CarouselText = await _carouselTextRepository.GetByIdsAsync(
                            carousel.Id, defaultLanguageId);

                        await _cache.SaveToCacheAsync(carouselTextCacheKey,
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

            if (cachePagesInHours > 0 && !forceReload)
            {
                carouselItem = await _cache
                    .GetObjectFromCacheAsync<CarouselItem>(carouselItemCacheKey);
            }

            if (carouselItem == null)
            {
                carouselItem = await _carouselItemRepository.GetForLayoutIncludingChildrenByIdAsync(
                    itemId, currentPageLayout.Value);

                if (carouselItem != null)
                {
                    var template = await _carouselTemplateRepository
                        .GetTemplateForCarouselAsync(carouselItem.CarouselId);

                    carouselItem.Buttons = carouselItem.Buttons?.OrderBy(_ => _.Order).ToList();

                    foreach (var button in carouselItem.Buttons)
                    {
                        button.CarouselItem = null;

                        if (!string.IsNullOrWhiteSpace(template?.ButtonUrlTemplate))
                        {
                            button.Url = template.ButtonUrlTemplate.Replace("{0}",
                                button.Url,
                                StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }

                if (carouselItem != null)
                {
                    await _cache.SaveToCacheAsync(carouselItemCacheKey,
                        carouselItem,
                        cachePagesInHours);
                }
            }

            if (carouselItem != null)
            {
                var languageIds = await GetCurrentDefaultLanguageIdAsync(
                        _httpContextAccessor,
                        _languageService);

                int? currentLanguageId = languageIds.First();
                var defaultLanguageId = languageIds.Last();

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

        private async Task<CarouselButtonLabelText> GetButtonLabelTextAsync(int labelId,
            int defaultLanguageId,
            int? currentLanguageId,
            int cachePagesInHours,
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

                if (cachePagesInHours > 0 && !forceReload)
                {
                    labelText = await _cache.GetObjectFromCacheAsync<CarouselButtonLabelText>(
                        carouselButtonLabelTextCacheKey);
                }

                if (labelText == null)
                {
                    labelText = await _carouselButtonLabelTextRepository
                        .GetByIdsAsync(labelId, currentLanguageId.Value);

                    await _cache.SaveToCacheAsync(carouselButtonLabelTextCacheKey,
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

                if (cachePagesInHours > 0 && !forceReload)
                {
                    labelText = await _cache.GetObjectFromCacheAsync<CarouselButtonLabelText>(
                        carouselButtonLabelTextCacheKey);
                }

                if (labelText == null)
                {
                    labelText = (await _carouselButtonLabelTextRepository
                        .GetByIdsAsync(labelId, defaultLanguageId));

                    await _cache.SaveToCacheAsync(carouselButtonLabelTextCacheKey,
                        labelText,
                        cachePagesInHours);
                }
            }

            return labelText;
        }

        private async Task<CarouselItemText> GetItemTextAsync(int itemId,
                    int defaultLanguageId,
            int? currentLanguageId,
            int cachePagesInHours,
            bool forceReload)
        {
            CarouselItemText itemText = null;

            if (currentLanguageId.HasValue)
            {
                var carouselItemTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromCarouselItemText,
                    currentLanguageId,
                    itemId);

                if (cachePagesInHours > 0 && !forceReload)
                {
                    itemText = await _cache.GetObjectFromCacheAsync<CarouselItemText>(
                        carouselItemTextCacheKey);
                }

                if (itemText == null)
                {
                    itemText = await _carouselItemTextRepository.GetByIdsAsync(
                        itemId, currentLanguageId.Value);

                    await _cache.SaveToCacheAsync(carouselItemTextCacheKey,
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

                if (cachePagesInHours > 0 && !forceReload)
                {
                    itemText = await _cache.GetObjectFromCacheAsync<CarouselItemText>(
                        carouselItemTextCacheKey);
                }

                if (itemText == null)
                {
                    itemText = await _carouselItemTextRepository.GetByIdsAsync(
                        itemId, defaultLanguageId);

                    await _cache.SaveToCacheAsync(carouselItemTextCacheKey,
                        itemText,
                        cachePagesInHours);
                }
            }

            return itemText;
        }
    }
}