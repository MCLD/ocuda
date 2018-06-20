using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Data.Ops;
using Ocuda.Ops.Models;

namespace Ops.Service
{
    public class LinkService
    {
        private readonly InsertSampleDataService _insertSampleDataService;
        private readonly LinkRepository _linkRepository;

        public LinkService(InsertSampleDataService insertSampleDataService,
            LinkRepository linkRepository)
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
            var links = await _linkRepository.ToListAsync(_ => _.Name);
            if (links == null || links.Count == 0)
            {
                await _insertSampleDataService.InsertLinks();
                links = await _linkRepository.ToListAsync(_ => _.Name);
            }
            return links;
        }

        public async Task<Link> GetLinkByIdAsync(int id)
        {
            return await _linkRepository.FindAsync(id);
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


        public async Task<Link> CreateLinkAsync(Link link)
        {
            link.CreatedAt = DateTime.Now;
            await _linkRepository.AddAsync(link);
            await _linkRepository.SaveAsync();

            return link;
        }

        public async Task<Link> EditLinkAsync(Link link)
        {
            // TODO fix edit logic
            _linkRepository.Update(link);
            await _linkRepository.SaveAsync();

            return link;
        }

        public async Task DeleteLinkAsync(int id)
        {
            _linkRepository.Remove(id);
            await _linkRepository.SaveAsync();
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
