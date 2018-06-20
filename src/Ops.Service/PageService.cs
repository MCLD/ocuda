using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Interfaces.Ops;

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

        public async Task<Page> GetPageByIdAsync(int id)
        {
            return await _pageRepository.FindAsync(id);
        }

        public async Task<Page> GetPageByStubAsync(string stub)
        {
            //TODO add repository method
            throw new NotImplementedException();
        }

        public async Task<Page> CreatePageAsync(Page page)
        {
            page.CreatedAt = DateTime.Now;
            await _pageRepository.AddAsync(page);
            await _pageRepository.SaveAsync();

            return page;
        }

        public async Task<Page> EditPageAsync(Page page)
        {
            // TODO fix edit logic
            var currentPage = await _pageRepository.FindAsync(page.Id);
            currentPage.Title = page.Title;
            currentPage.Content = page.Content;

            _pageRepository.Update(currentPage);
            await _pageRepository.SaveAsync();

            return page;
        }

        public async Task DeletePageAsync(int id)
        {
            _pageRepository.Remove(id);
            await _pageRepository.SaveAsync();
        }
    }
}
