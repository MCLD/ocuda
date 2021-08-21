using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Emedia;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class EmediaController : BaseController<EmediaController>
    {
        private readonly ICategoryService _categoryService;
        private readonly IEmediaService _emediaService;
        private readonly ILanguageService _languageService;

        public static string Name { get { return "Emedia"; } }
        public static string Area { get { return "SiteManagement"; } }

        public EmediaController(ServiceFacades.Controller<EmediaController> context,
            ICategoryService categoryService,
            IEmediaService emediaService,
            ILanguageService languageService) : base(context)
        {
            _categoryService = categoryService
                ?? throw new ArgumentNullException(nameof(categoryService));
            _emediaService = emediaService
                ?? throw new ArgumentNullException(nameof(emediaService));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var filter = new BaseFilter(page);

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
        public async Task<IActionResult> CreateGroup(IndexViewModel model)
        {
            JsonResponse response;

            if (ModelState.IsValid)
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
        public async Task<IActionResult> EditGroup(IndexViewModel model)
        {
            JsonResponse response;

            if (ModelState.IsValid)
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

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteGroup(IndexViewModel model)
        {
            try
            {
                await _emediaService.DeleteGroupAsync(model.EmediaGroup.Id);
                ShowAlertSuccess($"Deleted emedia group: {model.EmediaGroup.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting emedia group: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting emedia group: {model.EmediaGroup.Name}");
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> ChangeGroupSort(int id, bool increase)
        {
            JsonResponse response;

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

            return Json(response);
        }

        [Route("[action]/{id}")]
        public async Task<IActionResult> GroupDetails(int id, int page = 1)
        {
            var group = await _emediaService.GetGroupByIdAsync(id);

            if (group == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var filter = new BaseFilter(page);

            var emediaList = await _emediaService.GetPaginatedListAsync(filter);

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

            var viewModel = new GroupDetailsViewModel
            {
                EmediaGroup = group,
                Emedias = emediaList.Data,
                PaginateModel = paginateModel
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreateEmedia(GroupDetailsViewModel model)
        {
            JsonResponse response;

            if (ModelState.IsValid)
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
        public async Task<IActionResult> EditEmedia(GroupDetailsViewModel model)
        {
            JsonResponse response;

            if (ModelState.IsValid)
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
        public async Task<IActionResult> DeleteEmedia(GroupDetailsViewModel model)
        {
            try
            {
                await _emediaService.DeleteAsync(model.Emedia.Id);
                ShowAlertSuccess($"Deleted emedia : {model.Emedia.Name}");
            }
            catch (Exception ex)
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

        [Route("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Details(int id, string language)
        {
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

            var viewModel = new DetailsViewModel
            {
                CategoryList = await _categoryService.GetAllAsync(),
                Emedia = emedia,
                LanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages,
                    nameof(Language.Name),
                    nameof(Language.Description),
                    selectedLanguage.Name),
                EmediaText = emediaText
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]/{id}")]
        [SaveModelState]
        public async Task<IActionResult> Details(DetailsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _emediaService.SetEmediaTextAsync(model.EmediaText);
                    ShowAlertSuccess("Updated emedia text");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating emedia text: {Message}", ex.Message);
                    ShowAlertDanger($"Error updating emedia text");
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
        public async Task<IActionResult> ChangeCategories(int id, List<int> categories)
        {
            return null;
        }

        /*
        #region Emedia

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page, itemsPerPage);

            var emediaList = await _emediaService.GetPaginatedListAsync(filter);

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

            var viewModel = new EmediaViewModel
            {
                AllEmedia = emediaList.Data,
                PaginateModel = paginateModel
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("[action]/{emediaStub}")]
        [RestoreModelState]
        public async Task<IActionResult> EditEmedia(string emediaStub)
        {
            var emedia = await _emediaService.GetByStubAsync(emediaStub);
            if (emedia != null)
            {
                var categories = await _categoryService.GetAllCategories();
                var viewModel = new EmediaViewModel
                {
                    Emedia = emedia,
                    SelectionEmediaCategories = new SelectList(categories, "Id", "Name"),
                    CategoryIds = emedia.Categories.Select(_ => _.Id).ToList()
                };
                return View(viewModel);
            }
            else
            {
                ShowAlertDanger($"The emedia {emediaStub} does not exist.");
                return RedirectToAction(nameof(EmediaController.Index));
            }
        }

        [HttpPost]
        [Route("[action]/{emediaStub}")]
        [SaveModelState]
        public async Task<IActionResult> EditEmedia(EmediaViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel?.Emedia.Stub))
            {
                var currEmedia = await _emediaService.GetByStubAsync(viewModel.Emedia.Stub);
                if (currEmedia.Id != viewModel.Emedia.Id)
                {
                    ModelState.AddModelError("Emedia.Stub", "This stub already exists");
                    ShowAlertDanger("Stub is required for an emedia");
                    return RedirectToAction(nameof(EmediaController.EditEmedia));
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await _emediaService.UpdateEmedia(viewModel.Emedia);
                    await _emediaService.UpdateEmediaCategoryAsync(viewModel.CategoryIds, viewModel.Emedia.Id);
                    ShowAlertSuccess($"Updated emedia: {viewModel.Emedia.Name}");
                    return RedirectToAction(nameof(EmediaController.EditEmedia),
                        new { emediaStub = viewModel.Emedia.Stub });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to Update emedia: {viewModel.Emedia.Name}");
                    _logger.LogError(ex, "Problem updating emedia: {Message}", ex.Message);
                    return RedirectToAction(nameof(EmediaController.EditEmedia),
                        new { emediaStub = viewModel.Emedia });
                }
            }
            else
            {
                ShowAlertDanger($"Invalid Parameters: {viewModel.Emedia.Name}");
                return RedirectToAction(nameof(EmediaController.EditEmedia),
                    new { emediaStub = viewModel.Emedia.Stub });
            }
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> DeleteEmedia(Emedia emedia)
        {
            try
            {
                var categories = await _emediaService.GetEmediaCategoriesById(emedia.Id);
                if (categories.Count > 0)
                {
                    ShowAlertDanger($"Remove {emedia.Name}'s categories before deleting.");
                    return RedirectToAction(nameof(Index));
                }
                await _emediaService.DeleteAsync(emedia.Id);
                ShowAlertSuccess($"Deleted Emedia: {emedia.Name}");
            }
            catch (OcudaException ex)
            {
                _logger.LogError(ex, "Problem deleting emedia: {Message}", ex.Message);
                ShowAlertDanger($"Unable to Delete emedia {emedia.Name}: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        [Route("[action]")]
        [RestoreModelState]
        public async Task<IActionResult> AddEmedia()
        {
            var categories = await _categoryService.GetAllCategories();
            var viewModel = new EmediaViewModel
            {
                Emedia = new Emedia(),
                SelectionEmediaCategories = new SelectList(categories, "Id", "Name")
            };
            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> AddEmedia(EmediaViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel?.Emedia.Stub))
            {
                var currEmedia = await _emediaService.GetByStubAsync(viewModel.Emedia.Stub);
                if (currEmedia != null)
                {
                    ModelState.AddModelError("Emedia.Stub", "This stub already exists");
                    ShowAlertDanger("Stub is required for an emedia");
                    return RedirectToAction(nameof(EmediaController.AddEmedia));
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await _emediaService.AddEmedia(viewModel.Emedia);
                    var emedia = await _emediaService
                        .GetByStubAsync(viewModel.Emedia.Stub.ToLowerInvariant().Trim());
                    await _emediaService.UpdateEmediaCategoryAsync(viewModel.CategoryIds,
                        emedia.Id);
                    ShowAlertSuccess($"Added emedia: {emedia.Name}");
                    return RedirectToAction(nameof(EmediaController.EditEmedia),
                        new { emediaStub = viewModel.Emedia.Stub });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to Create emedia: {ex.Message}");
                    _logger.LogError(ex, "Problem creating emedia: {Message}", ex.Message);
                    return RedirectToAction(nameof(EmediaController.AddEmedia));
                }
            }
            else
            {
                ShowAlertDanger($"Invalid paramaters");
                return RedirectToAction(nameof(EmediaController.AddEmedia));
            }
        }

        #endregion

        #region Emedia Groups

        [Route("[action]")]
        public async Task<IActionResult> Groups(int page = 1)
        {
            var filter = new BaseFilter(page);

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

            var viewModel = new GroupsViewModel
            {
                EmediaGroups = groupList.Data,
                PaginateModel = paginateModel
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteGroup(GroupsViewModel model)
        {
            return null;
        }

        #endregion
        */
    }
}
