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

        public CategoryService(ILogger<CategoryService> logger,
            IHttpContextAccessor httpContextAccessor,
            ICategoryRepository categoryRepository)
            : base(logger, httpContextAccessor)
        {
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
        }

        public async Task<ICollection<Category>> GetAllCategories()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public Category GetByClass(string categoryClass)
        {
            return _categoryRepository.GetByClass(categoryClass);
        }

        public async Task AddCategory(Category category)
        {
            category.Name = category.Name.Trim();
            category.Class = category.Class.ToLower().Trim();
            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveAsync();
        }

        public async Task<DataWithCount<ICollection<Category>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _categoryRepository.GetPaginatedListAsync(filter);
        }

        public async Task UpdateCategory(Category category)
        {
            var currentCategory = await _categoryRepository.FindAsync(category.Id);
            currentCategory.Name = category.Name.Trim();
            currentCategory.Class = category.Class.ToLower().Trim();
            _categoryRepository.Update(currentCategory);
            await _categoryRepository.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.FindAsync(id);
                _categoryRepository.Remove(category);
                await _categoryRepository.SaveAsync();
            }
            catch (OcudaException ex)
            {
                _logger.LogError(ex, "Could not delete category", ex.Message);
                throw;
            }
        }
    }
}
