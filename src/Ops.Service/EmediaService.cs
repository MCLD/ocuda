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
        public async Task AddGroupSegmentAsync(EmediaGroup group)
        {
            ArgumentNullException.ThrowIfNull(group);

            var currentGroup = await emediaGroupRepository.FindAsync(group.Id);
            currentGroup.Segment = await segmentService.CreateNoSaveAsync(group.Segment);

            emediaGroupRepository.Update(currentGroup);
            await emediaGroupRepository.SaveAsync();
        }

        public async Task<Emedia> CreateAsync(Emedia emedia)
        {
            ArgumentNullException.ThrowIfNull(emedia);

            var currentEmedia = await emediaRepository.FindAsync(emedia.Slug);

            if (currentEmedia != null)
            {
                throw new OcudaException($"That emedia slug is already in use for: {currentEmedia.Name}");
            }

            emedia.Name = emedia.Name?.Trim();
            emedia.RedirectUrl = emedia.RedirectUrl?.Trim();
            emedia.SortAs = emedia.SortAs?.Trim() ?? emedia.Name?.Trim();
            emedia.Slug = emedia.Slug?.Trim();
            emedia.IsActive = true;

            await emediaRepository.AddAsync(emedia);
            await emediaRepository.SaveAsync();
            return emedia;
        }

        public async Task<EmediaGroup> CreateGroupAsync(EmediaGroup group)
        {
            ArgumentNullException.ThrowIfNull(group);

            group.Name = group.Name?.Trim();

            var maxSortOrder = await emediaGroupRepository.GetMaxSortOrderAsync();
            group.SortOrder = maxSortOrder.HasValue ? maxSortOrder.Value + 1 : 0;

            await emediaGroupRepository.AddAsync(group);
            await emediaGroupRepository.SaveAsync();
            return group;
        }

        public async Task DeleteAsync(int id)
        {
            var emedia = await emediaRepository.FindAsync(id)
                ?? throw new OcudaException("Emedia does not exist.");

            var emediaCategories = await emediaCategoryRepository.GetAllForEmediaAsync(emedia.Id);
            emediaCategoryRepository.RemoveRange(emediaCategories);

            var emediaSubjects = await emediaSubjectRepository.GetAllForEmediaAsync(emedia.Id);
            emediaSubjectRepository.RemoveRange(emediaSubjects);

            var emediaTexts = await emediaTextRepository.GetAllForEmediaAsync(emedia.Id);
            emediaTextRepository.RemoveRange(emediaTexts);

            await emediaRepository.DeactivateAsync(emedia.Id);

            await emediaRepository.SaveAsync();
        }

        public async Task DeleteGroupAsync(int id)
        {
            var group = await emediaGroupRepository.GetIncludingEmediaAsync(id)
                ?? throw new OcudaException("Emedia group does not exist.");

            var subsequentGroups = await emediaGroupRepository
                .GetSubsequentGroupsAsync(group.SortOrder);

            if (subsequentGroups.Count > 0)
            {
                subsequentGroups.ForEach(_ => _.SortOrder--);
                emediaGroupRepository.UpdateRange(subsequentGroups);
            }

            if (group.SegmentId.HasValue)
            {
                await segmentService.DeleteNoSaveAsync(group.SegmentId.Value);
            }

            emediaRepository.DeactivateRange(group.Emedias, true);

            var emediaCategories = await emediaCategoryRepository.GetAllForGroupAsync(id);
            emediaCategoryRepository.RemoveRange(emediaCategories);

            var emediaSubjects = await emediaSubjectRepository.GetAllForEmediaAsync(id);
            emediaSubjectRepository.RemoveRange(emediaSubjects);

            var emediaTexts = await emediaTextRepository.GetAllForGroupAsync(group.Id);
            emediaTextRepository.RemoveRange(emediaTexts);

            emediaGroupRepository.Remove(group);

            await emediaRepository.SaveAsync();
        }

        public async Task DeleteGroupSegmentAsync(int groupId)
        {
            var group = await emediaGroupRepository.FindAsync(groupId);
            if (!group.SegmentId.HasValue)
            {
                throw new OcudaException("Emedia group does not have a segment.");
            }

            await segmentService.DeleteNoSaveAsync(group.SegmentId.Value);

            group.SegmentId = null;
            emediaGroupRepository.Update(group);

            await emediaGroupRepository.SaveAsync();
        }

        public async Task<Emedia> EditAsync(Emedia emedia)
        {
            ArgumentNullException.ThrowIfNull(emedia);

            var currentEmedia = await emediaRepository.FindAsync(emedia.Slug?.Trim());

            if (currentEmedia != null && currentEmedia.Id != emedia.Id)
            {
                throw new OcudaException($"That emedia slug is already in use for: {currentEmedia.Name}");
            }

            currentEmedia ??= await emediaRepository.FindAsync(emedia.Id);

            currentEmedia.Name = emedia.Name?.Trim();
            currentEmedia.RedirectUrl = emedia.RedirectUrl?.Trim();
            currentEmedia.Slug = emedia.Slug?.Trim();
            currentEmedia.IsHttpPost = emedia.IsHttpPost;
            currentEmedia.IsAvailableExternally = emedia.IsAvailableExternally;

            emediaRepository.Update(currentEmedia);
            await emediaRepository.SaveAsync();
            return currentEmedia;
        }

        public async Task<EmediaGroup> EditGroupAsync(EmediaGroup group)
        {
            ArgumentNullException.ThrowIfNull(group);

            var currentGroup = await emediaGroupRepository.FindAsync(group.Id);
            currentGroup.Name = group.Name?.Trim();

            emediaGroupRepository.Update(currentGroup);
            await emediaGroupRepository.SaveAsync();
            return currentGroup;
        }

        public async Task EnsureSlugsAsync()
        {
            var emedias = await emediaRepository.GetMissingSlugsAsync();
            if (emedias?.Count > 0)
            {
                var slugHelper = new SlugHelper();
                foreach (var emedia in emedias)
                {
                    await emediaRepository.ApplySlugAsync(emedia.Key,
                        slugHelper.GenerateSlug(emedia.Value));
                }
            }
        }

        public async Task<IEnumerable<ESourceImport>> ExportItemsAsync(int groupId)
        {
            var defaultLanguageId = await languageRepository.GetDefaultLanguageId();

            var subjects = await subjectRepository.GetAllAsync();

            var emedias = await emediaRepository.GetPaginatedListForGroupAsync(groupId,
                new BaseFilter(1, await emediaRepository.CountAsync()));

            var esourceExport = new List<ESourceImport>();

            foreach (var emedia in emedias.Data)
            {
                var emediaText = await emediaTextRepository
                    .GetByEmediaAndLanguageAsync(emedia.Id, defaultLanguageId);

                var emediaSubjects = await emediaSubjectRepository.GetAllForEmediaAsync(emedia.Id);

                esourceExport.Add(new ESourceImport
                {
                    Categories = [.. subjects
                        .Where(_ => emediaSubjects.Select(_ => _.SubjectId).Contains(_.Id))
                        .Select(_ => _.Name)],
                    Description = emediaText.Description,
                    InHouseAccess = ESourceAccessLevel.NoLoginRequired,
                    IsHttpPost = emedia.IsHttpPost,
                    Link = emedia.RedirectUrl,
                    Message = emediaText.Details,
                    Name = emedia.Name,
                    RemoteAccess = emedia.IsAvailableExternally
                        ? ESourceAccessLevel.NoLoginRequired
                        : ESourceAccessLevel.LoginRequired,
                });
            }

            return esourceExport;
        }

        public async Task<ICollection<Category>> GetCategoriesForEmediaAsync(int emediaId)
        {
            return await emediaCategoryRepository.GetCategoriesForEmediaAsync(emediaId);
        }

        public async Task<ICollection<string>> GetEmediaLanguagesAsync(int id)
        {
            return await emediaTextRepository.GetUsedLanguagesForEmediaAsync(id);
        }

        public async Task<EmediaGroup> GetGroupByIdAsync(int id)
        {
            return await emediaGroupRepository.FindAsync(id);
        }

        public async Task<EmediaGroup> GetGroupIncludingSegmentAsync(int id)
        {
            return await emediaGroupRepository.GetIncludingSegmentAsync(id);
        }

        public async Task<EmediaGroup> GetGroupUsingSegmentAsync(int segmentId)
        {
            return await emediaGroupRepository.GetUsingSegmentAsync(segmentId);
        }

        public async Task<Emedia> GetIncludingGroupAsync(int id)
        {
            return await emediaRepository.GetIncludingGroupAsync(id);
        }

        public async Task<DataWithCount<ICollection<EmediaGroup>>> GetPaginatedGroupListAsync(
            BaseFilter filter)
        {
            return await emediaGroupRepository.GetPaginatedListAsync(filter);
        }

        public async Task<DataWithCount<ICollection<Emedia>>> GetPaginatedListForGroupAsync(
            int emediaId,
            BaseFilter filter)
        {
            return await emediaRepository.GetPaginatedListForGroupAsync(emediaId, filter);
        }

        public async Task<ICollection<Subject>> GetSubjectsForEmediaAsync(int emediaId)
        {
            return await emediaSubjectRepository.GetSubjectsForEmediaAsync(emediaId);
        }

        public async Task<EmediaText> GetTextByEmediaAndLanguageAsync(int emediaId, int languageId)
        {
            return await emediaTextRepository.GetByEmediaAndLanguageAsync(emediaId, languageId);
        }

        public async Task ImportItemsAsync(int groupId, IEnumerable<ESourceImport> importData)
        {
            ArgumentNullException.ThrowIfNull(importData);

            var importSubjects = importData
                .SelectMany(_ => _.Categories, (_, __) => new string(__)).Distinct();

            var defaultLanguageId = await languageRepository.GetDefaultLanguageId();

            var dbSubjectsDictionary = (await subjectRepository.GetAllAsync())
                .ToDictionary(k => k.Name, v => v.Id);

            var slugHelper = new SlugHelper();

            foreach (var importSubject in importSubjects)
            {
                var subjectName = importSubject.Trim();
                if (!dbSubjectsDictionary.ContainsKey(subjectName))
                {
                    var slug = await subjectRepository
                        .GetUnusedSlugAsync(slugHelper.GenerateSlug(subjectName));
                    _logger.LogDebug("Adding subject {Subject} ({Slug})", subjectName, slug);
                    var addSubject = new Subject
                    {
                        Name = subjectName,
                        Slug = slug,
                    };

                    await subjectRepository.AddAsync(addSubject);
                    await subjectTextRepository.AddAsync(new SubjectText
                    {
                        LanguageId = defaultLanguageId,
                        Text = subjectName,
                        Subject = addSubject,
                    });
                    await subjectRepository.SaveAsync();
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
                var emedia = await emediaRepository
                    .FindAsync(name, importEsource.Link.Trim());

                var newEmedia = emedia == null;

                if (newEmedia)
                {
                    var slug = await emediaRepository
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
                        Slug = slug,
                    };
                    await emediaRepository.AddAsync(emedia);

                    var addEmediaText = new EmediaText
                    {
                        Description = importEsource.Description,
                        Details = importEsource.Message,
                        Emedia = emedia,
                        LanguageId = defaultLanguageId,
                    };
                    await emediaTextRepository.AddAsync(addEmediaText);
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
                        SubjectId = dbSubjectsDictionary[importCategory.Trim()],
                    };

                    await emediaSubjectRepository.AddAsync(addSubjectMapping);
                }

                await emediaRepository.SaveAsync();
            }
        }

        public async Task SetEmediaTextAsync(EmediaText emediaText)
        {
            ArgumentNullException.ThrowIfNull(emediaText);

            var currentText = await emediaTextRepository
                .GetByEmediaAndLanguageAsync(emediaText.EmediaId, emediaText.LanguageId);

            if (currentText == null)
            {
                emediaText.Description = emediaText.Description?.Trim();
                emediaText.Details = emediaText.Details?.Trim();

                await emediaTextRepository.AddAsync(emediaText);
            }
            else
            {
                currentText.Description = emediaText.Description?.Trim();
                currentText.Details = emediaText.Details?.Trim();

                emediaTextRepository.Update(currentText);
            }

            await emediaTextRepository.SaveAsync();
        }

        public async Task SetSortAsAsync(int emediaId, string sortAs)
        {
            var emedia = await emediaRepository.FindAsync(emediaId)
                ?? throw new OcudaException($"Unable to find emedia with id {emediaId}");

            emedia.SortAs = !string.IsNullOrWhiteSpace(sortAs)
                ? sortAs
                : emedia.Name?.Trim();

            emediaRepository.Update(emedia);
            await emediaRepository.SaveAsync();
        }

        public async Task UpdateCategoriesAsync(int emediaId, ICollection<int> categoryIds)
        {
            var currentCategories = await emediaCategoryRepository
                .GetCategoryIdsForEmediaAsync(emediaId);

            var categoriesIdsToAdd = categoryIds.Except(currentCategories).ToList();
            var categoriesIdsToRemove = currentCategories.Except(categoryIds).ToList();

            var categoriesToAdd = categoriesIdsToAdd.ConvertAll(_ => new EmediaCategory
            {
                CategoryId = _,
                EmediaId = emediaId,
            });

            await emediaCategoryRepository.AddRangeAsync(categoriesToAdd);
            emediaCategoryRepository.RemoveByEmediaAndCategories(emediaId, categoriesIdsToRemove);

            await emediaCategoryRepository.SaveAsync();
        }

        public async Task UpdateGroupSortOrder(int id, bool increase)
        {
            var group = await emediaGroupRepository.FindAsync(id);

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

            var groupInPosition = await emediaGroupRepository.GetByOrderAsync(newSortOrder)
                ?? throw new OcudaException("Group is already in the last position.");

            groupInPosition.SortOrder = group.SortOrder;
            group.SortOrder = newSortOrder;

            emediaGroupRepository.Update(group);
            emediaGroupRepository.Update(groupInPosition);
            await emediaGroupRepository.SaveAsync();
        }

        public async Task UpdateSubjectsAsync(int emediaId, ICollection<int> subjectIds)
        {
            var currentSubjects = await emediaSubjectRepository
                .GetSubjectIdsForEmediaAsync(emediaId);

            var subjectIdsToAdd = subjectIds.Except(currentSubjects).ToList();
            var subjectIdsToRemove = currentSubjects.Except(subjectIds).ToList();

            var subjectsToAdd = subjectIdsToAdd.ConvertAll(_ => new EmediaSubject
            {
                SubjectId = _,
                EmediaId = emediaId,
            });

            await emediaSubjectRepository.AddRangeAsync(subjectsToAdd);
            emediaSubjectRepository.RemoveByEmediaAndSubjects(emediaId, subjectIdsToRemove);

            await emediaSubjectRepository.SaveAsync();
        }
    }
}
