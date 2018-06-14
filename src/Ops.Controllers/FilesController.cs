using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Files;
using Ocuda.Utility.Models;
using Ops.Service;
using System.Linq;
using System.Threading.Tasks;

namespace Ocuda.Ops.Controllers
{
    public class FilesController : BaseController
    {
        private readonly SectionService _sectionService;

        public FilesController(SectionService sectionService)
        {
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public IActionResult Index(int page = 1)
        {
            var fileList = _sectionService.GetFiles();

            var paginateModel = new PaginateModel()
            {
                ItemCount = fileList.Count(),
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

            var viewModel = new FileListViewModel()
            {
                PaginateModel = paginateModel,
                SectionFiles = fileList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                        .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        public IActionResult AdminList(int page = 1)
        {
            var fileList = _sectionService.GetFiles();

            var paginateModel = new PaginateModel()
            {
                ItemCount = fileList.Count(),
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
                SectionFiles = fileList.Skip((page - 1) * paginateModel.ItemsPerPage)
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
            if(ModelState.IsValid)
            {
                try
                {
                    var newFile = await _sectionService.CreateSectionFileAsync(model.SectionFile);
                    ShowAlertSuccess($"Added file: {newFile.Name}");
                    return RedirectToAction(nameof(AdminList));
                }
                catch(Exception ex)
                {
                    ShowAlertDanger("Unable to add file: ", ex.Message);
                }
            }

            model.Action = nameof(AdminCreate);
            return View("AdminDetail", model);
        }

        public IActionResult AdminEdit(int id)
        {
            var viewModel = new AdminDetailViewModel()
            {
                Action = nameof(AdminEdit),
                SectionFile = _sectionService.GetSectionFileById(id)
            };

            return View("AdminDetail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AdminEdit(AdminDetailViewModel model)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var file = await _sectionService.EditSectionFileAsync(model.SectionFile);
                    // Save file data logic here
                    ShowAlertSuccess($"Updated file: {file.Name}");
                    return RedirectToAction(nameof(AdminList));
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to add file: ", ex.Message);
                }
            }

            model.Action = nameof(AdminCreate);
            return View("AdminDetail", model);
        }

        [HttpPost]
        public async Task<IActionResult> AdminDelete(AdminListViewModel model)
        {
            try
            {
                await _sectionService.DeleteSectionFileAsync(model.SectionFile.Id);
                ShowAlertSuccess("File deleted successfully.");
            }
            catch(Exception ex)
            {
                ShowAlertDanger("Unable to delete file: ", ex.Message);
            }

            return RedirectToAction(nameof(AdminList), new { page = model.PaginateModel.CurrentPage });
        }
    }
}
