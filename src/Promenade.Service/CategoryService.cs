using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class CategoryService(ILogger<CategoryService> logger,
        IDateTimeProvider dateTimeProvider,
        ICategoryRepository categoryRepository)
            : BaseService<CategoryService>(logger, dateTimeProvider)
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));

        public async Task<List<Category>> GetAllAsync()
        {
            return await _categoryRepository.GetAllCategories();
        }
    }
}
