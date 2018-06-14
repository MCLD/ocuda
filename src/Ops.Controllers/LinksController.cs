using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Links;
using Ocuda.Utility.Models;
using Ops.Service;

namespace Ocuda.Ops.Controllers
{
    public class LinksController : BaseController
    {
        private readonly LinkService _linkService;

        public LinksController(LinkService linkService)
        {
            _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var linkList = await _linkService.GetLinksAsync();

            var paginateModel = new PaginateModel()
            {
                ItemCount = await _linkService.GetLinkCountAsync(),
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

            var viewModel = new LinkListViewModel()
            {
                PaginateModel = paginateModel,
                Links = linkList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                        .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> AdminList(int page = 1)
        {
            var linkList = await _linkService.GetLinksAsync();

            var paginateModel = new PaginateModel()
            {
                ItemCount = await _linkService.GetLinkCountAsync(),
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

            var viewModel = new AdminListViewModel()
            {
                PaginateModel = paginateModel,
                Links = linkList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                        .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        public IActionResult AdminCreate()
        {
            var viewModel = new AdminDetailViewModel()
            {
                Action = nameof(AdminCreate)
            };

            return View("AdminDetail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AdminCreate(AdminDetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var newLink = await _linkService.CreateLinkAsync(model.Link);
                    ShowAlertSuccess($"Added link: {newLink.Name}");
                    return RedirectToAction(nameof(AdminList));
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to add link: ", ex.Message);
                }
            }

            model.Action = nameof(AdminCreate);
            return View("AdminDetail", model);
        }

        public async Task<IActionResult> AdminEdit(int id)
        {
            var viewModel = new AdminDetailViewModel()
            {
                Action = nameof(AdminEdit),
                Link = await _linkService.GetLinkByIdAsync(id)
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
                    var link = await _linkService.EditLinkAsync(model.Link);
                    ShowAlertSuccess($"Updated link: {link.Name}");
                    return RedirectToAction(nameof(AdminList));
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to update link: ", ex.Message);
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
                await _linkService.DeleteLinkAsync(model.Link.Id);
                ShowAlertSuccess("Link deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowAlertDanger("Unable to delete link: ", ex.Message);
            }

            return RedirectToAction(nameof(AdminList), new { page = model.PaginateModel.CurrentPage });
        }

        public IActionResult AdminCategories(int page = 1)
        {
            var categoryList = _linkService.GetLinkCategories();

            var paginateModel = new PaginateModel()
            {
                ItemCount = categoryList.Count(),
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

            var viewModel = new AdminCategoriesViewModel()
            {
                PaginateModel = paginateModel,
                Categories = categoryList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                            .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> AdminCategoryCreate(AdminCategoriesViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var newCategory = await _linkService.CreateLinkCategoryAsync(model.Category);
                    ShowAlertSuccess($"Added link category: {newCategory.Name}");
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to add category: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(AdminCategories), new { page = model.PaginateModel.CurrentPage });
        }

        [HttpPost]
        public async Task<IActionResult> AdminCategoryEdit(AdminCategoriesViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _linkService.EditLinkCategoryAsync(model.Category);
                    ShowAlertSuccess($"Updated link category: {category.Name}");
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to update category: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(AdminCategories), new { page = model.PaginateModel.CurrentPage });
        }

        [HttpPost]
        public async Task<IActionResult> AdminCategoryDelete(AdminCategoriesViewModel model)
        {
            try
            {
                await _linkService.DeleteLinkCategoryAsync(model.Category.Id);
                ShowAlertSuccess("Link category deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowAlertDanger("Unable to delete category: ", ex.Message);
            }

            return RedirectToAction(nameof(AdminCategories), new { page = model.PaginateModel.CurrentPage });
        }
    }
}
