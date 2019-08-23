using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service
{
    public class PageService : IPageService
    {
        private readonly ILogger<PageService> _logger;
        private readonly IPageRepository _pageRepository;

        public PageService(ILogger<PageService> logger,
            IPageRepository pageRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _pageRepository = pageRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
        }

        public async Task<DataWithCount<ICollection<Page>>> GetPaginatedListAsync(BaseFilter filter)
        {
            return await _pageRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Page> GetByIdAsync(int id)
        {
            return await _pageRepository.FindAsync(id);
        }

        public async Task<Page> CreateAsync(Page page)
        {
            page.Content = page.Content?.Trim();
            page.Stub = page.Stub?.Trim().ToLower();

            await _pageRepository.AddAsync(page);
            await _pageRepository.SaveAsync();
            return page;
        }

        public async Task<Page> EditAsync(Page page, bool publish = false)
        {
            var currentPage = await _pageRepository.FindAsync(page.Id);
            currentPage.Content = page.Content?.Trim();
            currentPage.SocialCardId = page.SocialCardId;

            if (!currentPage.IsPublished)
            {
                currentPage.IsPublished = publish;
                currentPage.Type = page.Type;
                currentPage.Stub = page.Stub?.Trim().ToLower();
            }

            _pageRepository.Update(currentPage);
            await _pageRepository.SaveAsync();
            return page;
        }

        public async Task DeleteAsync(int id)
        {
            _pageRepository.Remove(id);
            await _pageRepository.SaveAsync();
        }

        public async Task<bool> StubInUseAsync(Page page)
        {
            page.Stub = page.Stub?.Trim().ToLower();
            return await _pageRepository.StubInUseAsync(page);
        }
    }
}
