using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Pages;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class PagesController : BaseController<PagesController>
    {
        private readonly IPageService _pageService;

        public static string Name { get { return "Pages"; } }

        public PagesController(ServiceFacades.Controller<PagesController> context,
            IPageService pageService) : base(context)
        {
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        }

        [Route("")]
        [Route("[action]/{page}")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var filter = new BaseFilter(page);

            var pageList = await _pageService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = pageList.Count,
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
                PaginateModel = paginateModel,
                Pages = pageList.Data
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Create(IndexViewModel model)
        {
            JsonResponse response;

            model.Page.IsPublished = false;

            try
            {
                var newPage = await _pageService.CreateAsync(model.Page);
                response = new JsonResponse
                {
                    Success = true,
                    EntityId = newPage.Id
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

            return Json(response);
        }

        [Route("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Edit(int id)
        {
            var page = await _pageService.GetByIdAsync(id);

            var viewModel = new DetailViewModel
            {
                Action = nameof(Edit),
                Page = page
            };

            return View("Detail", viewModel);
        }

        [HttpPost]
        [Route("[action]/{id?}")]
        [SaveModelState]
        public async Task<IActionResult> Edit(DetailViewModel model)
        {
            var currentPage = await _pageService.GetByIdAsync(model.Page.Id);

            if (!currentPage.IsPublished && model.Publish)
            {
                var stubInUse = await _pageService.StubInUseAsync(model.Page);

                if (stubInUse)
                {
                    ModelState.AddModelError("Page.Stub", "The chosen stub is already in use for that page type. Please choose a different stub.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var page = await _pageService.EditAsync(model.Page, model.Publish);
                    ShowAlertSuccess($"Updated page: {page.Stub}");
                    return RedirectToAction(nameof(Index));
                }
                catch (OcudaException ex)
                {
                    _logger.LogError($"Error editing page: {ex}", ex);
                    ShowAlertDanger("Unable to update page: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Edit), new { id = model.Page.Id });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Delete(IndexViewModel model)
        {
            try
            {
                await _pageService.DeleteAsync(model.Page.Id);
                ShowAlertSuccess($"Deleted page: {model.Page.Stub}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting page: {ex}", ex);
                ShowAlertDanger("Unable to delete page: ", ex.Message);
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> StubInUse(Page page)
        {
            var response = new JsonResponse
            {
                Success = !(await _pageService.StubInUseAsync(page))
            };

            if (!response.Success)
            {
                response.Message = "The chosen stub is already in use for that page type. Please choose a different stub.";
            }

            return Json(response);
        }
    }
}
