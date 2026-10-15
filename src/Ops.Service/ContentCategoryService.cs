using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class ContentCategoryService(
        ICategoryRepository categoryRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<CoverIssueService> logger,
        ISectionService sectionService)
        : BaseService<CoverIssueService>(logger, httpContextAccessor),
        IContentCategoryService
    {
        public async Task AddAsync(Category category)
        {
            var existing = await categoryRepository.GetCategoryBySlugAsync(category.Slug);
            if (existing != null)
            {
                throw new OcudaException("A category with that slug already exists.");
            }

            await categoryRepository.AddAsync(category);
            await categoryRepository.SaveAsync();
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            var categories = await categoryRepository.PageAsync(new BaseFilter
            {
                Skip = 0,
                Take = await categoryRepository.CountAsync(),
            });

            return categories.Data;
        }

        public async Task<int?> GetIdAsync(string categorySlug)
        {
            var category = await categoryRepository.GetCategoryBySlugAsync(categorySlug);
            return category?.Id;
        }

        public async Task<CollectionWithCount<Category>> PageAsync(BaseFilter filter)
        {
            var categoriesWithCount = await categoryRepository.PageAsync(filter);

            foreach (var category in categoriesWithCount.Data)
            {
                var usedBy = await sectionService.GetByCategoryId(category.Id);
                category.UsedInSections.AddRange([.. usedBy]);
            }

            return categoriesWithCount;
        }

        public async Task RemoveAsync(int categoryId)
        {
            categoryRepository.Remove(categoryId);
            await categoryRepository.SaveAsync();
        }
    }
}
