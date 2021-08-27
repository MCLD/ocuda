using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class CategoryService : BaseService<CategoryService>, ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryTextRepository _categoryTextRepository;
        private readonly IEmediaCategoryRepository _emediaCategoryRepository;

        public CategoryService(ILogger<CategoryService> logger,
            IHttpContextAccessor httpContextAccessor,
            ICategoryRepository categoryRepository,
            ICategoryTextRepository categoryTextRepository,
            IEmediaCategoryRepository emediaCategoryRepository)
            : base(logger, httpContextAccessor)
        {
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
            _categoryTextRepository = categoryTextRepository
                ?? throw new ArgumentNullException(nameof(categoryTextRepository));
            _emediaCategoryRepository = emediaCategoryRepository
                ?? throw new ArgumentNullException(nameof(emediaCategoryRepository));
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _categoryRepository.FindAsync(id);
        }

        public async Task<ICollection<Category>> GetAllAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }


        public async Task<DataWithCount<ICollection<Category>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _categoryRepository.GetPaginatedListAsync(filter);
        }

        public async Task<ICollection<string>> GetCategoryLanguagesAsync(int id)
        {
            return await _categoryTextRepository.GetUsedLanguagesForCategoryAsync(id);
        }

        public async Task<Category> CreateAsync(Category category)
        {
            category.Class = category.Class?.ToLowerInvariant().Trim();
            category.Name = category.Name?.Trim();

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveAsync();

            return category;
        }

        public async Task<Category> EditAsync(Category category)
        {
            var currentCategory = await _categoryRepository.FindAsync(category.Id);

            currentCategory.Class = category.Class?.ToLowerInvariant().Trim();
            currentCategory.Name = category.Name?.Trim();

            _categoryRepository.Update(currentCategory);
            await _categoryRepository.SaveAsync();

            return currentCategory;
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.FindAsync(id);

            if (category == null)
            {
                throw new OcudaException("Category does not exist.");
            }

            var categoryTexts = await _categoryTextRepository.GetAllForCategoryAsync(category.Id);

            var emediaCategories = await _emediaCategoryRepository
                .GetByCategoryIdAsync(category.Id);

            _categoryTextRepository.RemoveRange(categoryTexts);
            _emediaCategoryRepository.RemoveRange(emediaCategories);
            _categoryRepository.Remove(category);

            await _categoryRepository.SaveAsync();
        }

        public async Task<CategoryText> GetTextByCategoryAndLanguageAsync(int categoryId,
            int languageId)
        {
            return await _categoryTextRepository.GetByCategoryAndLanguageAsync(categoryId,
                languageId);
        }
    }
}
