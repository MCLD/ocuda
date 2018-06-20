using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Files;
using Ocuda.Utility.Models;
using Ops.Service;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    public class FilesController : BaseController
    {
        private readonly FileService _fileService;

        public FilesController(FileService fileService)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var fileList = await _fileService.GetFilesAsync();

            var paginateModel = new PaginateModel()
            {
                ItemCount = await _fileService.GetFileCountAsync(),
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

            var viewModel = new IndexViewModel()
            {
                PaginateModel = paginateModel,
                Files = fileList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                        .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            var viewModel = new DetailViewModel()
            {
                Action = nameof(Create)
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
                    model.File.SectionId = 1; //TODO: Use actual SectionId
                    var newFile = await _fileService.CreateFileAsync(model.File);
                    ShowAlertSuccess($"Added file: {newFile.Name}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to add file: ", ex.Message);
                }
            }

            model.Action = nameof(Create);
            return View("Detail", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var viewModel = new DetailViewModel()
            {
                Action = nameof(Edit),
                File = await _fileService.GetFileByIdAsync(id)
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
                    model.File.SectionId = 1; //TODO: Use actual SectionId
                    var file = await _fileService.EditFileAsync(model.File);
                    // Save file data logic here
                    ShowAlertSuccess($"Updated file: {file.Name}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to update file: ", ex.Message);
                }
            }

            model.Action = nameof(Create);
            return View("Detail", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(IndexViewModel model)
        {
            try
            {
                await _fileService.DeleteFileAsync(model.File.Id);
                ShowAlertSuccess("File deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowAlertDanger("Unable to delete file: ", ex.Message);
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }

        public IActionResult Categories(int page = 1)
        {
            var categoryList = _fileService.GetFileCategories();

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

            var viewModel = new CategoriesViewModel()
            {
                PaginateModel = paginateModel,
                Categories = categoryList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                            .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoriesViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var newCategory = await _fileService.CreateFileCategoryAsync(model.Category);
                    ShowAlertSuccess($"Added file category: {newCategory.Name}");
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to add category: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Categories), new { page = model.PaginateModel.CurrentPage });
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(CategoriesViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _fileService.EditFileCategoryAsync(model.Category);
                    ShowAlertSuccess($"Updated file category: {category.Name}");
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to update category: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Categories), new { page = model.PaginateModel.CurrentPage });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(CategoriesViewModel model)
        {
            try
            {
                await _fileService.DeleteFileCategoryAsync(model.Category.Id);
                ShowAlertSuccess("File category deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowAlertDanger("Unable to delete category: ", ex.Message);
            }

            return RedirectToAction(nameof(Categories), new { page = model.PaginateModel.CurrentPage });
        }
    }
}
