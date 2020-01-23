using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Category;
using Ocuda.Ops.Controllers.Filters;
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
        private readonly IEmediaService _emediaService;

        public static string Name { get { return "Categories"; } }
        public static string Area { get { return "SiteManagement"; } }

        public CategoriesController(ServiceFacades.Controller<CategoriesController> context,
            ICategoryService categoryService,
            IEmediaService emediaService) : base(context)
        {
            _categoryService = categoryService
                ?? throw new ArgumentNullException(nameof(categoryService));
            _emediaService = emediaService
                ?? throw new ArgumentNullException(nameof(emediaService));
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

            var viewModel = new CategoryViewModel
            {
                AllCategories = categoryList.Data,
                PaginateModel = paginateModel
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EditCategory(CategoryViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel?.Category?.Class))
            {
                var currCategory = _categoryService.GetByClass(viewModel.Category.Class);
                if (currCategory != null && currCategory.Id != viewModel.Category.Id)
                {
                    ModelState.AddModelError("Category.Class", "This stub already exists");
                    ShowAlertDanger("Class is required for a category");
                    return RedirectToAction(nameof(CategoriesController.Index));
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.UpdateCategory(viewModel.Category);
                    ShowAlertSuccess($"Updated category: {viewModel.Category.Name}");
                    return RedirectToAction(nameof(CategoriesController.Index));
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to Update category: {viewModel.Category.Name}");
                    _logger.LogError(ex, "Problem updating category: {Message}", ex.Message);
                    return RedirectToAction(nameof(CategoriesController.Index));
                }
            }
            else
            {
                ShowAlertDanger($"Invalid Parameters: {viewModel.Category.Name}");
                return RedirectToAction(nameof(CategoriesController.Index));
            }
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> DeleteCategory(Category category)
        {
            try
            {
                var categories = await _emediaService.GetEmediaCategoriesByCategoryId(category.Id);
                if (categories.ToList().Count > 0)
                {
                    ShowAlertDanger($"Remove {category.Name}'s categories before deleting.");
                    return RedirectToAction(nameof(CategoriesController.Index));
                }
                await _categoryService.DeleteAsync(category.Id);
                ShowAlertSuccess($"Deleted category: {category.Name}");
            }
            catch (OcudaException ex)
            {
                _logger.LogError(ex, "Problem deleting category: {Message}", ex.Message);
                ShowAlertDanger($"Unable to Delete category {category.Name}: {ex.Message}");
            }

            return RedirectToAction(nameof(CategoriesController.Index));
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> AddCategory(CategoryViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel?.Category?.Class))
            {
                var currCategory = _categoryService.GetByClass(viewModel.Category.Class);
                if (currCategory != null)
                {
                    ModelState.AddModelError("Category.Class", "This class already exists");
                    ShowAlertDanger("Class is required for a category");
                    return RedirectToAction(nameof(CategoriesController.Index));
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.AddCategory(viewModel.Category);
                    var category = _categoryService.GetByClass(viewModel.Category.Class.ToLower().Trim());
                    ShowAlertSuccess($"Added Category: {category.Name}");
                    return RedirectToAction(nameof(CategoriesController.Index));
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to Create Category: {ex.Message}");
                    _logger.LogError(ex, "Problem creating category: {Message}", ex.Message);
                    return RedirectToAction(nameof(CategoriesController.Index));
                }
            }
            else
            {
                ShowAlertDanger($"Invalid paramaters");
                return RedirectToAction(nameof(CategoriesController.Index));
            }
        }
    }
}
