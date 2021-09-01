using System;
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
    public class CategoriesController : BaseController<CategoriesController>
    {
        private readonly ICategoryService _categoryService;
        private readonly ILanguageService _languageService;

        public static string Name { get { return "Categories"; } }
        public static string Area { get { return "SiteManagement"; } }

        public CategoriesController(ServiceFacades.Controller<CategoriesController> context,
            ICategoryService categoryService,
            ILanguageService languageService) : base(context)
        {
            _categoryService = categoryService
                ?? throw new ArgumentNullException(nameof(categoryService));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);
            var filter = new BaseFilter(page, itemsPerPage);

            var categoryList = await _categoryService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = categoryList.Count,
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

            foreach (var category in categoryList.Data)
            {
                category.CategoryLanguages = await _categoryService
                    .GetCategoryLanguagesAsync(category.Id);

                category.CategoryEmedias = await _categoryService
                    .GetCategoryEmediasAsync(category.Id);
            }

            var viewModel = new IndexViewModel
            {
                Categories = categoryList.Data,
                PaginateModel = paginateModel
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreateCategory(IndexViewModel model)
        {
            JsonResponse response;

            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _categoryService.CreateAsync(model.Category);
                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(Details), new { id = category.Id })
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
        public async Task<IActionResult> EditCategory(IndexViewModel model)
        {
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
        public async Task<IActionResult> DeleteCategory(IndexViewModel model)
        {
            try
            {
                await _categoryService.DeleteAsync(model.Category.Id);
                ShowAlertSuccess($"Deleted category: {model.Category.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting category: {model.Category.Name}");
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }

        [Route("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Details(int id, string language)
        {
            var category = await _categoryService.GetByIdAsync(id);

            if (category == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var viewModel = new DetailsViewModel
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
        public async Task<IActionResult> Details (DetailsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.SetCategoryTextAsync(model.CategoryText);
                    ShowAlertSuccess("Updated category text");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating category text: {Message}", ex.Message);
                    ShowAlertDanger("Error updating category text");
                }
            }

            var language = await _languageService.GetActiveByIdAsync(model.CategoryText.LanguageId);

            return RedirectToAction(nameof(Details), new
            {
                id = model.CategoryText.CategoryId,
                language = language.IsDefault ? null : language.Name
            });
        }
    }
}
