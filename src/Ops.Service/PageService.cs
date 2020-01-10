using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class PageService : BaseService<PageService>, IPageService
    {
        private readonly IPageHeaderRepository _pageHeaderRepository;
        private readonly IPageRepository _pageRepository;

        public PageService(ILogger<PageService> logger,
            IHttpContextAccessor httpContextAccessor,
            IPageHeaderRepository pageHeaderRepository,
            IPageRepository pageRepository)
            : base(logger, httpContextAccessor)
        {
            _pageHeaderRepository = pageHeaderRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
            _pageRepository = pageRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
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
            BaseFilter filter)
        {
            return await _pageHeaderRepository.GetPaginatedListAsync(filter);
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
            var pages = await _pageRepository.GetByHeaderIdAsync(header.Id);

            _pageRepository.RemoveRange(pages);
            _pageHeaderRepository.Remove(header);
            await _pageHeaderRepository.SaveAsync();
        }

        public async Task<bool> StubInUseAsync(PageHeader header)
        {
            header.Stub = header.Stub?.Trim().ToLower();
            return await _pageHeaderRepository.StubInUseAsync(header);
        }
    }
}
