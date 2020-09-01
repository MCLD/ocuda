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
        private readonly IPageLayoutRepository _pageLayoutRepository;
        private readonly IPageLayoutTextRepository _pageLayoutTextRepository;
        private readonly IPageRepository _pageRepository;
        private readonly IPermissionGroupPageContentRepository
            _permissionGroupPageContentRepository;

        public PageService(ILogger<PageService> logger,
            IHttpContextAccessor httpContextAccessor,
            IPageHeaderRepository pageHeaderRepository,
            IPageLayoutRepository pageLayoutRepository,
            IPageLayoutTextRepository pageLayoutTextRepository,
            IPageRepository pageRepository,
            IPermissionGroupPageContentRepository permissionGroupPageContentRepository)
            : base(logger, httpContextAccessor)
        {
            _pageHeaderRepository = pageHeaderRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
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

            _pageLayoutRepository.Remove(layout);
            await _pageLayoutRepository.SaveAsync();
        }
    }
}
