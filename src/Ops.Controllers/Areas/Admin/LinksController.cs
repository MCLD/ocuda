using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Links;
using Ocuda.Ops.Controllers.Authorization;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(SectionManagerRequirement))]
    public class LinksController : BaseController<LinksController>
    {
        private readonly ILinkService _linkService;
        private readonly ISectionService _sectionService;

        public LinksController(ServiceFacades.Controller<LinksController> context,
            ILinkService linkService,
            ISectionService sectionService) : base(context)
        {
            _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
        }

        #region Libraries
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Index(string section, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BlogFilter(page, itemsPerPage)
            {
                SectionId = currentSection.Id
            };

            var libraryList = await _linkService.GetPaginatedLibraryListAsync(filter);

            var paginateModel = new PaginateModel()
            {
                ItemCount = libraryList.Count,
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

            var viewModel = new IndexViewModel()
            {
                PaginateModel = paginateModel,
                Libraries = libraryList.Data,
                SectionId = currentSection.Id
            };

            return View(viewModel);
        }


        [HttpPost]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> CreateLibrary(IndexViewModel model)
        {
            var message = string.Empty;
            var success = true;

            try
            {
                var newLibrary = await _linkService
                    .CreateLibraryAsync(CurrentUserId, model.LinkLibrary);
                ShowAlertSuccess($"Added link library: {newLibrary.Name}");
            }
            catch (OcudaException ex)
            {
                message = ex.Message;
                success = false;
            }

            return Json(new { success, message });
        }

        [HttpPost]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> EditLibrary(IndexViewModel model)
        {
            var message = string.Empty;
            var success = true;

            try
            {
                var library = await _linkService.EditLibraryAsync(model.LinkLibrary);
                ShowAlertSuccess($"Updated link library: {library.Name}");
            }
            catch (OcudaException ex)
            {
                message = ex.Message;
                success = false;
            }

            return Json(new { success, message });
        }

        [HttpPost]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> DeleteLibrary(IndexViewModel model)
        {
            try
            {
                await _linkService.DeleteLibraryAsync(model.LinkLibrary.Id);
                ShowAlertSuccess($"Delete link library: {model.LinkLibrary.Name}.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting link library: {ex}", ex);
                ShowAlertDanger("Unable to delete library: ", ex.Message);
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }
        #endregion

        #region Links
        public async Task<IActionResult> Library(string section, int id, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var currentLibrary = await _linkService.GetLibraryByIdAsync(id);

            if (currentLibrary?.SectionId != currentSection.Id)
            {
                return RedirectToAction(nameof(Library));
            }

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BlogFilter(page, itemsPerPage)
            {
                LinkLibraryId = id
            };

            var linkList = await _linkService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel()
            {
                ItemCount = linkList.Count,
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

            var viewModel = new LibraryViewModel()
            {
                PaginateModel = paginateModel,
                Links = linkList.Data,
                LinkLibrary = currentLibrary
            };

            return View(viewModel);
        }

        [RestoreModelState]
        public async Task<IActionResult> Create(string section, int libraryId)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var currentLibrary = await _linkService.GetLibraryByIdAsync(libraryId);

            if (currentLibrary?.SectionId != currentSection.Id)
            {
                return RedirectToAction(nameof(Library));
            }

            var viewModel = new DetailViewModel()
            {
                Action = nameof(Create),
                LibraryId = currentLibrary.Id
            };

            return View("Detail", viewModel);
        }

        [HttpPost]
        [SaveModelState]
        public async Task<IActionResult> Create(DetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.Link.LinkLibraryId = model.LibraryId;
                    var newLink = await _linkService.CreateAsync(CurrentUserId, model.Link);

                    ShowAlertSuccess($"Added link: {newLink.Name}");
                    return RedirectToAction(nameof(Library), new { id = model.LibraryId });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger("Unable to add link: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Create), new { libraryId = model.LibraryId });
        }

        [RestoreModelState]
        public async Task<IActionResult> Edit(string section, int id)
        {
            var link = await _linkService.GetByIdAsync(id);

            var currentSection = await _sectionService.GetByPathAsync(section);
            var currentLibrary = await _linkService.GetLibraryByIdAsync(link.LinkLibraryId);

            if (currentLibrary?.SectionId != currentSection.Id)
            {
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new DetailViewModel()
            {
                Action = nameof(Edit),
                LibraryId = currentLibrary.Id,
                Link = link
            };

            return View("Detail", viewModel);
        }

        [HttpPost]
        [SaveModelState]
        public async Task<IActionResult> Edit(DetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var link = await _linkService.EditAsync(model.Link);
                    ShowAlertSuccess($"Updated link: {link.Name}");
                    return RedirectToAction(nameof(Library), new { id = model.LibraryId });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger("Unable to update link: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Edit));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(LibraryViewModel model)
        {
            try
            {
                await _linkService.DeleteAsync(model.Link.Id);
                ShowAlertSuccess($"Deleted link: {model.Link.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting link: {ex}", ex);
                ShowAlertDanger("Unable to delete link: ", ex.Message);
            }

            return RedirectToAction(nameof(Library), new
            {
                id = model.LinkLibrary.Id,
                page = model.PaginateModel.CurrentPage
            });
        }
        #endregion
    }
}
