using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Emedia;
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
    public class EmediaController : BaseController<EmediaController>
    {
        private readonly IEmediaService _emediaService;
        private readonly ICategoryService _categoryService;

        public static string Name { get { return "Emedia"; } }
        public static string Area { get { return "SiteManagement"; } }

        public EmediaController(ServiceFacades.Controller<EmediaController> context,
            IEmediaService emediaService,
            ICategoryService categoryService) : base(context)
        {
            _emediaService = emediaService
                ?? throw new ArgumentNullException(nameof(emediaService));
            _categoryService = categoryService
                ?? throw new ArgumentNullException(nameof(categoryService));
        }

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
            if (!string.IsNullOrEmpty(viewModel.Emedia.Stub))
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
            if (!string.IsNullOrEmpty(viewModel.Emedia.Stub))
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
                    var emedia = await _emediaService.GetByStubAsync(viewModel.Emedia.Stub.ToLower().Trim());
                    await _emediaService.UpdateEmediaCategoryAsync(viewModel.CategoryIds, emedia.Id);
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
    }
}
