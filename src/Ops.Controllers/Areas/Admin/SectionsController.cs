﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Sections;
using Ocuda.Ops.Controllers.Key;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    public class SectionsController : BaseController<SectionsController>
    {
        private readonly SectionService _sectionService;

        public SectionsController(ServiceFacade.Controller<SectionsController> context,
            SectionService sectionService) : base(context)
        {
            _sectionService = sectionService 
                ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var itemsPerPage = await _siteSettingService.GetSetting(SiteSettingKey.Pagination.ItemsPerPage);
            int.TryParse(itemsPerPage, out int take);

            var filter = new BlogFilter(page, take);

            var sectionList = await _sectionService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel()
            {
                ItemCount = sectionList.Count,
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
                Sections = sectionList.Data
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            var viewModel = new DetailViewModel
            {
                Action = nameof(Create),
                IsReadonly = false
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
                    var newSection = await _sectionService.CreateAsync(CurrentUserId, model.Section);
                    ShowAlertSuccess($"Added section: {newSection.Name}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error adding section: {ex}", ex);
                    ShowAlertDanger("Unable to add section: ", ex.Message);
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
                Section = await _sectionService.GetByIdAsync(id),
            };

            viewModel.IsReadonly = string.IsNullOrWhiteSpace(viewModel.Section.Path) ? true : false;

            return View("Detail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var section = await _sectionService.EditAsync(model.Section);
                    ShowAlertSuccess($"Updated section: {section.Name}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating section: {ex}", ex);
                    ShowAlertDanger("Unable to update section: ", ex.Message);
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
                await _sectionService.DeleteAsync(model.Section.Id);
                ShowAlertSuccess("Section deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting section: {ex}", ex);
                ShowAlertDanger("Unable to delete section: ", ex.Message);
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }
    }
}
