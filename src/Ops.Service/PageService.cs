using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service
{
    public class PageService
    {
        private readonly InsertSampleDataService _insertSampleDataService;
        private IPageRepository _pageRepository;

        public PageService(IPageRepository pageRepository, InsertSampleDataService insertSampleDataService)
        {
            _pageRepository = pageRepository 
                ?? throw new ArgumentNullException(nameof(pageRepository));
            _insertSampleDataService = insertSampleDataService
                ?? throw new ArgumentNullException(nameof(insertSampleDataService));
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
            return await _pageRepository.GetByStubAsync(stub);
        }

        public async Task<DataWithCount<ICollection<Page>>> GetPaginatedListAsync(BlogFilter filter)
        {
            return await _pageRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Page> CreateAsync(Page page)
        {
            page.CreatedAt = DateTime.Now;
            // TODO Set CreatedBy Id
            page.CreatedBy = 1;

            await _pageRepository.AddAsync(page);
            await _pageRepository.SaveAsync();
            return page;
        }

        public async Task<Page> EditAsync(Page page)
        {
            // TODO check edit logic
            var currentPage = await _pageRepository.FindAsync(page.Id);
            currentPage.Title = page.Title;
            currentPage.Content = page.Content;

            _pageRepository.Update(currentPage);
            await _pageRepository.SaveAsync();
            return page;
        }

        public async Task DeleteAsync(int id)
        {
            _pageRepository.Remove(id);
            await _pageRepository.SaveAsync();
        }
    }
}
