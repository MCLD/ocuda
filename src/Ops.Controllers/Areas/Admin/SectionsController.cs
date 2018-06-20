using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Sections;
using Ocuda.Ops.Service;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    public class SectionsController : BaseController
    {
        private readonly SectionService _sectionService;

        public SectionsController(SectionService sectionService)
        {
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var sectionList = await _sectionService.GetSectionsAsync();

            var paginateModel = new PaginateModel()
            {
                ItemCount = await _sectionService.GetSectionCountAsync(),
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

            var viewModel = new IndexViewModel
            {
                PaginateModel = paginateModel,
                Sections = sectionList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                                .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            var viewModel = new DetailViewModel
            {
                Action = nameof(Create),
                IsReadonly = null
            };

            return View("Detail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DetailViewModel model)
        {
            model.Section.CreatedBy = 1; //TODO fix CreatedBy

            if (ModelState.IsValid)
            {
                try
                {
                    var newSection = await _sectionService.CreateSectionAsync(model.Section);
                    ShowAlertSuccess($"Added section: {newSection.Name}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
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
                Section = await _sectionService.GetSectionByIdAsync(id),
            };

            viewModel.IsReadonly =
                string.IsNullOrWhiteSpace(viewModel.Section.Path) ? "readonly" : null;

            return View("Detail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var section = await _sectionService.EditSectionAsync(model.Section);
                    ShowAlertSuccess($"Updated section: {section.Name}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
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
                await _sectionService.DeleteSectionAsync(model.Section.Id);
                ShowAlertSuccess("Section deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowAlertDanger("Unable to delete section: ", ex.Message);
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }
    }
}
