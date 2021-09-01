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

        public async Task<Emedia> GetByIdAsync(int id)
        {
            return await _emediaRepository.FindAsync(id);
        }

        public async Task<Emedia> GetIncludingGroupAsync(int id)
        {
            return await _emediaRepository.GetIncludingGroupAsync(id);
        }

        public async Task<Emedia> CreateAsync(Emedia emedia)
        {
            emedia.Name = emedia.Name?.Trim();
            emedia.RedirectUrl = emedia.RedirectUrl?.Trim();

            await _emediaRepository.AddAsync(emedia);
            await _emediaRepository.SaveAsync();
            return emedia;
        }

        public async Task<Emedia> EditAsync(Emedia emedia)
        {
            var currentEmedia = await _emediaRepository.FindAsync(emedia.Id);

            currentEmedia.Name = emedia.Name?.Trim();
            currentEmedia.RedirectUrl = emedia.RedirectUrl?.Trim();

            _emediaRepository.Update(currentEmedia);
            await _emediaRepository.SaveAsync();
            return currentEmedia;
        }

        public async Task DeleteAsync(int id)
        {
            var emedia = await _emediaRepository.FindAsync(id);

            if (emedia == null)
            {
                throw new OcudaException("Emedia does not exist.");
            }

            var emediaCategories = await _emediaCategoryRepository.GetAllForEmediaAsync(emedia.Id);

            var emediaTexts = await _emediaTextReposiory.GetAllForEmediaAsync(emedia.Id);

            _emediaCategoryRepository.RemoveRange(emediaCategories);
            _emediaTextReposiory.RemoveRange(emediaTexts);
            _emediaRepository.Remove(emedia);

            await _emediaRepository.SaveAsync();
        }

        public async Task SetEmediaTextAsync(EmediaText emediaText)
        {
            var currentText = await _emediaTextReposiory.GetByEmediaAndLanguageAsync(
                emediaText.EmediaId, emediaText.LanguageId);

            if (currentText == null)
            {
                emediaText.Description = emediaText.Description?.Trim();
                emediaText.Details = emediaText.Details?.Trim();

                await _emediaTextReposiory.AddAsync(emediaText);
            }
            else
            {
                currentText.Description = emediaText.Description?.Trim();
                currentText.Details = emediaText.Details?.Trim();

                 _emediaTextReposiory.Update(currentText);
            }

            await _emediaTextReposiory.SaveAsync();
        }

        public async Task<ICollection<string>> GetEmediaLanguagesAsync(int id)
        {
            return await _emediaTextReposiory.GetUsedLanguagesForEmediaAsync(id);
        }

        public async Task<EmediaText> GetTextByEmediaAndLanguageAsync(int emediaId,
            int languageId)
        {
            return await _emediaTextReposiory.GetByEmediaAndLanguageAsync(emediaId, languageId);
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

        public async Task<ICollection<EmediaCategory>>
            GetEmediaCategoriesByCategoryId(int categoryId)
        {
            return await _emediaCategoryRepository.GetByCategoryIdAsync(categoryId);
        }

        public async Task<ICollection<Category>> GetCategoriesForEmediaAsync(int emediaId)
        {
            return await _emediaCategoryRepository.GetCategoriesForEmediaAsync(emediaId);
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

        public async Task UpdateCategoriesAsync(int emediaId, ICollection<int> categoryIds)
        {
            var currentCategories = await _emediaCategoryRepository
                .GetCategoryIdsForEmediaAsync(emediaId);

            var categoriesIdsToAdd = categoryIds.Except(currentCategories).ToList();
            var categoriesIdsToRemove = currentCategories.Except(categoryIds).ToList();

            var categoriesToAdd = categoriesIdsToAdd
                .Select(_ => new EmediaCategory
                {
                    CategoryId = _,
                    EmediaId = emediaId
                })
                .ToList();

            await _emediaCategoryRepository.AddRangeAsync(categoriesToAdd);
            _emediaCategoryRepository.RemoveByEmediaAndCategories(emediaId, categoriesIdsToRemove);

            await _emediaCategoryRepository.SaveAsync();
        }

        public async Task<DataWithCount<ICollection<Emedia>>> GetPaginatedListForGroupAsync(
            int groupId, BaseFilter filter)
        {
            return await _emediaRepository.GetPaginatedListForGroupAsync(groupId, filter);
        }

        public async Task<EmediaGroup> GetGroupByIdAsync(int id)
        {
            return await _emediaGroupRepository.FindAsync(id);
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

        public async Task<EmediaGroup> EditGroupAsync(EmediaGroup group)
        {
            var currentGroup = await _emediaGroupRepository.FindAsync(group.Id);
            currentGroup.Name = group.Name?.Trim();

            _emediaGroupRepository.Update(currentGroup);
            await _emediaGroupRepository.SaveAsync();
            return currentGroup;
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

            var emediaTexts = await _emediaTextReposiory.GetAllForGroupAsync(group.Id);

            _emediaCategoryRepository.RemoveRange(emediaCategories);
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
