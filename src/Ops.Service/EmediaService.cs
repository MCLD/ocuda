using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Models;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;
using Slugify;

namespace Ocuda.Ops.Service
{
    public class EmediaService(ILogger<EmediaService> logger,
        IHttpContextAccessor httpContextAccessor,
        IEmediaCategoryRepository emediaCategoryRepository,
        IEmediaGroupRepository emediaGroupRepository,
        IEmediaRepository emediaRepository,
        IEmediaTextRepository emediaTextRepository,
        IEmediaSubjectRepository emediaSubjectRepository,
        ILanguageRepository languageRepository,
        ISegmentService segmentService,
        ISubjectRepository subjectRepository,
        ISubjectTextRepository subjectTextRepository)
            : BaseService<EmediaService>(logger, httpContextAccessor),
            IEmediaService
    {
        private readonly IEmediaCategoryRepository _emediaCategoryRepository
            = emediaCategoryRepository
            ?? throw new ArgumentNullException(nameof(emediaCategoryRepository));

        private readonly IEmediaGroupRepository _emediaGroupRepository = emediaGroupRepository
            ?? throw new ArgumentNullException(nameof(emediaGroupRepository));

        private readonly IEmediaRepository _emediaRepository = emediaRepository
            ?? throw new ArgumentNullException(nameof(emediaRepository));

        private readonly IEmediaSubjectRepository _emediaSubjectRepository = emediaSubjectRepository
            ?? throw new ArgumentNullException(nameof(emediaSubjectRepository));

        private readonly IEmediaTextRepository _emediaTextReposiory = emediaTextRepository
                    ?? throw new ArgumentNullException(nameof(emediaTextRepository));

        private readonly ILanguageRepository _languageRepository = languageRepository
            ?? throw new ArgumentNullException(nameof(languageRepository));

        private readonly ISegmentService _segmentService = segmentService
            ?? throw new ArgumentNullException(nameof(segmentService));

        private readonly ISubjectRepository _subjectRepository = subjectRepository
            ?? throw new ArgumentNullException(nameof(subjectRepository));

        private readonly ISubjectTextRepository _subjectTextRepository = subjectTextRepository
            ?? throw new ArgumentNullException(nameof(subjectTextRepository));

        public async Task AddGroupSegmentAsync(EmediaGroup group)
        {
            ArgumentNullException.ThrowIfNull(group);

            var currentGroup = await _emediaGroupRepository.FindAsync(group.Id);
            currentGroup.Segment = await _segmentService.CreateNoSaveAsync(group.Segment);

            _emediaGroupRepository.Update(currentGroup);
            await _emediaGroupRepository.SaveAsync();
        }

        public async Task<Emedia> CreateAsync(Emedia emedia)
        {
            ArgumentNullException.ThrowIfNull(emedia);

            var currentEmedia = await _emediaRepository.FindAsync(emedia.Slug);

            if (currentEmedia != null)
            {
                throw new OcudaException($"That emedia slug is already in use for: {currentEmedia.Name}");
            }

            emedia.Name = emedia.Name?.Trim();
            emedia.RedirectUrl = emedia.RedirectUrl?.Trim();
            emedia.Slug = emedia.Slug?.Trim();
            emedia.IsActive = true;

            await _emediaRepository.AddAsync(emedia);
            await _emediaRepository.SaveAsync();
            return emedia;
        }

        public async Task<EmediaGroup> CreateGroupAsync(EmediaGroup group)
        {
            ArgumentNullException.ThrowIfNull(group);

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

        public async Task DeleteAsync(int id)
        {
            var emedia = await _emediaRepository.FindAsync(id)
                ?? throw new OcudaException("Emedia does not exist.");

            var emediaCategories = await _emediaCategoryRepository.GetAllForEmediaAsync(emedia.Id);
            _emediaCategoryRepository.RemoveRange(emediaCategories);

            var emediaSubjects = await _emediaSubjectRepository.GetAllForEmediaAsync(emedia.Id);
            _emediaSubjectRepository.RemoveRange(emediaSubjects);

            var emediaTexts = await _emediaTextReposiory.GetAllForEmediaAsync(emedia.Id);
            _emediaTextReposiory.RemoveRange(emediaTexts);

            await _emediaRepository.DeactivateAsync(emedia.Id);

            await _emediaRepository.SaveAsync();
        }

        public async Task DeleteGroupAsync(int id)
        {
            var group = await _emediaGroupRepository.GetIncludingEmediaAsync(id)
                ?? throw new OcudaException("Emedia group does not exist.");

            var subsequentGroups = await _emediaGroupRepository
                .GetSubsequentGroupsAsync(group.SortOrder);

            if (subsequentGroups.Count > 0)
            {
                subsequentGroups.ForEach(_ => _.SortOrder--);
                _emediaGroupRepository.UpdateRange(subsequentGroups);
            }

            if (group.SegmentId.HasValue)
            {
                await _segmentService.DeleteNoSaveAsync(group.SegmentId.Value);
            }

            _emediaRepository.DeactivateRange(group.Emedias, true);

            var emediaCategories = await _emediaCategoryRepository.GetAllForGroupAsync(id);
            _emediaCategoryRepository.RemoveRange(emediaCategories);

            var emediaSubjects = await _emediaSubjectRepository.GetAllForEmediaAsync(id);
            _emediaSubjectRepository.RemoveRange(emediaSubjects);

            var emediaTexts = await _emediaTextReposiory.GetAllForGroupAsync(group.Id);
            _emediaTextReposiory.RemoveRange(emediaTexts);

            _emediaGroupRepository.Remove(group);

            await _emediaRepository.SaveAsync();
        }

        public async Task DeleteGroupSegmentAsync(int groupId)
        {
            var group = await _emediaGroupRepository.FindAsync(groupId);
            if (!group.SegmentId.HasValue)
            {
                throw new OcudaException("Emedia group does not have a segment.");
            }

            await _segmentService.DeleteNoSaveAsync(group.SegmentId.Value);

            group.SegmentId = null;
            _emediaGroupRepository.Update(group);

            await _emediaGroupRepository.SaveAsync();
        }

        public async Task<Emedia> EditAsync(Emedia emedia)
        {
            ArgumentNullException.ThrowIfNull(emedia);

            var currentEmedia = await _emediaRepository.FindAsync(emedia.Slug?.Trim());

            if (currentEmedia != null && currentEmedia.Id != emedia.Id)
            {
                throw new OcudaException($"That emedia slug is already in use for: {currentEmedia.Name}");
            }

            currentEmedia ??= await _emediaRepository.FindAsync(emedia.Id);

            currentEmedia.Name = emedia.Name?.Trim();
            currentEmedia.RedirectUrl = emedia.RedirectUrl?.Trim();
            currentEmedia.Slug = emedia.Slug?.Trim();
            currentEmedia.IsHttpPost = emedia.IsHttpPost;
            currentEmedia.IsAvailableExternally = emedia.IsAvailableExternally;

            _emediaRepository.Update(currentEmedia);
            await _emediaRepository.SaveAsync();
            return currentEmedia;
        }

        public async Task<EmediaGroup> EditGroupAsync(EmediaGroup group)
        {
            ArgumentNullException.ThrowIfNull(group);

            var currentGroup = await _emediaGroupRepository.FindAsync(group.Id);
            currentGroup.Name = group.Name?.Trim();

            _emediaGroupRepository.Update(currentGroup);
            await _emediaGroupRepository.SaveAsync();
            return currentGroup;
        }

        public async Task EnsureSlugsAsync()
        {
            var emedias = await _emediaRepository.GetMissingSlugsAsync();
            if (emedias?.Count > 0)
            {
                var slugHelper = new SlugHelper();
                foreach (var emedia in emedias)
                {
                    await _emediaRepository.ApplySlugAsync(emedia.Key,
                        slugHelper.GenerateSlug(emedia.Value));
                }
            }
        }

        public async Task<IEnumerable<ESourceImport>> ExportItemsAsync(int groupId)
        {
            var defaultLanguageId = await _languageRepository.GetDefaultLanguageId();

            var subjects = await _subjectRepository.GetAllAsync();

            var emedias = await _emediaRepository.GetPaginatedListForGroupAsync(groupId,
                new BaseFilter(1, await _emediaRepository.CountAsync()));

            var esourceExport = new List<ESourceImport>();

            foreach (var emedia in emedias.Data)
            {
                var emediaText = await _emediaTextReposiory
                    .GetByEmediaAndLanguageAsync(emedia.Id, defaultLanguageId);

                var emediaSubjects = await _emediaSubjectRepository.GetAllForEmediaAsync(emedia.Id);

                esourceExport.Add(new ESourceImport
                {
                    Categories = subjects
                        .Where(_ => emediaSubjects.Select(_ => _.SubjectId).Contains(_.Id))
                        .Select(_ => _.Name)
                        .ToArray(),
                    Description = emediaText.Description,
                    InHouseAccess = ESourceAccessLevel.NoLoginRequired,
                    IsHttpPost = emedia.IsHttpPost,
                    Link = emedia.RedirectUrl,
                    Message = emediaText.Details,
                    Name = emedia.Name,
                    RemoteAccess = emedia.IsAvailableExternally
                        ? ESourceAccessLevel.NoLoginRequired
                        : ESourceAccessLevel.LoginRequired
                });
            }

            return esourceExport;
        }

        public async Task<ICollection<Category>> GetCategoriesForEmediaAsync(int emediaId)
        {
            return await _emediaCategoryRepository.GetCategoriesForEmediaAsync(emediaId);
        }

        public async Task<ICollection<string>> GetEmediaLanguagesAsync(int id)
        {
            return await _emediaTextReposiory.GetUsedLanguagesForEmediaAsync(id);
        }

        public async Task<EmediaGroup> GetGroupByIdAsync(int id)
        {
            return await _emediaGroupRepository.FindAsync(id);
        }

        public async Task<EmediaGroup> GetGroupIncludingSegmentAsync(int id)
        {
            return await _emediaGroupRepository.GetIncludingSegmentAsync(id);
        }

        public async Task<EmediaGroup> GetGroupUsingSegmentAsync(int segmentId)
        {
            return await _emediaGroupRepository.GetUsingSegmentAsync(segmentId);
        }

        public async Task<Emedia> GetIncludingGroupAsync(int id)
        {
            return await _emediaRepository.GetIncludingGroupAsync(id);
        }

        public async Task<DataWithCount<ICollection<EmediaGroup>>> GetPaginatedGroupListAsync(
            BaseFilter filter)
        {
            return await _emediaGroupRepository.GetPaginatedListAsync(filter);
        }

        public async Task<DataWithCount<ICollection<Emedia>>> GetPaginatedListForGroupAsync(
            int emediaId,
            BaseFilter filter)
        {
            return await _emediaRepository.GetPaginatedListForGroupAsync(emediaId, filter);
        }

        public async Task<ICollection<Subject>> GetSubjectsForEmediaAsync(int emediaId)
        {
            return await _emediaSubjectRepository.GetSubjectsForEmediaAsync(emediaId);
        }

        public async Task<EmediaText> GetTextByEmediaAndLanguageAsync(int emediaId, int languageId)
        {
            return await _emediaTextReposiory.GetByEmediaAndLanguageAsync(emediaId, languageId);
        }

        public async Task ImportItemsAsync(int groupId, IEnumerable<ESourceImport> importData)
        {
            ArgumentNullException.ThrowIfNull(importData);

            var importSubjects = importData
                .SelectMany(_ => _.Categories, (_, __) => new string(__)).Distinct();

            var defaultLanguageId = await _languageRepository.GetDefaultLanguageId();

            var dbSubjectsDictionary = (await _subjectRepository.GetAllAsync())
                .ToDictionary(k => k.Name, v => v.Id);

            var slugHelper = new SlugHelper();

            foreach (var importSubject in importSubjects)
            {
                var subjectName = importSubject.Trim();
                if (!dbSubjectsDictionary.ContainsKey(subjectName))
                {
                    var slug = await _subjectRepository
                        .GetUnusedSlugAsync(slugHelper.GenerateSlug(subjectName));
                    _logger.LogDebug("Adding subject {Subject} ({Slug})", subjectName, slug);
                    var addSubject = new Subject
                    {
                        Name = subjectName,
                        Slug = slug
                    };

                    await _subjectRepository.AddAsync(addSubject);
                    await _subjectTextRepository.AddAsync(new SubjectText
                    {
                        LanguageId = defaultLanguageId,
                        Text = subjectName,
                        Subject = addSubject
                    });
                    await _subjectRepository.SaveAsync();
                    dbSubjectsDictionary.Add(addSubject.Name, addSubject.Id);
                }
                else
                {
                    _logger.LogInformation("Subject {Subject} already present",
                        importSubject.Trim());
                }
            }

            foreach (var importEsource in importData)
            {
                var name = importEsource.Name.Trim();

                // check if this emedia is already present
                var emedia = await _emediaRepository
                    .FindAsync(name, importEsource.Link.Trim());

                var newEmedia = emedia == null;

                if (newEmedia)
                {
                    var slug = await _emediaRepository
                        .GetUnusedSlugAsync(slugHelper.GenerateSlug(name));

                    _logger.LogDebug("Adding esource {ESource} ({Slug})", name, slug);
                    // not present, create new
                    emedia = new Emedia
                    {
                        GroupId = groupId,
                        IsActive = true,
                        IsHttpPost = importEsource.IsHttpPost,
                        IsAvailableExternally
                            = importEsource.RemoteAccess == ESourceAccessLevel.NoLoginRequired,
                        Name = name,
                        RedirectUrl = importEsource.Link.Trim(),
                        Slug = slug
                    };
                    await _emediaRepository.AddAsync(emedia);

                    var addEmediaText = new EmediaText
                    {
                        Description = importEsource.Description,
                        Details = importEsource.Message,
                        Emedia = emedia,
                        LanguageId = defaultLanguageId
                    };
                    await _emediaTextReposiory.AddAsync(addEmediaText);
                }

                _logger.LogDebug("Adding {Count} subjects to {ESource}",
                    importEsource.Categories.Count,
                    name);

                foreach (var importCategory in importEsource.Categories)
                {
                    var addSubjectMapping = new EmediaSubject
                    {
                        // if it's a new emedia it doesn't have an id yet, let EF map the object
                        Emedia = newEmedia ? emedia : null,
                        EmediaId = newEmedia ? default : emedia.Id,
                        SubjectId = dbSubjectsDictionary[importCategory.Trim()]
                    };

                    await _emediaSubjectRepository.AddAsync(addSubjectMapping);
                }

                await _emediaRepository.SaveAsync();
            }
        }

        public async Task SetEmediaTextAsync(EmediaText emediaText)
        {
            ArgumentNullException.ThrowIfNull(emediaText);

            var currentText = await _emediaTextReposiory
                .GetByEmediaAndLanguageAsync(emediaText.EmediaId, emediaText.LanguageId);

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

        public async Task UpdateCategoriesAsync(int emediaId, ICollection<int> categoryIds)
        {
            var currentCategories = await _emediaCategoryRepository
                .GetCategoryIdsForEmediaAsync(emediaId);

            var categoriesIdsToAdd = categoryIds.Except(currentCategories).ToList();
            var categoriesIdsToRemove = currentCategories.Except(categoryIds).ToList();

            var categoriesToAdd = categoriesIdsToAdd.ConvertAll(_ => new EmediaCategory
            {
                CategoryId = _,
                EmediaId = emediaId
            });

            await _emediaCategoryRepository.AddRangeAsync(categoriesToAdd);
            _emediaCategoryRepository.RemoveByEmediaAndCategories(emediaId, categoriesIdsToRemove);

            await _emediaCategoryRepository.SaveAsync();
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

            var groupInPosition = await _emediaGroupRepository.GetByOrderAsync(newSortOrder)
                ?? throw new OcudaException("Group is already in the last position.");

            groupInPosition.SortOrder = group.SortOrder;
            group.SortOrder = newSortOrder;

            _emediaGroupRepository.Update(group);
            _emediaGroupRepository.Update(groupInPosition);
            await _emediaGroupRepository.SaveAsync();
        }

        public async Task UpdateSubjectsAsync(int emediaId, ICollection<int> subjectIds)
        {
            var currentSubjects = await _emediaSubjectRepository
                .GetSubjectIdsForEmediaAsync(emediaId);

            var subjectIdsToAdd = subjectIds.Except(currentSubjects).ToList();
            var subjectIdsToRemove = currentSubjects.Except(subjectIds).ToList();

            var subjectsToAdd = subjectIdsToAdd.ConvertAll(_ => new EmediaSubject
            {
                SubjectId = _,
                EmediaId = emediaId
            });

            await _emediaSubjectRepository.AddRangeAsync(subjectsToAdd);
            _emediaSubjectRepository.RemoveByEmediaAndSubjects(emediaId, subjectIdsToRemove);

            await _emediaSubjectRepository.SaveAsync();
        }
    }
}