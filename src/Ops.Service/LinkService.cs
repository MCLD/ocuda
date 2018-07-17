using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service
{
    public class LinkService : ILinkService
    {
        private readonly IInsertSampleDataService _insertSampleDataService;
        private readonly ILinkRepository _linkRepository;

        public LinkService(IInsertSampleDataService insertSampleDataService,
            ILinkRepository linkRepository,
            ICategoryRepository categoryRepository)
        {
            _insertSampleDataService = insertSampleDataService
                ?? throw new ArgumentNullException(nameof(insertSampleDataService));
            _linkRepository = linkRepository
                ?? throw new ArgumentNullException(nameof(linkRepository));
        }
        public async Task<int> GetLinkCountAsync()
        {
            return await _linkRepository.CountAsync();
        }

        public async Task<ICollection<Link>> GetLinksAsync()
        {
            return await _linkRepository.ToListAsync(_ => _.Name);
        }

        public async Task<Link> GetByIdAsync(int id)
        {
            return await _linkRepository.FindAsync(id);
        }

        public async Task<DataWithCount<ICollection<Link>>> GetPaginatedListAsync(BlogFilter filter)
        {
            return await _linkRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Link> CreateAsync(int currentUserId, Link link)
        {
            link.CreatedAt = DateTime.Now;
            link.CreatedBy = currentUserId;

            await _linkRepository.AddAsync(link);
            await _linkRepository.SaveAsync();
            return link;
        }

        public async Task<Link> EditAsync(Link link)
        {
            var currentLink = await _linkRepository.FindAsync(link.Id);
            currentLink.Name = link.Name;
            currentLink.Url = link.Url;
            currentLink.CategoryId = link.CategoryId;
            currentLink.IsFeatured = link.IsFeatured;

            _linkRepository.Update(currentLink);
            await _linkRepository.SaveAsync();
            return link;
        }

        public async Task DeleteAsync(int id)
        {
            _linkRepository.Remove(id);
            await _linkRepository.SaveAsync();
        }
    }
}
