using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Emedia;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area(nameof(SiteManagement))]
    [Route("[area]/[controller]")]
    public class EmediaController(ServiceFacades.Controller<EmediaController> context,
        ICategoryService categoryService,
        IEmediaService emediaService,
        ILanguageService languageService,
        IPermissionGroupService permissionGroupService,
        ISegmentService segmentService,
        ISubjectService subjectService) : BaseController<EmediaController>(context)
    {
        private readonly ICategoryService _categoryService = categoryService
            ?? throw new ArgumentNullException(nameof(categoryService));

        private readonly IEmediaService _emediaService = emediaService
            ?? throw new ArgumentNullException(nameof(emediaService));

        private readonly ILanguageService _languageService = languageService
            ?? throw new ArgumentNullException(nameof(languageService));

        private readonly IPermissionGroupService _permissionGroupService = permissionGroupService
            ?? throw new ArgumentNullException(nameof(permissionGroupService));

        private readonly ISegmentService _segmentService = segmentService
            ?? throw new ArgumentNullException(nameof(segmentService));

        private readonly ISubjectService _subjectService = subjectService
            ?? throw new ArgumentNullException(nameof(subjectService));


        public static string Area
        { get { return nameof(SiteManagement); } }

        public static string Name
        { get { return "Emedia"; } }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ChangeCategories(int id, ICollection<int> categories)
        {
            JsonResponse response;

            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }
            else
            {
                try
                {
                    await _emediaService.UpdateCategoriesAsync(id, categories);
                    response = new JsonResponse
                    {
                        Success = true
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Message = ex.Message,
                        Success = false
                    };
                }
            }

            return Json(response);
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ChangeSubjects(int id, ICollection<int> subjects)
        {
            JsonResponse response;

            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }
            else
            {
                try
                {
                    await _emediaService.UpdateSubjectsAsync(id, subjects);
                    response = new JsonResponse
                    {
                        Success = true
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Message = ex.Message,
                        Success = false
                    };
                }
            }

            return Json(response);
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> ChangeGroupSort(int id, bool increase)
        {
            JsonResponse response;

            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }
            else
            {
                try
                {
                    await _emediaService.UpdateGroupSortOrder(id, increase);
                    response = new JsonResponse
                    {
                        Success = true
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Message = ex.Message,
                        Success = false
                    };
                }
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreateEmedia(GroupDetailsViewModel model)
        {
            JsonResponse response;

            if (model == null)
            {
                response = new JsonResponse
                {
                    Success = false,
                    Message = "Unable to create empty emedia item."
                };
            }
            else if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    var group = await _emediaService.CreateAsync(model.Emedia);
                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(Details), new { id = group.Id })
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors)
                };
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreateGroup(IndexViewModel model)
        {
            JsonResponse response;

            if (model == null)
            {
                response = new JsonResponse
                {
                    Success = false,
                    Message = "Unable to create empty group."
                };
            }
            else if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    var group = await _emediaService.CreateGroupAsync(model.EmediaGroup);
                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(GroupDetails), new { id = group.Id })
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors)
                };
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteEmedia(GroupDetailsViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await _emediaService.DeleteAsync(model.Emedia.Id);
                ShowAlertSuccess($"Deleted emedia : {model.Emedia.Name}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting emedia: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting emedia: {model.Emedia.Name}");
            }

            return RedirectToAction(nameof(GroupDetails),
                new
                {
                    id = model.EmediaGroup.Id,
                    page = model.PaginateModel.CurrentPage
                });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteGroup(IndexViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await _emediaService.DeleteGroupAsync(model.EmediaGroup.Id);
                ShowAlertSuccess($"Deleted emedia group: {model.EmediaGroup.Name}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting emedia group: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting emedia group: {model.EmediaGroup.Name}");
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteGroupSegment(GroupDetailsViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await _emediaService.DeleteGroupSegmentAsync(model.EmediaGroupId);
                ShowAlertSuccess($"Deleted group segment: {model.Segment.Name}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting group segment: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting group segment: {model.Segment.Name}");
            }

            return RedirectToAction(nameof(GroupDetails), new
            {
                id = model.EmediaGroupId,
                page = model.PaginateModel.CurrentPage
            });
        }

        [Route("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Details(int id, string language)
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            var emedia = await _emediaService.GetIncludingGroupAsync(id);

            if (emedia == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var emediaText = await _emediaService.GetTextByEmediaAndLanguageAsync(emedia.Id,
                selectedLanguage.Id);

            var selectedTopics = await _emediaService.GetSubjectsForEmediaAsync(emedia.Id);

            var selectedCategories = await _emediaService.GetCategoriesForEmediaAsync(emedia.Id);

            var viewModel = new DetailsViewModel
            {
                CategorySelectionText = string.Join(", ",
                    selectedCategories.Select(_ => _.Name).Order()),
                Emedia = emedia,
                EmediaText = emediaText,
                LanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages,
                    nameof(Language.Name),
                    nameof(Language.Description),
                    selectedLanguage.Name),
                SubjectSelectionText = string.Join(", ",
                    selectedTopics.Select(_ => _.Name).Order())
            };

            viewModel.CategoryList.AddRange(await _categoryService.GetAllAsync());
            viewModel.CategorySelection.AddRange(selectedCategories.Select(_ => _.Id).ToList());

            viewModel.SubjectList.AddRange(await _subjectService.GetAllAsync());
            viewModel.SubjectSelection.AddRange(selectedTopics.Select(_ => _.Id).ToList());

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]/{id}")]
        [SaveModelState]
        public async Task<IActionResult> Details(DetailsViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _emediaService.SetEmediaTextAsync(model.EmediaText);
                    ShowAlertSuccess("Updated emedia text");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error updating emedia text: {Message}", ex.Message);
                    ShowAlertDanger("Error updating emedia text");
                }
            }

            var language = await _languageService.GetActiveByIdAsync(model.EmediaText.LanguageId);

            return RedirectToAction(nameof(Details), new
            {
                id = model.EmediaText.EmediaId,
                language = language.IsDefault ? null : language.Name
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditEmedia(GroupDetailsViewModel model)
        {
            JsonResponse response;

            if (model == null)
            {
                response = new JsonResponse
                {
                    Success = false,
                    Message = "Unable to edit empty emedia item."
                };
            }
            else if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    var emedia = await _emediaService.EditAsync(model.Emedia);
                    response = new JsonResponse
                    {
                        Success = true
                    };

                    ShowAlertSuccess($"Updated emedia: {emedia.Name}");
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors)
                };
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditGroup(IndexViewModel model)
        {
            JsonResponse response;

            if (model == null)
            {
                response = new JsonResponse
                {
                    Success = false,
                    Message = "Unable to edit empty group."
                };
            }
            else if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    var group = await _emediaService.EditGroupAsync(model.EmediaGroup);
                    response = new JsonResponse
                    {
                        Success = true
                    };

                    ShowAlertSuccess($"Updated group: {group.Name}");
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors)
                };
            }

            return Json(response);
        }

        [Route("[action]/{id}")]
        public async Task<IActionResult> GroupDetails(int id, int page = 1)
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            var group = await _emediaService.GetGroupIncludingSegmentAsync(id);

            if (group == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page, itemsPerPage);

            var emediaList = await _emediaService.GetPaginatedListForGroupAsync(group.Id, filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = emediaList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };
            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            foreach (var emedia in emediaList.Data)
            {
                var languages = await _emediaService.GetEmediaLanguagesAsync(emedia.Id);
                emedia.EmediaLanguages.AddRange(languages);
            }

            var viewModel = new GroupDetailsViewModel
            {
                EmediaGroup = group,
                EmediaGroupId = id,
                Emedias = emediaList.Data,
                PaginateModel = paginateModel
            };

            if (group.SegmentId.HasValue)
            {
                viewModel.SegmentLanguages = await _segmentService
                    .GetSegmentLanguagesByIdAsync(group.SegmentId.Value);
            }

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Import(int groupId, IFormFile jsonImportFile)
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            if (jsonImportFile == null)
            {
                return StatusCode(500);
            }

            var jsonStream = jsonImportFile.OpenReadStream();

            var dataFromFile = await JsonSerializer
                .DeserializeAsync<PortableList<Ocuda.Models.ESourceImport>>(jsonStream);

            await _emediaService.ImportItemsAsync(groupId, dataFromFile.Items);

            return RedirectToAction(nameof(Index));
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index(int page = 1)
        {
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page, itemsPerPage);

            var groupList = await _emediaService.GetPaginatedGroupListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = groupList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };
            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new IndexViewModel
            {
                EmediaGroups = groupList.Data,
                PaginateModel = paginateModel
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdateGroupSegment(GroupDetailsViewModel model)
        {
            JsonResponse response;

            if (model == null)
            {
                response = new JsonResponse
                {
                    Success = false,
                    Message = "Unable to update empty group."
                };
            }
            else if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    string url = null;

                    var group = await _emediaService.GetGroupByIdAsync(model.EmediaGroupId);
                    if (group.SegmentId.HasValue)
                    {
                        var segment = await _segmentService.GetByIdAsync(group.SegmentId.Value);
                        segment.Name = model.Segment.Name;
                        segment = await _segmentService.EditAsync(segment);
                        ShowAlertSuccess($"Updated group segment: {segment.Name}");
                    }
                    else
                    {
                        group.Segment = model.Segment;
                        group.Segment.IsActive = true;
                        await _emediaService.AddGroupSegmentAsync(group);
                        url = Url.Action(
                            nameof(SegmentsController.Detail),
                            SegmentsController.Name,
                            new { id = group.SegmentId });
                    }

                    response = new JsonResponse
                    {
                        Success = true,
                        Url = url
                    };
                }
                catch (DbUpdateException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors)
                };
            }

            return Json(response);
        }
    }
}