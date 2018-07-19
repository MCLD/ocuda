using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Pages;
using Ocuda.Ops.Controllers.Authorization;
using Ocuda.Ops.Controllers.Filter;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(SectionManagerRequirement))]
    public class PagesController : BaseController<PagesController>
    {
        private readonly IPageService _pageService;
        private readonly ISectionService _sectionService;

        public PagesController(ServiceFacades.Controller<PagesController> context,
            IPageService pageService,
            ISectionService sectionService) : base(context)
        {
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public async Task<IActionResult> Index(string section, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BlogFilter(page, itemsPerPage)
            {
                SectionId = currentSection.Id
            };

            var pageList = await _pageService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = pageList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.MaxPage > 0 && paginateModel.CurrentPage > paginateModel.MaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new IndexViewModel
            {
                PaginateModel = paginateModel,
                Pages = pageList.Data,
                SectionId = currentSection.Id
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string title, string stub, int sectionId)
        {
            var page = new Page
            {
                IsDraft = true,
                SectionId = sectionId,
                Stub = stub,
                Title = title
            };

            try
            {
                var newPage = await _pageService.CreateAsync(CurrentUserId, page);
                return Json(new { success = true, id = newPage.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding page: {ex}", ex);
                ShowAlertDanger("Unable to add page: ", ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [RestoreModelState]
        public async Task<IActionResult> Edit(int id)
        {
            var page = await _pageService.GetByIdAsync(id);

            var viewModel = new DetailViewModel
            {
                Action = nameof(Edit),
                Page = page,
                SectionId = page.SectionId,
                IsDraft = page.IsDraft
            };

            return View("Detail", viewModel);
        }

        [HttpPost]
        [SaveModelState]
        public async Task<IActionResult> Edit(DetailViewModel model)
        {
            var currentPost = await _pageService.GetByIdAsync(model.Page.Id);

            if (currentPost.IsDraft == true && model.Page.IsDraft == false)
            {
                var stubInUse = await _pageService.StubInUseAsync(model.Page.Stub, currentPost.SectionId);

                if (stubInUse)
                {
                    ModelState.AddModelError("Page_IsDraft", string.Empty);
                    ShowAlertDanger("The chosen stub is already in use. Please choose a different stub.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var page = await _pageService.EditAsync(model.Page);
                    ShowAlertSuccess($"Updated page: {page.Title}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error editing page: {ex}", ex);
                    ShowAlertDanger("Unable to update page: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Edit), new { id = model.Page.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(IndexViewModel model)
        {
            try
            {
                await _pageService.DeleteAsync(model.Page.Id);
                ShowAlertSuccess("Page deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting page: {ex}", ex);
                ShowAlertDanger("Unable to delete page: ", ex.Message);
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }

        [HttpPost]
        public async Task<JsonResult> StubInUse(string stub, int sectionId)
        {
            return Json(await _pageService.StubInUseAsync(stub, sectionId));
        }
    }
}
