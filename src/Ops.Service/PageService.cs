using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class PageService : BaseService<PageService>, IPageService
    {
        private const string IndexStub = "index";
        private readonly ICarouselService _carouselService;
        private readonly IDeckService _deckService;
        private readonly IImageFeatureService _imageFeatureService;
        private readonly ILanguageService _languageService;
        private readonly INavBannerService _navBannerService;
        private readonly IPageHeaderRepository _pageHeaderRepository;
        private readonly IPageItemRepository _pageItemRepository;
        private readonly IPageLayoutRepository _pageLayoutRepository;
        private readonly IPageLayoutTextRepository _pageLayoutTextRepository;
        private readonly IPageRepository _pageRepository;

        private readonly IPermissionGroupPageContentRepository
            _permissionGroupPageContentRepository;

        private readonly ISegmentService _segmentService;

        public PageService(ILogger<PageService> logger,
            ICarouselService carouselService,
            IDeckService deckService,
            IHttpContextAccessor httpContextAccessor,
            IImageFeatureService imageFeatureService,
            ILanguageService languageService,
            INavBannerService navBannerService,
            IPageHeaderRepository pageHeaderRepository,
            IPageItemRepository pageItemRepository,
            IPageLayoutRepository pageLayoutRepository,
            IPageLayoutTextRepository pageLayoutTextRepository,
            IPageRepository pageRepository,
            IPermissionGroupPageContentRepository permissionGroupPageContentRepository,
            ISegmentService segmentService)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(carouselService);
            ArgumentNullException.ThrowIfNull(deckService);
            ArgumentNullException.ThrowIfNull(imageFeatureService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(navBannerService);
            ArgumentNullException.ThrowIfNull(pageHeaderRepository);
            ArgumentNullException.ThrowIfNull(pageItemRepository);
            ArgumentNullException.ThrowIfNull(pageLayoutRepository);
            ArgumentNullException.ThrowIfNull(pageLayoutTextRepository);
            ArgumentNullException.ThrowIfNull(pageRepository);
            ArgumentNullException.ThrowIfNull(permissionGroupPageContentRepository);
            ArgumentNullException.ThrowIfNull(segmentService);

            _carouselService = carouselService;
            _deckService = deckService;
            _imageFeatureService = imageFeatureService;
            _languageService = languageService;
            _navBannerService = navBannerService;
            _pageHeaderRepository = pageHeaderRepository;
            _pageItemRepository = pageItemRepository;
            _pageLayoutRepository = pageLayoutRepository;
            _pageLayoutTextRepository = pageLayoutTextRepository;
            _pageRepository = pageRepository;
            _permissionGroupPageContentRepository = permissionGroupPageContentRepository;
            _segmentService = segmentService;
        }

        public async Task<PageLayout> CloneLayoutAsync(int pageHeaderId,
            int layoutId,
            string clonedName)
        {
            var layout = await GetLayoutDetailsAsync(layoutId);

            if (layout.PageHeaderId != pageHeaderId)
            {
                throw new OcudaException("Requested layout does not match page header id.");
            }

            var newLayout = await CreateLayoutAsync(new PageLayout
            {
                Name = clonedName?.Trim(),
                PageHeaderId = layout.PageHeaderId,
                SocialCardId = layout.SocialCardId
            });

            var pageLayouts = await _pageLayoutTextRepository.GetAllForLayoutAsync(layout.Id);

            foreach (var pageLayout in pageLayouts)
            {
                await _pageLayoutTextRepository.AddAsync(new PageLayoutText
                {
                    IsTitleHidden = pageLayout.IsTitleHidden,
                    LanguageId = pageLayout.LanguageId,
                    PageLayoutId = newLayout.Id,
                    Title = pageLayout.Title
                });
            }

            _logger.LogDebug("Layout {NewLayoutId}: created {ClonedName} based on id {LayoutId}",
                newLayout.Id,
                clonedName?.Trim(),
                layoutId);

            foreach (var item in layout.Items.OrderBy(_ => _.Order))
            {
                if (item.SegmentId.HasValue)
                {
                    var newItem = await CreateItemAsync(new PageItem
                    {
                        Order = item.Order,
                        PageLayoutId = newLayout.Id,
                        Segment = new Segment
                        {
                            EndDate = item.Segment.EndDate,
                            IsActive = item.Segment.IsActive,
                            Name = item.Segment.Name,
                            SegmentLanguages = item.Segment.SegmentLanguages,
                            StartDate = item.Segment.StartDate
                        }
                    });

                    var languageIds = new List<int>();

                    foreach (var language in await _languageService.GetActiveAsync())
                    {
                        var segmentText = await _segmentService
                            .GetBySegmentAndLanguageAsync(item.Segment.Id, language.Id);

                        if (segmentText != null)
                        {
                            await _segmentService.CreateSegmentTextAsync(new SegmentText
                            {
                                Header = segmentText.Header,
                                LanguageId = segmentText.LanguageId,
                                SegmentId = (int)newItem.SegmentId,
                                Text = segmentText.Text
                            });

                            languageIds.Add(language.Id);
                        }
                    }

                    _logger.LogDebug("Layout {NewLayoutId}: created item id {ItemId}, segment id {NewSegmentId} (copy of {SegmentId}) for languages {LanguageIds}",
                        newLayout.Id,
                        newItem.Id,
                        newItem.SegmentId,
                        item.SegmentId,
                        languageIds);
                }
                else if (item.PageFeatureId.HasValue || item.WebslideId.HasValue || item.BannerFeatureId.HasValue)
                {
                    var newItem = await ConnectImageFeatureAsync(new PageItem
                    {
                        BannerFeatureId = item.BannerFeatureId,
                        Order = item.Order,
                        PageLayoutId = newLayout.Id,
                        PageFeatureId = item.PageFeatureId,
                        WebslideId = item.WebslideId
                    });

                    _logger.LogDebug("Layout {NewLayoutId}: created item id {ItemId}, linked to {ImageFeatureType} {ImageFeatureId}",
                        newLayout.Id,
                        newItem.Id,
                        newItem.BannerFeatureId.HasValue ? "banner feature"
                            : newItem.PageFeatureId.HasValue ? "page feature" : "Web slide",
                        newItem.BannerFeatureId
                            ?? newItem.PageFeatureId
                            ?? newItem.WebslideId.Value);
                }
                else if (item.DeckId.HasValue)
                {
                    var newItem = await CreateItemAsync(new PageItem
                    {
                        Order = item.Order,
                        PageLayoutId = newLayout.Id,
                        Deck = new Deck
                        {
                            Name = item.Deck.Name
                        }
                    });

                    (var cardCount, var cardDetailCount) = await _deckService
                        .CloneAsync(item.DeckId.Value, newItem.Deck.Id);

                    _logger.LogDebug("Layout {NewLayoutId}: cloned deck to new deck id {ItemId}: {CardCount} cards, {CardDetailCount} card details",
                        newLayout.Id,
                        newItem.Id,
                        cardCount,
                        cardDetailCount);
                }
                else if (item.CarouselId.HasValue)
                {
                    var newCarousel = await _carouselService.CloneAsync((int)item.CarouselId);

                    var newItem = await CreateItemAsync(new PageItem
                    {
                        Order = item.Order,
                        PageLayoutId = newLayout.Id,
                        Carousel = newCarousel
                    });

                    _logger.LogDebug("Layout {NewLayoutId}: cloned carousel to new carousel id {ItemId}",
                        newLayout.Id,
                        newItem.Id);
                }
            }

            return newLayout;
        }

        public async Task<PageItem> ConnectImageFeatureAsync(PageItem pageItem)
        {
            if (pageItem == null)
            {
                throw new OcudaException("Cannot connect empty object");
            }

            var maxSortOrder = await _pageItemRepository
                .GetMaxSortOrderForLayoutAsync(pageItem.PageLayoutId);
            if (maxSortOrder.HasValue)
            {
                pageItem.Order = maxSortOrder.Value + 1;
            }

            await _pageItemRepository.AddAsync(pageItem);
            await _pageItemRepository.SaveAsync();
            return pageItem;
        }

        public async Task<Page> CreateAsync(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);

            page.Title = page.Title?.Trim();
            page.Content = page.Content?.Trim();

            await _pageRepository.AddAsync(page);
            await _pageRepository.SaveAsync();
            return page;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization",
            "CA1308:Normalize strings to uppercase",
            Justification = "Stubs are part of a URI and are normalized to lowercase")]
        public async Task<PageHeader> CreateHeaderAsync(PageHeader header)
        {
            ArgumentNullException.ThrowIfNull(header);

            header.Stub = header.Stub?.Trim().ToLowerInvariant();

            if (await _pageHeaderRepository.StubInUseAsync(header))
            {
                throw new OcudaException($"The stub \"{header.Stub}\" is already in use for that page type. Please choose a different stub.");
            }

            header.PageName = header.PageName?.Trim();

            await _pageHeaderRepository.AddAsync(header);
            await _pageHeaderRepository.SaveAsync();

            return header;
        }

        public async Task<PageItem> CreateItemAsync(PageItem pageItem)
        {
            if (pageItem == null)
            {
                throw new OcudaException("No type selected");
            }

            pageItem.BannerFeatureId = null;
            pageItem.CarouselId = null;
            pageItem.DeckId = null;
            pageItem.NavBannerId = null;
            pageItem.PageFeatureId = null;
            pageItem.SegmentId = null;
            pageItem.WebslideId = null;

            ImageFeature bannerFeature = null;
            Carousel carousel = null;
            Deck deck = null;
            ImageFeature pageFeature = null;
            NavBanner navBanner = null;
            Segment segment = null;
            ImageFeature webslide = null;

            if (pageItem.BannerFeature != null)
            {
                bannerFeature = await _imageFeatureService
                    .CreateNoSaveAsync(pageItem.BannerFeature);
            }
            else if (pageItem.Carousel != null)
            {
                if (pageItem.Carousel.Id == 0)
                {
                    pageItem.Carousel.CarouselText ??= new CarouselText
                    {
                        Title = pageItem.Carousel.Name
                    };
                    carousel = await _carouselService.CreateNoSaveAsync(pageItem.Carousel);
                }
                else
                {
                    carousel = pageItem.Carousel;
                }
            }
            else if (pageItem.Deck != null)
            {
                deck = await _deckService.CreateNoSaveAsync(pageItem.Deck);
            }
            else if (pageItem.NavBanner != null)
            {
                navBanner = await _navBannerService.CreateNoSaveAsync(pageItem.NavBanner);
            }
            else if (pageItem.PageFeature != null)
            {
                pageFeature = await _imageFeatureService.CreateNoSaveAsync(pageItem.PageFeature);
            }
            else if (pageItem.Segment != null)
            {
                segment = await _segmentService.CreateNoSaveAsync(pageItem.Segment);
            }
            else if (pageItem.Webslide != null)
            {
                webslide = await _imageFeatureService.CreateNoSaveAsync(pageItem.Webslide);
            }

            pageItem.BannerFeature = bannerFeature;
            pageItem.Carousel = carousel;
            pageItem.Deck = deck;
            pageItem.NavBanner = navBanner;
            pageItem.PageFeature = pageFeature;
            pageItem.Segment = segment;
            pageItem.Webslide = webslide;

            if (pageItem.BannerFeature == null
                && pageItem.Carousel == null
                && pageItem.Deck == null
                && pageItem.NavBanner == null
                && pageItem.PageFeature == null
                && pageItem.Segment == null
                && pageItem.Webslide == null)
            {
                throw new OcudaException("No type selected");
            }

            var maxSortOrder = await _pageItemRepository.GetMaxSortOrderForLayoutAsync(
                pageItem.PageLayoutId);
            if (maxSortOrder.HasValue)
            {
                pageItem.Order = maxSortOrder.Value + 1;
            }

            await _pageItemRepository.AddAsync(pageItem);
            await _pageItemRepository.SaveAsync();
            return pageItem;
        }

        public async Task<PageLayout> CreateLayoutAsync(PageLayout layout)
        {
            ArgumentNullException.ThrowIfNull(layout);

            layout.Name = layout.Name.Trim();
            layout.PreviewId = Guid.NewGuid();

            await _pageLayoutRepository.AddAsync(layout);
            await _pageLayoutRepository.SaveAsync();

            return layout;
        }

        public async Task DeleteAsync(Page page)
        {
            _pageRepository.Remove(page);
            await _pageRepository.SaveAsync();
        }

        public async Task DeleteHeaderAsync(int id)
        {
            var header = await _pageHeaderRepository.FindAsync(id);

            if (header.Type == PageType.Home
                && header.Stub.Equals(IndexStub, StringComparison.OrdinalIgnoreCase))
            {
                throw new OcudaException($"Unable to delete {header.Type} section {header.Stub} page.");
            }

            if (header.IsLayoutPage)
            {
                var layoutTexts = await _pageLayoutTextRepository.GetAllForHeaderAsync(header.Id);
                _pageLayoutTextRepository.RemoveRange(layoutTexts);

                var layouts = await _pageLayoutRepository.GetAllForHeaderIncludingChildrenAsync(
                    header.Id);

                foreach (var layout in layouts)
                {
                    foreach (var item in layout.Items)
                    {
                        await DeleteItemNoSaveAsync(item.Id, true);
                    }
                    layout.Items = null;
                }

                _pageLayoutRepository.RemoveRange(layouts);
            }
            else
            {
                var pages = await _pageRepository.GetByHeaderIdAsync(header.Id);
                _pageRepository.RemoveRange(pages);
            }

            _pageHeaderRepository.Remove(header);
            await _pageHeaderRepository.SaveAsync();
        }

        public async Task DeleteItemAsync(int pageItemId)
        {
            await DeleteItemNoSaveAsync(pageItemId);
            await _pageItemRepository.SaveAsync();
        }

        public async Task DeleteItemNoSaveAsync(int pageItemId, bool ignoreSort = false)
        {
            var pageItem = await _pageItemRepository.FindAsync(pageItemId)
                ?? throw new OcudaException("Page item does not exist.");

            if (pageItem.CarouselId.HasValue)
            {
                await _carouselService.DeleteNoSaveAsync(pageItem.CarouselId.Value);
            }
            if (pageItem.DeckId.HasValue)
            {
                await _deckService.DeleteDeckNoSaveAsync(pageItem.DeckId.Value);
            }
            if (pageItem.SegmentId.HasValue)
            {
                await _segmentService.DeleteNoSaveAsync(pageItem.SegmentId.Value);
            }
            if (pageItem.BannerFeatureId.HasValue || pageItem.PageFeatureId.HasValue || pageItem.WebslideId.HasValue)
            {
                var imageFeatureId = pageItem.BannerFeatureId ?? pageItem.PageFeatureId ?? pageItem.WebslideId;
                var usage = await _pageItemRepository
                    .GetImageFeatureUseCountAsync(imageFeatureId.Value);
                if (usage <= 1)
                {
                    await _imageFeatureService.DeleteNoSaveAsync(imageFeatureId.Value);
                }
            }

            if (!ignoreSort)
            {
                var subsequentItems = await _pageItemRepository.GetLayoutSubsequentAsync(
                    pageItem.PageLayoutId,
                    pageItem.Order);

                if (subsequentItems.Count > 0)
                {
                    subsequentItems.ForEach(_ => _.Order--);
                    _pageItemRepository.UpdateRange(subsequentItems);
                }
            }

            _pageItemRepository.Remove(pageItem);
        }

        public async Task DeleteLayoutAsync(int id)
        {
            var layout = await _pageLayoutRepository.GetIncludingChildrenAsync(id);

            var texts = await _pageLayoutTextRepository.GetAllForLayoutAsync(layout.Id);
            _pageLayoutTextRepository.RemoveRange(texts);

            foreach (var item in layout.Items)
            {
                await DeleteItemNoSaveAsync(item.Id, true);
            }
            layout.Items = null;

            _pageLayoutRepository.Remove(layout);
            await _pageLayoutRepository.SaveAsync();
        }

        public async Task<Page> EditAsync(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);

            var currentPage = await _pageRepository.GetByHeaderAndLanguageAsync(
                page.PageHeaderId, page.LanguageId);
            currentPage.Title = page.Title?.Trim();
            currentPage.Content = page.Content?.Trim();
            currentPage.IsPublished = page.IsPublished;
            currentPage.SocialCardId = page.SocialCardId;

            _pageRepository.Update(currentPage);
            await _pageRepository.SaveAsync();
            return currentPage;
        }

        public async Task<PageHeader> EditHeaderAsync(PageHeader header)
        {
            ArgumentNullException.ThrowIfNull(header);

            var currentHeader = await _pageHeaderRepository.FindAsync(header.Id);

            currentHeader.PageName = header.PageName?.Trim();

            _pageHeaderRepository.Update(currentHeader);
            await _pageHeaderRepository.SaveAsync();

            return header;
        }

        public async Task<PageItem> EditItemAsync(PageItem pageItem)
        {
            ArgumentNullException.ThrowIfNull(pageItem);

            if (!pageItem.CarouselId.HasValue && !pageItem.SegmentId.HasValue)
            {
                throw new OcudaException("No content selected");
            }

            var currentPageItem = await _pageItemRepository.FindAsync(pageItem.Id);
            if (pageItem.CarouselId.HasValue)
            {
                currentPageItem.CarouselId = pageItem.CarouselId;
                currentPageItem.SegmentId = null;
            }
            else
            {
                currentPageItem.SegmentId = pageItem.SegmentId;
            }

            _pageItemRepository.Update(currentPageItem);
            await _pageItemRepository.SaveAsync();
            return currentPageItem;
        }

        public async Task<PageLayout> EditLayoutAsync(PageLayout layout)
        {
            ArgumentNullException.ThrowIfNull(layout);

            var currentlayout = await _pageLayoutRepository.FindAsync(layout.Id);
            currentlayout.Name = layout.Name?.Trim();
            currentlayout.SocialCardId = layout.SocialCardId;
            currentlayout.StartDate = layout.StartDate;

            _pageLayoutRepository.Update(currentlayout);
            await _pageLayoutRepository.SaveAsync();
            return currentlayout;
        }

        public async Task<Page> GetByHeaderAndLanguageAsync(int headerId, int languageId)
        {
            return await _pageRepository.GetByHeaderAndLanguageAsync(headerId, languageId);
        }

        public async Task<PageHeader> GetHeaderByIdAsync(int id)
        {
            return await _pageHeaderRepository.FindAsync(id);
        }

        public async Task<PageItem> GetItemByIdAsync(int id)
        {
            return await _pageItemRepository.FindAsync(id);
        }

        public async Task<PageLayout> GetLayoutByIdAsync(int id)
        {
            return await _pageLayoutRepository.FindAsync(id);
        }

        public async Task<PageLayout> GetLayoutDetailsAsync(int id)
        {
            var layout = await _pageLayoutRepository.GetIncludingChildrenWithItemContent(id);
            layout.Items = layout.Items?.OrderBy(_ => _.Order).ToList();

            return layout;
        }

        public async Task<PageLayout> GetLayoutForItemAsync(int itemId)
        {
            return await _pageItemRepository.GetLayoutForItemAsync(itemId);
        }

        public async Task<DataWithCount<ICollection<PageHeader>>> GetPaginatedHeaderListAsync(
                                                            PageFilter filter)
        {
            var headerList = await _pageHeaderRepository.GetPaginatedListAsync(filter);
            foreach (var header in headerList.Data)
            {
                var perms
                    = await _permissionGroupPageContentRepository.GetByPageHeaderId(header.Id);
                header.PermissionGroupIds = perms.Select(_ => _.PermissionGroupId
                    .ToString(CultureInfo.InvariantCulture));
            }
            return headerList;
        }

        public async Task<DataWithCount<ICollection<PageLayout>>>
            GetPaginatedLayoutListForHeaderAsync(int headerId, BaseFilter filter)
        {
            return await _pageLayoutRepository.GetPaginatedListForHeaderAsync(headerId, filter);
        }

        public async Task<PageLayoutText> GetTextByLayoutAndLanguageAsync(int layoutId,
            int languageId)
        {
            return await _pageLayoutTextRepository.GetByPageLayoutAndLanguageAsync(
                layoutId, languageId);
        }

        public async Task<PageLayoutText> SetLayoutTextAsync(PageLayoutText layoutText)
        {
            ArgumentNullException.ThrowIfNull(layoutText);

            var currentText = await _pageLayoutTextRepository.GetByPageLayoutAndLanguageAsync(
                layoutText.PageLayoutId, layoutText.LanguageId);

            if (currentText == null)
            {
                layoutText.Title = layoutText.Title?.Trim();

                await _pageLayoutTextRepository.AddAsync(layoutText);
                await _pageLayoutTextRepository.SaveAsync();
                return layoutText;
            }
            else
            {
                currentText.Title = layoutText.Title?.Trim();
                currentText.IsTitleHidden = layoutText.IsTitleHidden;

                _pageLayoutTextRepository.Update(currentText);
                await _pageLayoutTextRepository.SaveAsync();
                return currentText;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization",
            "CA1308:Normalize strings to uppercase",
            Justification = "Stubs are part of a URI and are normalized to lowercase")]
        public async Task<bool> StubInUseAsync(PageHeader header)
        {
            ArgumentNullException.ThrowIfNull(header);

            header.Stub = header.Stub?.Trim().ToLowerInvariant();
            return await _pageHeaderRepository.StubInUseAsync(header);
        }

        public async Task UpdateItemSortOrder(int id, bool increase)
        {
            var item = await _pageItemRepository.FindAsync(id);

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

            var itemInPosition = await _pageItemRepository.GetByLayoutAndOrderAsync(
                item.PageLayoutId, newSortOrder)
                ?? throw new OcudaException("Item is already in the last position.");

            itemInPosition.Order = item.Order;
            item.Order = newSortOrder;

            _pageItemRepository.Update(item);
            _pageItemRepository.Update(itemInPosition);
            await _pageItemRepository.SaveAsync();
        }
    }
}