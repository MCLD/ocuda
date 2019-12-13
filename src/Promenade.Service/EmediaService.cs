using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class EmediaService : BaseService<EmediaService>
    {
        private readonly IEmediaRepository _emediaRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IEmediaCategoryRepository _emediaCategoryRepository;

        public EmediaService(ILogger<EmediaService> logger,
            IDateTimeProvider dateTimeProvider,
            IEmediaRepository emediaRepository,
            ICategoryRepository categoryRepository,
            IEmediaCategoryRepository emediaCategoryRepository)
            : base(logger, dateTimeProvider)
        {
            _emediaRepository = emediaRepository
                ?? throw new ArgumentNullException(nameof(emediaRepository));
            _emediaCategoryRepository = emediaCategoryRepository
                ?? throw new ArgumentNullException(nameof(emediaCategoryRepository));
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
        }

        public async Task<List<Emedia>> GetAllEmediaAsync()
        {
            var emedia =  await _emediaRepository.GetAllEmedia();
            var categories = await _categoryRepository.GetAllCategories();

            foreach(var media in emedia)
            {
                var categoryList = new List<Category>();
                var categoryIds = await _emediaCategoryRepository.GetEmediaCategoriesByEmediaId(media.Id);
                foreach (var id in categoryIds.Select(_=>_.CategoryId))
                {
                    var category = await _categoryRepository.FindAsync(id);
                    categoryList.Add(category);
                }
                media.Categories = categoryList;
            }
            return emedia;
        }
    }
}
