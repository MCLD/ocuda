using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service
{
    public class CategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
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

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.FindAsync(id);
        }

        public async Task<Category> GetByNameAsync(string name)
        {
            return await _categoryRepository.GetByNameAsync(name);
        }

        public async Task<Category> GetByNameAndSectionIdAsync(string name, int sectionId)
        {
            return await _categoryRepository.GetByNameAndSectionIdAsync(name, sectionId);
        }

        public async Task<Category> GetAttachmentCategoryAsync(int sectionId)
        {
            var category = await GetByNameAndSectionIdAsync("Attachments", sectionId);

            if (category == null)
            {
                var newCategory = new Category()
                {
                    SectionId = sectionId,
                    Name = "Attachments",
                    CategoryType = CategoryType.File
                };

                category = await CreateCategoryAsync(newCategory);
            }

            return category;
        }

        public async Task<int> GetCategoryCountAsync()
        {
            return await _categoryRepository.CountAsync();
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            category.CreatedAt = DateTime.Now;
            category.CreatedBy = 1; //TODO fix CreatedBy
            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveAsync();
            return category;
        }

        public async Task<Category> EditCategoryAsync(Category category)
        {
            var currentCategory = await _categoryRepository.FindAsync(category.Id);
            currentCategory.Name = category.Name;

            _categoryRepository.Update(currentCategory);
            await _categoryRepository.SaveAsync();
            return currentCategory;
        }

        public async Task DeleteCategoryAsync(int id)
        {
            _categoryRepository.Remove(id);
            await _categoryRepository.SaveAsync();
        }
    }
}
