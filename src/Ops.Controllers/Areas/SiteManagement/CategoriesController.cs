using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Categories;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Filters;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class CategoriesController(ServiceFacades.Controller<CategoriesController> context,
        ICategoryService categoryService,
        ILanguageService languageService,
        IPermissionGroupService permissionGroupService,
        ISubjectService subjectService) : BaseController<CategoriesController>(context)
    {
        private readonly ICategoryService _categoryService = categoryService
            ?? throw new ArgumentNullException(nameof(categoryService));

        private readonly ILanguageService _languageService = languageService
            ?? throw new ArgumentNullException(nameof(languageService));

        private readonly IPermissionGroupService _permissionGroupService = permissionGroupService
            ?? throw new ArgumentNullException(nameof(permissionGroupService));

        private readonly ISubjectService _subjectService = subjectService
            ?? throw new ArgumentNullException(nameof(subjectService));

        public static string Area
        { get { return "SiteManagement"; } }

        public static string Name
        { get { return "Categories"; } }

        [HttpGet("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> CategoryDetails(int id, string language)
        {
            if (!await HasAreaPermissionsAsync()) { return RedirectToUnauthorized(); }

            var category = await _categoryService.GetByIdAsync(id);

            if (category == null) { return NotFound(); }

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var viewModel = new CategoryDetailsViewModel
            {
                Category = category,
                CategoryText = await _categoryService.GetTextByCategoryAndLanguageAsync(category.Id,
                    selectedLanguage.Id),
                LanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages,
                    nameof(Language.Name),
                    nameof(Language.Description),
                    selectedLanguage.Name)
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]/{id}")]
        [SaveModelState]
        public async Task<IActionResult> CategoryDetails(int id, CategoryDetailsViewModel model)
        {
            if (!await HasAreaPermissionsAsync()) { return RedirectToUnauthorized(); }

            ArgumentNullException.ThrowIfNull(model);
            if (model.CategoryText == null)
            {
                _logger.LogError("Cannot update empty category text");
                ShowAlertDanger("Cannot update empty category text.");
            }
            else
            {
                model.CategoryText.CategoryId = id;
                if (ModelState.IsValid)
                {
                    try
                    {
                        await _categoryService.SetCategoryTextAsync(model.CategoryText);
                        if (model.CategoryText.LanguageId == await _languageService.GetDefaultLanguageId())
                        {
                            var category = await _categoryService.GetByIdAsync(id);
                            category.Name = model.CategoryText.Text.Trim();
                            await _categoryService.EditAsync(category);
                        }

                        ShowAlertSuccess("Updated category text");
                    }
                    catch (OcudaException oex)
                    {
                        _logger.LogError(oex, "Error updating category text: {Message}", oex.Message);
                        ShowAlertDanger("Error updating category text");
                    }
                }
            }

            var language = await _languageService.GetActiveByIdAsync(model.CategoryText.LanguageId);

            return RedirectToAction(nameof(CategoryDetails), new
            {
                id = model.CategoryText.CategoryId,
                language = language.IsDefault ? null : language.Name
            });
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> CategoryIndex(int page)
        {
            if (!await HasAreaPermissionsAsync()) { return RedirectToUnauthorized(); }

            page = page > 0 ? page : 1;

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);
            var filter = new BaseFilter(page, itemsPerPage);

            var categoryList = await _categoryService.GetPaginatedListAsync(filter);

            var viewModel = new IndexViewModel
            {
                ItemCount = categoryList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            foreach (var category in categoryList.Data)
            {
                category.CategoryLanguages = await _categoryService
                    .GetCategoryLanguagesAsync(category.Id);

                category.CategoryEmedias = await _categoryService
                    .GetCategoryEmediasAsync(category.Id);

                viewModel.Categories.Add(category);
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreateCategory(IndexViewModel model)
        {
            if (!await HasAreaPermissionsAsync()) { return RedirectToUnauthorized(); }

            ArgumentNullException.ThrowIfNull(model);
            JsonResponse response;

            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _categoryService.CreateAsync(model.Category);
                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(CategoryDetails), new { id = category.Id })
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
        public async Task<IActionResult> CreateSubject(IndexViewModel model)
        {
            if (!await HasAreaPermissionsAsync()) { return RedirectToUnauthorized(); }

            ArgumentNullException.ThrowIfNull(model);
            JsonResponse response;

            if (ModelState.IsValid)
            {
                try
                {
                    var subject = await _subjectService.CreateAsync(model.Subject);
                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(SubjectDetails), new { id = subject.Id })
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
        public async Task<IActionResult> DeleteCategory(IndexViewModel model)
        {
            if (!await HasAreaPermissionsAsync()) { return RedirectToUnauthorized(); }

            ArgumentNullException.ThrowIfNull(model);
            try
            {
                await _categoryService.DeleteAsync(model.Category.Id);
                ShowAlertSuccess($"Deleted category: {model.Category.Name}");
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex, "Error deleting category: {Message}", oex.Message);
                ShowAlertDanger($"Error deleting category: {model.Category.Name}");
            }

            return RedirectToAction(nameof(CategoryIndex), new { page = model.CurrentPage });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteSubject(IndexViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            try
            {
                await _subjectService.DeleteAsync(model.Subject.Id);
                ShowAlertSuccess($"Deleted subject: {model.Subject.Name}");
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex, "Error deleting subject: {Message}", oex.Message);
                ShowAlertDanger($"Error deleting subject: {model.Subject.Name}");
            }

            return RedirectToAction(nameof(SubjectIndex), new { page = model.CurrentPage });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditCategory(IndexViewModel model)
        {
            if (!await HasAreaPermissionsAsync()) { return RedirectToUnauthorized(); }

            ArgumentNullException.ThrowIfNull(model);
            JsonResponse response;

            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _categoryService.EditAsync(model.Category);
                    response = new JsonResponse
                    {
                        Success = true
                    };

                    ShowAlertSuccess($"Updated category: {category.Name}");
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
        public async Task<IActionResult> EditSubject(IndexViewModel model)
        {
            if (!await HasAreaPermissionsAsync()) { return RedirectToUnauthorized(); }

            ArgumentNullException.ThrowIfNull(model);
            JsonResponse response;

            if (ModelState.IsValid)
            {
                try
                {
                    var subject = await _subjectService.EditAsync(model.Subject);
                    response = new JsonResponse
                    {
                        Success = true
                    };

                    ShowAlertSuccess($"Updated subject: {subject.Name}");
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

        [HttpGet("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> SubjectDetails(int id, string language)
        {
            if (!await HasAreaPermissionsAsync()) { return RedirectToUnauthorized(); }

            var subject = await _subjectService.GetByIdAsync(id);

            if (subject == null) { return NotFound(); }

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var viewModel = new SubjectDetailsViewModel
            {
                Subject = subject,
                SubjectText = await _subjectService.GetTextBySubjectAndLanguageAsync(subject.Id,
                    selectedLanguage.Id),
                LanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages,
                    nameof(Language.Name),
                    nameof(Language.Description),
                    selectedLanguage.Name)
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]/{id}")]
        [SaveModelState]
        public async Task<IActionResult> SubjectDetails(int id, SubjectDetailsViewModel model)
        {
            if (!await HasAreaPermissionsAsync()) { return RedirectToUnauthorized(); }

            ArgumentNullException.ThrowIfNull(model);
            if (model.SubjectText == null)
            {
                _logger.LogError("Cannot update empty subject text");
                ShowAlertDanger("Cannot update empty subject text.");
            }
            else
            {
                model.SubjectText.SubjectId = id;
                if (ModelState.IsValid)
                {
                    try
                    {
                        await _subjectService.SetSubjectTextAsync(model.SubjectText);
                        if (model.SubjectText.LanguageId == await _languageService.GetDefaultLanguageId())
                        {
                            var subject = await _subjectService.GetByIdAsync(id);
                            subject.Name = model.SubjectText.Text.Trim();
                            await _subjectService.EditAsync(subject);
                        }
                        ShowAlertSuccess("Updated subject text");
                    }
                    catch (OcudaException oex)
                    {
                        _logger.LogError(oex,
                            "Error updating subject text: {ErrorMessage}",
                            oex.Message);
                        ShowAlertDanger("Error updating subject text");
                    }
                }
            }

            var language = await _languageService.GetActiveByIdAsync(model.SubjectText.LanguageId);

            return RedirectToAction(nameof(SubjectDetails), new
            {
                id = model.SubjectText.SubjectId,
                language = language.IsDefault ? null : language.Name
            });
        }

        [Route("[action]")]
        public async Task<IActionResult> SubjectIndex(int page)
        {
            if (!await HasAreaPermissionsAsync()) { return RedirectToUnauthorized(); }

            page = page > 0 ? page : 1;
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);
            var filter = new BaseFilter(page, itemsPerPage);

            var subjectList = await _subjectService.GetPaginatedListAsync(filter);

            var viewModel = new IndexViewModel
            {
                ItemCount = subjectList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            foreach (var subject in subjectList.Data)
            {
                subject.SubjectLanguages.AddRange(await _subjectService
                    .GetSubjectLanguagesAsync(subject.Id));
                subject.SubjectEmedias.AddRange(await _subjectService
                    .GetSubjectEmediasAsync(subject.Id));

                viewModel.Subjects.Add(subject);
            }

            return View(viewModel);
        }

        private async Task<bool> HasAreaPermissionsAsync()
        {
            return await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.CategoryManagement);
        }
    }
}