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
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class PageService : BaseService<PageService>, IPageService
    {
        private readonly IPageHeaderRepository _pageHeaderRepository;
        private readonly IPageItemRepository _pageItemRepository;
        private readonly IPageLayoutRepository _pageLayoutRepository;
        private readonly IPageLayoutTextRepository _pageLayoutTextRepository;
        private readonly IPageRepository _pageRepository;
        private readonly IPermissionGroupPageContentRepository
            _permissionGroupPageContentRepository;

        public PageService(ILogger<PageService> logger,
            IHttpContextAccessor httpContextAccessor,
            IPageHeaderRepository pageHeaderRepository,
            IPageItemRepository pageItemRepository,
            IPageLayoutRepository pageLayoutRepository,
            IPageLayoutTextRepository pageLayoutTextRepository,
            IPageRepository pageRepository,
            IPermissionGroupPageContentRepository permissionGroupPageContentRepository)
            : base(logger, httpContextAccessor)
        {
            _pageHeaderRepository = pageHeaderRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
            _pageItemRepository = pageItemRepository
                ?? throw new ArgumentNullException(nameof(pageItemRepository));
            _pageLayoutRepository = pageLayoutRepository
                ?? throw new ArgumentNullException(nameof(pageLayoutRepository));
            _pageLayoutTextRepository = pageLayoutTextRepository
                ?? throw new ArgumentNullException(nameof(pageLayoutTextRepository));
            _pageRepository = pageRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
            _permissionGroupPageContentRepository = permissionGroupPageContentRepository
                ?? throw new ArgumentNullException(nameof(permissionGroupPageContentRepository));
        }

        public async Task<Page> GetByHeaderAndLanguageAsync(int headerId, int languageId)
        {
            return await _pageRepository.GetByHeaderAndLanguageAsync(headerId, languageId);
        }

        public async Task<Page> CreateAsync(Page page)
        {
            page.Title = page.Title?.Trim();
            page.Content = page.Content?.Trim();

            await _pageRepository.AddAsync(page);
            await _pageRepository.SaveAsync();
            return page;
        }

        public async Task<Page> EditAsync(Page page)
        {
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

        public async Task DeleteAsync(Page page)
        {
            _pageRepository.Remove(page);
            await _pageRepository.SaveAsync();
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

        public async Task<PageHeader> GetHeaderByIdAsync(int id)
        {
            return await _pageHeaderRepository.FindAsync(id);
        }

        public async Task<ICollection<string>> GetHeaderLanguagesByIdAsync(int id)
        {
            return await _pageHeaderRepository.GetLanguagesByIdAsync(id);
        }

        public async Task<PageHeader> CreateHeaderAsync(PageHeader header)
        {
            header.Stub = header.Stub?.Trim().ToLower();

            if (await _pageHeaderRepository.StubInUseAsync(header))
            {
                throw new OcudaException($"The stub \"{header.Stub}\" is already in use for that page type. Please choose a different stub.");
            }

            header.PageName = header.PageName?.Trim();

            await _pageHeaderRepository.AddAsync(header);
            await _pageHeaderRepository.SaveAsync();

            return header;
        }

        public async Task<PageHeader> EditHeaderAsync(PageHeader header)
        {
            var currentHeader = await _pageHeaderRepository.FindAsync(header.Id);

            currentHeader.PageName = header.PageName?.Trim();

            _pageHeaderRepository.Update(currentHeader);
            await _pageHeaderRepository.SaveAsync();

            return header;
        }

        public async Task DeleteHeaderAsync(int id)
        {
            var header = await _pageHeaderRepository.FindAsync(id);

            if (header.IsLayoutPage)
            {
                var layoutTexts = await _pageLayoutTextRepository.GetAllForHeaderAsync(header.Id);
                _pageLayoutTextRepository.RemoveRange(layoutTexts);

                var layouts = await _pageLayoutRepository.GetAllForHeaderIncludingChildrenAsync(
                    header.Id);

                var items = layouts.SelectMany(_ => _.Items).ToList();
                _pageItemRepository.RemoveRange(items);

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

        public async Task<bool> StubInUseAsync(PageHeader header)
        {
            header.Stub = header.Stub?.Trim().ToLower();
            return await _pageHeaderRepository.StubInUseAsync(header);
        }

        public async Task<PageLayout> GetLayoutByIdAsync(int id)
        {
            return await _pageLayoutRepository.FindAsync(id);
        }

        public async Task<PageLayout> GetLayoutDetailsAsync(int id)
        {
            var layout = await _pageLayoutRepository.GetIncludingChildrenWithItemContent(id);
            layout.Items = layout.Items.OrderBy(_ => _.Order).ToList();

            return layout;
        }

        public async Task<DataWithCount<ICollection<PageLayout>>>
            GetPaginatedLayoutListForHeaderAsync(int headerId, BaseFilter filter)
        {
            return await _pageLayoutRepository.GetPaginatedListForHeaderAsync(headerId, filter);
        }

        public async Task<PageLayout> CreateLayoutAsync(PageLayout layout)
        {
            layout.Name = layout.Name.Trim();

            await _pageLayoutRepository.AddAsync(layout);
            await _pageLayoutRepository.SaveAsync();

            return layout;
        }

        public async Task<PageLayout> EditLayoutAsync(PageLayout layout)
        {
            var currentlayout = await _pageLayoutRepository.FindAsync(layout.Id);
            currentlayout.Name = layout.Name?.Trim();
            currentlayout.SocialCardId = layout.SocialCardId;
            currentlayout.StartDate = layout.StartDate;

            _pageLayoutRepository.Update(currentlayout);
            await _pageLayoutRepository.SaveAsync();
            return currentlayout;
        }

        public async Task DeleteLayoutAsync(int id)
        {
            var layout = await _pageLayoutRepository.GetIncludingChildrenAsync(id);

            var texts = await _pageLayoutTextRepository.GetAllForLayoutAsync(layout.Id);
            _pageLayoutTextRepository.RemoveRange(texts);

            _pageItemRepository.RemoveRange(layout.Items);
            _pageLayoutRepository.Remove(layout);
            await _pageLayoutRepository.SaveAsync();
        }

        public async Task<PageLayoutText> GetTextByLayoutAndLanguageAsync(int layoutId,
            int languageId)
        {
            return await _pageLayoutTextRepository.GetByPageLayoutAndLanguageAsync(
                layoutId, languageId);
        }

        public async Task<PageLayoutText> SetLayoutTextAsync(PageLayoutText layoutText)
        {
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

                _pageLayoutTextRepository.Update(currentText);
                await _pageLayoutTextRepository.SaveAsync();
                return currentText;
            }
        }

        public async Task<PageItem> CreateItemAsync(PageItem pageItem)
        {
            if (!pageItem.CarouselId.HasValue && !pageItem.SegmentId.HasValue)
            {
                throw new OcudaException("No content selected");
            }

            if (pageItem.CarouselId.HasValue)
            {
                pageItem.SegmentId = null;
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

        public async Task<PageItem> EditItemAsync(PageItem pageItem)
        {
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

        public async Task DeleteItemAsync(int pageItemId)
        {
            var pageItem = await _pageItemRepository
                .FindAsync(pageItemId);

            if (pageItem == null)
            {
                throw new OcudaException("Page item does not exist.");
            }

            var subsequentItems = await _pageItemRepository.GetLayoutSubsequentAsync(
                pageItem.PageLayoutId, pageItem.Order);

            if (subsequentItems.Count > 0)
            {
                subsequentItems.ForEach(_ => _.Order--);
                _pageItemRepository.UpdateRange(subsequentItems);
            }

            _pageItemRepository.Remove(pageItem);
            await _pageItemRepository.SaveAsync();
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
                item.PageLayoutId, newSortOrder);

            if (itemInPosition == null)
            {
                throw new OcudaException("Item is already in the last position.");
            }

            itemInPosition.Order = item.Order;
            item.Order = newSortOrder;

            _pageItemRepository.Update(item);
            _pageItemRepository.Update(itemInPosition);
            await _pageItemRepository.SaveAsync();
        }

        public async Task<int> GetHeaderIdForItemAsync(int itemId)
        {
            return await _pageItemRepository.GetHeaderIdForItemAsync(itemId);
        }
    }
}
