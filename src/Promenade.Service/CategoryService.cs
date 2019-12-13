using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class CategoryService : BaseService<CategoryService>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ILogger<CategoryService> logger,
            IDateTimeProvider dateTimeProvider,
            ICategoryRepository categoryRepository)
            : base(logger, dateTimeProvider)
        {
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
        }

        public async Task<List<Category>> GetAllLocationsAsync()
        {
            return await _categoryRepository.GetAllCategories();
        }
    }
}
