using System;
using System.Collections.Generic;
using System.Linq;
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
    public class EmediaService : BaseService<EmediaService>, IEmediaService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IEmediaRepository _emediaRepository;
        private readonly IEmediaCategoryRepository _emediaCategoryRepository;
        private readonly IEmediaGroupRepository _emediaGroupRepository;
        private readonly IEmediaTextRepository _emediaTextReposiory;

        public EmediaService(ILogger<EmediaService> logger,
            IHttpContextAccessor httpContextAccessor,
            ICategoryRepository categoryRepository,
            IEmediaRepository emediaRepository,
            IEmediaCategoryRepository emediaCategoryRepository,
            IEmediaGroupRepository emediaGroupRepository,
            IEmediaTextRepository emediaTextRepository)
            : base(logger, httpContextAccessor)
        {
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
            _emediaRepository = emediaRepository
                ?? throw new ArgumentNullException(nameof(emediaRepository));
            _emediaCategoryRepository = emediaCategoryRepository
                ?? throw new ArgumentNullException(nameof(emediaCategoryRepository));
            _emediaGroupRepository = emediaGroupRepository
                ?? throw new ArgumentNullException(nameof(emediaGroupRepository));
            _emediaTextReposiory = emediaTextRepository
                ?? throw new ArgumentNullException(nameof(emediaTextRepository));
        }

        public async Task<ICollection<Emedia>> GetAllEmedia()
        {
            var emedias = (await _emediaRepository.GetAllAsync()).ToList();
            var emediaCats = await _emediaCategoryRepository.GetAllAsync();
            foreach (var emedia in emedias)
            {
                var emediaCategories = new List<Category>();
                foreach (var emediacat in emediaCats.Where(_ => _.EmediaId == emedia.Id))
                {
                    var category = await _categoryRepository.FindAsync(emediacat.CategoryId);
                    emediaCategories.Add(category);
                }
                emedia.Categories = emediaCategories;
            }
            return emedias;
        }

        public async Task<Emedia> GetByStubAsync(string emediaStub)
        {
            var emedia = _emediaRepository.GetByStub(emediaStub);
            if (emedia != null)
            {
                var emediaCategories = new List<Category>();
                foreach (var emediacat in await _emediaCategoryRepository.GetByEmediaIdAsync(emedia.Id))
                {
                    var category = await _categoryRepository.FindAsync(emediacat.CategoryId);
                    emediaCategories.Add(category);
                }
                emedia.Categories = emediaCategories;
            }
            return emedia;
        }

        public async Task<ICollection<EmediaCategory>> GetEmediaCategoriesById(int emediaId)
        {
            return await _emediaCategoryRepository.GetByEmediaIdAsync(emediaId);
        }

        public async Task<ICollection<EmediaCategory>>
            GetEmediaCategoriesByCategoryId(int categoryId)
        {
            return await _emediaCategoryRepository.GetByCategoryIdAsync(categoryId);
        }

        public async Task AddEmediaCategory(EmediaCategory emediaCategory)
        {
            if (emediaCategory == null)
            {
                throw new ArgumentNullException(nameof(emediaCategory));
            }
            await _emediaCategoryRepository.AddAsync(emediaCategory);
            await _emediaCategoryRepository.SaveAsync();
        }

        public async Task UpdateEmediaCategoryAsync(List<int> newCategoryIds, int emediaId)
        {
            var currentCats = await _emediaCategoryRepository.GetByEmediaIdAsync(emediaId);
            var currentCatIds = currentCats.Select(_ => _.CategoryId).ToList();

            var addCategoryIds = newCategoryIds ?? new List<int>();

            var catsToDelete = currentCatIds.Except(addCategoryIds).ToList();
            foreach (var category in addCategoryIds.Except(currentCatIds).ToList())
            {
                var emediaCategory = new EmediaCategory
                {
                    EmediaId = emediaId,
                    CategoryId = category
                };
                await _emediaCategoryRepository.AddAsync(emediaCategory);
            }
            foreach (var category in catsToDelete)
            {
                var emediaCat
                    = _emediaCategoryRepository.GetByEmediaAndCategoryId(emediaId, category);
                _emediaCategoryRepository.Remove(emediaCat);
            }
            await _emediaCategoryRepository.SaveAsync();
        }

        public async Task<DataWithCount<ICollection<Emedia>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _emediaRepository.GetPaginatedListAsync(filter);
        }

        public async Task AddEmedia(Emedia emedia)
        {
            emedia.RedirectUrl = emedia.RedirectUrl.Trim();
            emedia.Name = emedia.Name.Trim();
            await _emediaRepository.AddAsync(emedia);
            await _emediaRepository.SaveAsync();
        }

        public async Task UpdateEmedia(Emedia emedia)
        {
            var currentEmedia = await _emediaRepository.FindAsync(emedia.Id);
            currentEmedia.RedirectUrl = emedia.RedirectUrl.Trim();
            currentEmedia.Name = emedia.Name.Trim();
            _emediaRepository.Update(currentEmedia);
            await _emediaRepository.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var emedia = await _emediaRepository.FindAsync(id);
                _emediaRepository.Remove(emedia);
                await _emediaRepository.SaveAsync();
            }
            catch (OcudaException ex)
            {
                _logger.LogError(ex, "Could not delete emedia", ex.Message);
                throw;
            }
        }

        public async Task<DataWithCount<ICollection<EmediaGroup>>> GetPaginatedGroupListAsync(
            BaseFilter filter)
        {
            return await _emediaGroupRepository.GetPaginatedListAsync(filter);
        }

        public async Task<EmediaGroup> CreateGroupAsync(EmediaGroup group)
        {
            group.Name = group.Name?.Trim();

            var maxSortOrder = await _emediaGroupRepository.GetMaxSortOrderAsync();
            if (maxSortOrder.HasValue)
            {
                group.SortOrder = maxSortOrder.Value + 1;
            }
            else
            {
                group.SortOrder = 0;
            }

            await _emediaGroupRepository.AddAsync(group);
            await _emediaGroupRepository.SaveAsync();
            return group;
        }

        public async Task DeleteGroupAsync(int id)
        {
            var group = await _emediaGroupRepository.GetIncludingChildredAsync(id);

            if (group == null)
            {
                throw new OcudaException("Emedia group does not exist.");
            }

            var subsequentGroups = await _emediaGroupRepository
                .GetSubsequentGroupsAsync(group.SortOrder);

            if (subsequentGroups.Count > 0)
            {
                subsequentGroups.ForEach(_ => _.SortOrder--);
                _emediaGroupRepository.UpdateRange(subsequentGroups);
            }

            var emediaCategories = await _emediaCategoryRepository.GetAllForGroupAsync(id);
            _emediaCategoryRepository.RemoveRange(emediaCategories);

            var emediaTexts = await _emediaTextReposiory.GetAllForGroupAsync(group.Id);
            _emediaTextReposiory.RemoveRange(emediaTexts);

            _emediaRepository.RemoveRange(group.Emedias);

            _emediaGroupRepository.Remove(group);

            await _emediaGroupRepository.SaveAsync();
        }

        public async Task UpdateGroupSortOrder(int id, bool increase)
        {
            var group = await _emediaGroupRepository.FindAsync(id);

            int newSortOrder;
            if (increase)
            {
                newSortOrder = group.SortOrder + 1;
            }
            else
            {
                if (group.SortOrder == 0)
                {
                    throw new OcudaException("Group is already in the first position.");
                }
                newSortOrder = group.SortOrder - 1;
            }

            var groupInPosition = await _emediaGroupRepository.GetByOrderAsync(newSortOrder);

            if (groupInPosition == null)
            {
                throw new OcudaException("Group is already in the last position.");
            }

            groupInPosition.SortOrder = group.SortOrder;
            group.SortOrder = newSortOrder;

            _emediaGroupRepository.Update(group);
            _emediaGroupRepository.Update(groupInPosition);
            await _emediaGroupRepository.SaveAsync();
        }
    }
}
