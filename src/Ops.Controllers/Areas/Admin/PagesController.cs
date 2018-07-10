using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Pages;
using Ocuda.Ops.Controllers.Authorization;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(SectionManagerRequirement))]
    public class PagesController : BaseController<PagesController>
    {
        private readonly PageService _pageService;
        private readonly SectionService _sectionService;

        public PagesController(ServiceFacade.Controller<PagesController> context,
            PageService pageService,
            SectionService sectionService) : base(context)
        {
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public async Task<IActionResult> Index(string section, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);

            var filter = new BlogFilter(page)
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
                Pages = pageList.Data
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create(string section)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);

            var viewModel = new DetailViewModel
            {
                Action = nameof(Create),
                SectionId = currentSection.Id
            };

            return View("Detail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.Page.SectionId = model.SectionId;
                    var newPage = await _pageService.CreateAsync(model.Page);
                    ShowAlertSuccess($"Added page: {newPage.Title}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error adding page: {ex}", ex);
                    ShowAlertDanger("Unable to add page: ", ex.Message);
                }
            }

            model.Action = nameof(Create);
            return View("Detail", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var viewModel = new DetailViewModel
            {
                Action = nameof(Edit),
                Page = await _pageService.GetByIdAsync(id)
            };

            return View("Detail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DetailViewModel model)
        {
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

            model.Action = nameof(Edit);
            return View("Detail", model);
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
    }
}
