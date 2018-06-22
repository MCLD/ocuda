using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service
{
    public class LinkService
    {
        private readonly InsertSampleDataService _insertSampleDataService;
        private readonly ILinkRepository _linkRepository;

        public LinkService(InsertSampleDataService insertSampleDataService,
            ILinkRepository linkRepository)
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

        public async Task<Link> CreateAsync(Link link)
        {
            link.CreatedAt = DateTime.Now;
            // TODO Set CreatedBy Id
            link.CreatedBy = 1;

            await _linkRepository.AddAsync(link);
            await _linkRepository.SaveAsync();
            return link;
        }

        public async Task<Link> EditAsync(Link link)
        {
            // TODO check edit logic
            var currentLink = await _linkRepository.FindAsync(link.Id);
            currentLink.Name = link.Name;
            currentLink.Url = link.Url;

            _linkRepository.Update(currentLink);
            await _linkRepository.SaveAsync();
            return link;
        }

        public async Task DeleteAsync(int id)
        {
            _linkRepository.Remove(id);
            await _linkRepository.SaveAsync();
        }

        public IEnumerable<LinkCategory> GetLinkCategories()
        {
            // TODO repository/database
            return new List<LinkCategory>
            {
                new LinkCategory
                {
                    Id = 1,
                    Name = "Link Category 1",
                },
                new LinkCategory
                {
                    Id = 2,
                    Name = "Link Category 2",
                },
                new LinkCategory
                {
                    Id = 3,
                    Name = "Link Category 3",
                },
            };
        }

        public LinkCategory GetLinkCategoryById(int id)
        {
            // TODO repository/database
            return new LinkCategory
            {
                Id = id,
                Name = $"Category {id}",
            };
        }

        public async Task<LinkCategory> CreateLinkCategoryAsync(LinkCategory linkCategory)
        {
            // TODO repository/database
            // call create method from repository
            return linkCategory;
        }

        public async Task<LinkCategory> EditLinkCategoryAsync(LinkCategory linkCategory)
        {
            // get existing item and update properties that changed
            // call edit method on existing post
            return linkCategory;
        }

        public async Task DeleteLinkCategoryAsync(int id)
        {
            // TODO repository/database
            // call delete method from repository
            throw new NotImplementedException();
        }
    }
}
