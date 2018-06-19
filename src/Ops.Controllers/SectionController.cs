using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.ViewModels.Section;
using Ocuda.Utility.Models;
using Ops.Service;

namespace Ocuda.Ops.Controllers
{
    public class SectionController : Abstract.BaseController
    {
        private readonly SectionService _sectionService;

        public SectionController(SectionService sectionService)
        {
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public IActionResult Index(int page = 1)
        {
            var sectionList = _sectionService.GetAll();

            var paginateModel = new PaginateModel()
            {
                ItemCount = sectionList.Count(),
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
                    return RedirectToAction(nameof(Index));
                }
                catch(Exception ex)
                {
                    ShowAlertDanger("Unable to add section: ", ex.Message);
                }
            }

            model.Action = nameof(AdminCreate);
            return View("AdminDetail", model);
        }

        public IActionResult AdminEdit(int id)
        {
            var viewModel = new AdminDetailViewModel
            {
                Action = nameof(AdminEdit),
                Section = _sectionService.GetSectionById(id),
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
                    return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> AdminDelete(SectionListViewModel model)
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

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }
    }
}
