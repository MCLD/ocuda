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
        private readonly ICategoryRepository _categoryRepository;

        public LinkService(InsertSampleDataService insertSampleDataService,
            ILinkRepository linkRepository,
            ICategoryRepository categoryRepository)
        {
            _insertSampleDataService = insertSampleDataService
                ?? throw new ArgumentNullException(nameof(insertSampleDataService));
            _linkRepository = linkRepository
                ?? throw new ArgumentNullException(nameof(linkRepository));
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
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
            return await _categoryRepository.ToListAsync(_ => _.Name);
        }

        public async Task<Category> GetLinkCategoryByIdAsync(int id)
        {
            return await _categoryRepository.FindAsync(id);
        }

        public async Task<int> GetLinkCategoryCountAsync()
        {
            return await _categoryRepository.CountAsync();
        }

        public async Task<Category> CreateLinkCategoryAsync(Category category)
        {
            category.CreatedAt = DateTime.Now;
            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveAsync();
            return category;
        }

        public async Task<Category> EditLinkCategoryAsync(Category category)
        {
            // TODO fix edit logic
            // get existing item and update properties that changed
            // call edit method on existing category
            _categoryRepository.Update(category);
            await _categoryRepository.SaveAsync();
            return category;
        }

        public async Task DeleteLinkCategoryAsync(int id)
        {
            _categoryRepository.Remove(id);
            await _categoryRepository.SaveAsync();
        }
    }
}
