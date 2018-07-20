using System;
using System.Collections.Generic;
using System.Text;
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
    public class CategoryService : ICategoryService
    {
        private readonly ILogger<CategoryService> _logger;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISectionRepository _sectionRepository;

        public CategoryService(ILogger<CategoryService> logger,
            ICategoryRepository categoryRepository,
            ISectionRepository sectionRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
        }

        public async Task<DataWithCount<ICollection<Category>>>
           GetPaginatedCategoryListAsync(BlogFilter filter)
        {
            return await _categoryRepository.GetPaginatedListAsync(filter);
        }

        public async Task<ICollection<Category>> GetCategoriesAsync()
        {
            return await _categoryRepository.ToListAsync(_ => _.Name);
        }

        public async Task<ICollection<Category>> GetBySectionIdAsync(BlogFilter filter)
        {
            return await _categoryRepository.GetBySectionIdAsync(filter);
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _categoryRepository.FindAsync(id);
        }

        public async Task<Category> GetByNameAsync(string name)
        {
            return await _categoryRepository.GetByNameAsync(name);
        }

        public async Task<Category> GetByNameAndFilterAsync(string name, BlogFilter filter)
        {
            return await _categoryRepository.GetByNameAndFilterAsync(name, filter);
        }

        public async Task<int> GetCategoryCountAsync()
        {
            return await _categoryRepository.CountAsync();
        }

        public async Task<Category> CreateCategoryAsync(int currentUserId, Category category)
        { 
            category.CreatedAt = DateTime.Now;
            category.CreatedBy = currentUserId;

            await ValidateCategoryAsync(category);

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveAsync();
            return category;
        }

        public async Task<Category> EditCategoryAsync(int id, string name)
        {
            var currentCategory = await _categoryRepository.FindAsync(id);
            currentCategory.Name = name;

            await ValidateCategoryAsync(currentCategory);

            _categoryRepository.Update(currentCategory);
            await _categoryRepository.SaveAsync();
            return currentCategory;
        }

        public async Task DeleteCategoryAsync(int id)
        {
            _categoryRepository.Remove(id);
            await _categoryRepository.SaveAsync();
        }

        public async Task CreateDefaultCategories(int currentUserId, int sectionId)
        {
            var defaultFileCategory = new Category
            {
                CreatedBy = currentUserId,
                CreatedAt = DateTime.Now,
                CategoryType = CategoryType.File,
                IsDefault = true,
                Name = string.Empty,
                SectionId = sectionId
            };

            var defaultLinkCategory = new Category
            {
                CreatedBy = currentUserId,
                CreatedAt = DateTime.Now,
                CategoryType = CategoryType.Link,
                IsDefault = true,
                Name = string.Empty,
                SectionId = sectionId
            };

            await _categoryRepository.AddAsync(defaultFileCategory);
            await _categoryRepository.AddAsync(defaultLinkCategory);
            await _categoryRepository.SaveAsync();
        }

        public async Task<Category> GetDefaultAsync(BlogFilter filter)
        {
            return await _categoryRepository.GetDefaultAsync(filter);
        }

        public async Task<bool> NameExistsAsync(Category category, BlogFilter filter)
        {
            var existingCategory = await GetByNameAndFilterAsync(category.Name, filter);

            if (existingCategory != null)
            {
                return existingCategory.Id != category.Id ? true : false;
            }

            return false;
        }

        private async Task ValidateCategoryAsync(Category category)
        {
            var message = string.Empty;
            var section = await _sectionRepository.FindAsync(category.SectionId);

            if(section == null)
            {
                message = $"SectionId '{category.SectionId}' is not a valid section.";
                _logger.LogWarning(message, category.SectionId);
                throw new OcudaException(message);
            }

            //TODO Category.CategoryType validation?

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                message = $"Category name cannot be empty.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            BlogFilter filter = new BlogFilter
            {
                CategoryType = category.CategoryType,
                SectionId = category.SectionId
            };

            if (await NameExistsAsync(category, filter))
            {
                message = $"Category '{category.Name}' already exists in '{section.Name}'.";
                _logger.LogWarning(message, category.Name, category.SectionId);
                throw new OcudaException(message);
            }
        }
    }
}
