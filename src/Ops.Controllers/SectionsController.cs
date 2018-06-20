using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.ViewModels.Sections;
using Ocuda.Utility.Models;
using Ops.Service;

namespace Ocuda.Ops.Controllers
{
    public class SectionsController : Abstract.BaseController
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

            var viewModel = new SectionListViewModel
            {
                PaginateModel = paginateModel,
                Sections = sectionList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                                .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> AdminList(int page = 1)
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

            var viewModel = new AdminListViewModel
            {
                PaginateModel = paginateModel,
                Sections = sectionList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                                .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        public IActionResult AdminCreate()
        {
            var viewModel = new AdminDetailViewModel
            {
                Action = nameof(AdminCreate),
                IsReadonly = null
            };

            return View("AdminDetail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AdminCreate(AdminDetailViewModel model)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var newSection = await _sectionService.CreateSectionAsync(model.Section);
                    ShowAlertSuccess($"Added section: {newSection.Name}");
                    return RedirectToAction(nameof(AdminList));
                }
                catch(Exception ex)
                {
                    ShowAlertDanger("Unable to add section: ", ex.Message);
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
                Section = await _sectionService.GetSectionByIdAsync(id),
            };

            viewModel.IsReadonly = 
                string.IsNullOrWhiteSpace(viewModel.Section.Path) ? "readonly" : null;

            return View("AdminDetail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AdminEdit(AdminDetailViewModel model)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var section = await _sectionService.EditSectionAsync(model.Section);
                    ShowAlertSuccess($"Updated section: {section.Name}");
                    return RedirectToAction(nameof(AdminList));
                }
                catch(Exception ex)
                {
                    ShowAlertDanger("Unable to update section: ", ex.Message);
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
                await _sectionService.DeleteSectionAsync(model.Section.Id);
                ShowAlertSuccess("Section deleted successfully.");
            }
            catch(Exception ex)
            {
                ShowAlertDanger("Unable to delete section: ", ex.Message);
            }

            return RedirectToAction(nameof(AdminList), new { page = model.PaginateModel.CurrentPage });
        }
    }
}
