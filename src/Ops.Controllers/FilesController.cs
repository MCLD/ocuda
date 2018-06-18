using System;
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

            var viewModel = new FileListViewModel()
            {
                PaginateModel = paginateModel,
                Files = fileList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                        .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> AdminList(int page = 1)
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

            var viewModel = new AdminListViewModel()
            {
                PaginateModel = paginateModel,
                Files = fileList.Skip((page - 1) * paginateModel.ItemsPerPage)
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
                    var newFile = await _fileService.CreateFileAsync(model.File);
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

        public async Task<IActionResult> AdminEdit(int id)
        {
            var viewModel = new AdminDetailViewModel()
            {
                Action = nameof(AdminEdit),
                File = await _fileService.GetFileByIdAsync(id)
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
                    var file = await _fileService.EditFileAsync(model.File);
                    // Save file data logic here
                    ShowAlertSuccess($"Updated file: {file.Name}");
                    return RedirectToAction(nameof(AdminList));
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to update file: ", ex.Message);
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
                await _fileService.DeleteFileAsync(model.File.Id);
                ShowAlertSuccess("File deleted successfully.");
            }
            catch(Exception ex)
            {
                ShowAlertDanger("Unable to delete file: ", ex.Message);
            }

            return RedirectToAction(nameof(AdminList), new { page = model.PaginateModel.CurrentPage });
        }

        public IActionResult AdminCategories(int page = 1)
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
                    var newCategory = await _fileService.CreateFileCategoryAsync(model.Category);
                    ShowAlertSuccess($"Added file category: {newCategory.Name}");
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
                    var category = await _fileService.EditFileCategoryAsync(model.Category);
                    ShowAlertSuccess($"Updated file category: {category.Name}");
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
                await _fileService.DeleteFileCategoryAsync(model.Category.Id);
                ShowAlertSuccess("File category deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowAlertDanger("Unable to delete category: ", ex.Message);
            }

            return RedirectToAction(nameof(AdminCategories), new { page = model.PaginateModel.CurrentPage });
        }
    }
}
