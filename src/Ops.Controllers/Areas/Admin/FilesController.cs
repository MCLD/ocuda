using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Files;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    public class FilesController : BaseController
    {
        private readonly FileService _fileService;
        private readonly CategoryService _categoryService;
        private readonly SectionService _sectionService;

        public FilesController(FileService fileService, 
            CategoryService categoryService,
            SectionService sectionService
            )
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public async Task<IActionResult> Index(string section, int? categoryId = null, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);

            var filter = new BlogFilter(page)
            {
                SectionId = currentSection.Id,
                CategoryId = categoryId,
                CategoryType = CategoryType.File
            };

            var fileList = await _fileService.GetPaginatedListAsync(filter);
            var categoryList = await _categoryService.GetBySectionIdAsync(filter);

            var paginateModel = new PaginateModel()
            {
                ItemCount = fileList.Count,
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
                Files = fileList.Data,
                Categories = categoryList
            };

            if (categoryId.HasValue)
            {
                viewModel.CategoryName =
                    (await _categoryService.GetCategoryByIdAsync(categoryId.Value)).Name;
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Create(string section)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);

            var filter = new BlogFilter()
            {
                SectionId = currentSection.Id,
                CategoryType = CategoryType.File
            };

            var categories = await _categoryService.GetBySectionIdAsync(filter);

            var viewModel = new DetailViewModel()
            {
                Action = nameof(Create),
                SectionId = currentSection.Id,
                Categories = categories
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
                    model.File.SectionId = model.SectionId;
                    var newFile = await _fileService.CreateAsync(model.File);
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
            var file = await _fileService.GetByIdAsync(id);

            var filter = new BlogFilter()
            {
                SectionId = file.SectionId,
                CategoryType = CategoryType.File
            };

            var categories = await _categoryService.GetBySectionIdAsync(filter);

            var viewModel = new DetailViewModel()
            {
                Action = nameof(Edit),
                SectionId = file.SectionId,
                File = file,
                Categories = categories
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
                    var file = await _fileService.EditAsync(model.File);
                    // Save file data logic here
                    ShowAlertSuccess($"Updated file: {file.Name}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to update file: ", ex.Message);
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
                await _fileService.DeleteAsync(model.File.Id);
                ShowAlertSuccess("File deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowAlertDanger("Unable to delete file: ", ex.Message);
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }

        public async Task<IActionResult> Categories(string section, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);

            var filter = new BlogFilter(page)
            {
                SectionId = currentSection.Id,
                CategoryType = CategoryType.File
            };

            var categoryList = await _categoryService.GetBySectionIdAsync(filter);

            var paginateModel = new PaginateModel()
            {
                ItemCount = categoryList.Count,
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

            var viewModel = new CategoriesViewModel()
            {
                PaginateModel = paginateModel,
                Categories = categoryList,
                SectionId = currentSection.Id
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
                    model.Category.SectionId = model.SectionId;
                    model.Category.CategoryType = CategoryType.File;
                    var newCategory = await _categoryService.CreateCategoryAsync(model.Category);
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
                    var category = await _categoryService.EditCategoryAsync(model.Category);
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
                await _categoryService.DeleteCategoryAsync(model.Category.Id);
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
