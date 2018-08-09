using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class PageService : IPageService
    {
        private readonly ILogger _logger;
        private IPageRepository _pageRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IUserRepository _userRepository;

        public PageService(ILogger<PageService> logger,
            IPageRepository pageRepository,
            ISectionRepository sectionRepository,
            IUserRepository userRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _pageRepository = pageRepository 
                ?? throw new ArgumentNullException(nameof(pageRepository));
            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<int> GetPageCountAsync()
        {
            return await _pageRepository.CountAsync();
        }

        public async Task<ICollection<Page>> GetPagesAsync(int skip = 0, int take = 5)
        {
            // TODO modify this to do descending (most recent first)
            return await _pageRepository.ToListAsync(skip, take, _ => _.CreatedAt);
        }

        public async Task<Page> GetByIdAsync(int id)
        {
            return await _pageRepository.FindAsync(id);
        }

        public async Task<Page> GetByStubAsync(string stub)
        {
            return await _pageRepository.GetByStubAsync(stub?.Trim().ToLower());
        }

        public async Task<Page> GetByStubAndSectionIdAsync(string stub, int sectionId)
        {
            return await _pageRepository.GetByStubAndSectionIdAsync(stub?.Trim().ToLower(), sectionId);
        }

        public async Task<Page> GetByTitleAndSectionIdAsync(string title, int sectionId)
        {
            return await _pageRepository.GetByTitleAndSectionIdAsync(title?.Trim(), sectionId);
        }

        public async Task<DataWithCount<ICollection<Page>>> GetPaginatedListAsync(BlogFilter filter)
        {
            return await _pageRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Page> CreateAsync(int currentUserId, Page page)
        {
            page.Title = page.Title?.Trim();
            page.Stub = page.Stub?.Trim().ToLower();
            page.CreatedAt = DateTime.Now;
            page.CreatedBy = currentUserId;

            await ValidatePageAsync(page);

            await _pageRepository.AddAsync(page);
            await _pageRepository.SaveAsync();
            return page;
        }

        public async Task<Page> EditAsync(Page page)
        {
            var currentPage = await _pageRepository.FindAsync(page.Id);
            currentPage.Title = page.Title?.Trim();
            currentPage.Stub = page.Stub?.Trim().ToLower();
            currentPage.Content = page.Content;
            currentPage.IsDraft = page.IsDraft;

            await ValidatePageAsync(currentPage);

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
            return await _pageRepository.StubInUseAsync(page);
        }

        public async Task ValidatePageAsync(Page page)
        {
            var message = string.Empty;
            var section = await _sectionRepository.FindAsync(page.SectionId);

            if (section == null)
            {
                message = $"SectionId '{page.SectionId}' is not a valid section.";
                _logger.LogWarning(message, page.SectionId);
                throw new OcudaException(message);
            }

            if (string.IsNullOrWhiteSpace(page.Title))
            {
                message = $"Page name cannot be empty.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            if (string.IsNullOrWhiteSpace(page.Stub))
            {
                message = $"Page stub cannot be empty.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            var stubInUse = await _pageRepository.StubInUseAsync(page);

            if (!page.IsDraft && stubInUse)
            {
                message = $"Stub '{page.Stub}' already exists in '{section.Name}'.";
                _logger.LogWarning(message, page.Title, page.SectionId);
                throw new OcudaException(message);
            }

            var creator = await _userRepository.FindAsync(page.CreatedBy);
            if (creator == null)
            {
                message = $"Created by invalid User Id: {page.CreatedBy}";
                _logger.LogWarning(message, page.CreatedBy);
                throw new OcudaException(message);
            }
        }
    }
}
