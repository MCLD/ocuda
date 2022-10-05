using System;
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
    public class DeckService : BaseService<DeckService>
    {
        private const string CardsFilePath = "cards";
        private const string ImagesFilePath = "images";

        private readonly ICardDetailRepository _cardDetailRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOcudaCache _cache;
        private readonly IPageLayoutRepository _pageLayoutRepository;
        private readonly IPathResolverService _pathResolver;
        private readonly LanguageService _languageService;

        public DeckService(ILogger<DeckService> logger,
            IDateTimeProvider dateTimeProvider,
            ICardDetailRepository cardDetailRepository,
            ICardRepository cardRepository,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            IOcudaCache cache,
            IPageLayoutRepository pageLayoutRepository,
            IPathResolverService pathResolver,
            LanguageService languageService) : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cardDetailRepository = cardDetailRepository
                ?? throw new ArgumentNullException(nameof(cardDetailRepository));
            _cardRepository = cardRepository
                ?? throw new ArgumentNullException(nameof(cardRepository));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _pageLayoutRepository = pageLayoutRepository
                ?? throw new ArgumentNullException(nameof(pageLayoutRepository));
            _pathResolver = pathResolver
                ?? throw new ArgumentNullException(nameof(pathResolver));
        }

        public async Task<IEnumerable<CardDetail>> GetByIdAsync(int deckId, bool forceReload)
        {
            var cachePagesInHours = GetPageCacheDuration(_config);

            string deckCardIdsCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromDeckCardIds,
                deckId);

            int[] cardIds = null;

            if (cachePagesInHours > 0 && !forceReload)
            {
                cardIds = await _cache.GetObjectFromCacheAsync<int[]>(deckCardIdsCacheKey);
            }

            if (cardIds == null)
            {
                cardIds = await _cardRepository.GetCardIdsByDeckAsync(deckId);

                if (cardIds != null)
                {
                    await _cache.SaveToCacheAsync(deckCardIdsCacheKey, cardIds, cachePagesInHours);
                }
            }

            var details = new List<CardDetail>();

            if (cardIds == null || cardIds.Length == 0)
            {
                return details;
            }

            var languageIds
                = await GetCurrentDefaultLanguageIdAsync(
                    _httpContextAccessor,
                    _languageService);

            // if currentLanguageID came up then cache under that even if we must
            // resort to defaultlanguageid - this may cache spanish under english
            // but if there's no spanish then we are returning english until cache clears
            int cacheLanguageId = languageIds.First();
            string cardDetailCacheKey;

            foreach (var cardId in cardIds)
            {
                cardDetailCacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromCardDetail,
                    cacheLanguageId,
                    cardId);

                CardDetail detail = null;

                if (cachePagesInHours > 0 && !forceReload)
                {
                    detail = await _cache.GetObjectFromCacheAsync<CardDetail>(cardDetailCacheKey);
                }

                if (detail == null)
                {
                    detail = await _cardDetailRepository.GetByIds(cardId, languageIds);

                    if (detail != null)
                    {
                        await _cache.SaveToCacheAsync(cardDetailCacheKey,
                            detail,
                            cachePagesInHours);
                    }
                }

                if (detail != null)
                {
                    var languageName = await _languageService
                        .GetNameAsync(detail.LanguageId, forceReload);

                    if (!string.IsNullOrEmpty(detail.Filename))
                    {
                        detail.ImagePath = _pathResolver.GetPublicContentLink(ImagesFilePath,
                            languageName,
                            CardsFilePath,
                            detail.Filename);
                    }

                    details.Add(detail);
                }
            }

            return details;
        }
    }
}