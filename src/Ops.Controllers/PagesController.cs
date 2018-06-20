using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.ViewModels.Pages;
using Ocuda.Ops.Service;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers
{
    public class PagesController : Abstract.BaseController
    {
        private readonly PageService _pageService;
        
        public PagesController(PageService pageService)
        {
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var pageList = await _pageService.GetPagesAsync();

            var paginateModel = new PaginateModel
            {
                ItemCount = await _pageService.GetPageCountAsync(),
                CurrentPage = page,
                ItemsPerPage = 2
            };

            if (paginateModel.MaxPage > 0 && paginateModel.CurrentPage > paginateModel.MaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new PageListViewModel
            {
                PaginateModel = paginateModel,
                Pages = pageList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        //TODO use stub instead of id
        public async Task<IActionResult> View(int id)
        {
            var page = await _pageService.GetPageByIdAsync(id);

            page.Content = CommonMark.CommonMarkConverter.Convert(page.Content);

            return View(page);
        }

        public async Task<IActionResult> AdminList(int page = 1)
        {
            var pageList = await _pageService.GetPagesAsync();

            var paginateModel = new PaginateModel
            {
                ItemCount = await _pageService.GetPageCountAsync(),
                CurrentPage = page,
                ItemsPerPage = 2
            };

            if (paginateModel.MaxPage > 0 && paginateModel.CurrentPage > paginateModel.MaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new AdminListViewModel
            {
                PaginateModel = paginateModel,
                Pages = pageList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        public IActionResult AdminCreate()
        {
            var viewModel = new AdminDetailViewModel
            {
                Action = nameof(AdminCreate)
            };

            return View("AdminDetail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AdminCreate(AdminDetailViewModel model)
        {
            model.Page.SectionId = 1; //TODO: Fix SectionId, CreatedBy
            model.Page.CreatedBy = 1;

            if (ModelState.IsValid)
            {
                try
                {
                    var newPage = await _pageService.CreatePageAsync(model.Page);
                    ShowAlertSuccess($"Added page: {newPage.Title}");
                    return RedirectToAction(nameof(AdminList));
                }
                catch(Exception ex)
                {
                    ShowAlertDanger("Unable to add page: ", ex.Message);
                }
            }

            model.Action = nameof(AdminCreate);
            return View("AdminDetail", model);
        }

        public async Task<IActionResult> AdminEdit(int id)
        {
            var viewModel = new AdminDetailViewModel
            {
                Action = nameof(AdminEdit),
                Page = await _pageService.GetPageByIdAsync(id)
            };

            return View("AdminDetail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AdminEdit(AdminDetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.Page.SectionId = 1; //TODO: Use actual SectionId
                    var page = await _pageService.EditPageAsync(model.Page);
                    ShowAlertSuccess($"Updated page: {page.Title}");
                    return RedirectToAction(nameof(AdminList));
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to update page: ", ex.Message);
                }
            }

            model.Action = nameof(AdminEdit);
            return View("AdminDetail", model);
        }

        [HttpPost]
        public async Task<IActionResult> AdminDelete(AdminListViewModel model)
        {
            try
            {
                await _pageService.DeletePageAsync(model.Page.Id);
                ShowAlertSuccess("Page deleted successfully.");
            }
            catch(Exception ex)
            {
                ShowAlertDanger("Unable to delete page: ", ex.Message);
            }

            return RedirectToAction(nameof(AdminList), new { page = model.PaginateModel.CurrentPage });
        }
    }
}
