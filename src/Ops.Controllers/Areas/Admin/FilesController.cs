using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Files;
using Ocuda.Ops.Controllers.Authorization;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(SectionManagerRequirement))]
    public class FilesController : BaseController
    {
        private readonly ILogger<FilesController> _logger;
        private readonly CategoryService _categoryService;
        private readonly FileService _fileService;
        private readonly FileTypeService _fileTypeService;
        private readonly SectionService _sectionService;

        private const string FileValidationPassed = "Valid";
        private const string FileValidationFailedType = "File is not a valid type.";
        private const string FileValidationFailedSize = "File is too large to upload.";
        private const int MaxFileSize = 2096000; //TODO get max filesize from config

        public FilesController(ILogger<FilesController> logger,
            ServiceFacade.Controller context,
            CategoryService categoryService,
            FileTypeService fileTypeService,
            FileService fileService,
            SectionService sectionService) : base(context)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _fileTypeService = fileTypeService ?? throw new ArgumentNullException(nameof(fileTypeService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

                    if (model.FileData != null)
                    {
                        byte[] fileBytes;

                        using (var fileStream = model.FileData.OpenReadStream())
                        {
                            using (var ms = new System.IO.MemoryStream())
                            {
                                fileStream.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }
                        }

                        if (model.FileData.Length < MaxFileSize)
                        {
                            var typeProvider = new FileExtensionContentTypeProvider();
                            typeProvider.TryGetContentType(model.FileData.FileName, out string contentType);

                            if (!string.IsNullOrWhiteSpace(contentType))
                            {
                                model.File.Extension = System.IO.Path.GetExtension(model.FileData.FileName);

                                var fileType = await _fileTypeService.GetByExtensionAsync(model.File.Extension);
                                model.File.Icon = fileType.Icon;

                                var newFile = await _fileService.CreateAsync(model.File, fileBytes);

                                ShowAlertSuccess($"Added file: {newFile.Name}");
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                ShowAlertDanger(FileValidationFailedType);
                            }
                        }
                        else
                        {
                            ShowAlertDanger(FileValidationFailedSize);
                        }
                    }
                    else
                    {
                        ShowAlertDanger("No file selected.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error creating file: {ex}", ex);
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
                    File file;
                    model.File.SectionId = model.SectionId;

                    if (model.FileData != null)
                    {
                        byte[] fileBytes;

                        using (var fileStream = model.FileData.OpenReadStream())
                        {
                            using (var ms = new System.IO.MemoryStream())
                            {
                                fileStream.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }
                        }

                        if (model.FileData.Length < MaxFileSize)
                        {
                            var typeProvider = new FileExtensionContentTypeProvider();
                            typeProvider.TryGetContentType(model.FileData.FileName, out string contentType);

                            if (!string.IsNullOrWhiteSpace(contentType))
                            {
                                model.File.Extension = System.IO.Path.GetExtension(model.FileData.FileName);

                                var fileType = await _fileTypeService.GetByExtensionAsync(model.File.Extension);
                                model.File.Icon = fileType.Icon;

                                file = await _fileService.EditAsync(model.File, fileBytes);
                                ShowAlertSuccess($"Updated file: {file.Name}");
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                ShowAlertDanger(FileValidationFailedType);
                            }
                        }
                        else
                        {
                            ShowAlertDanger(FileValidationFailedSize);
                        }
                    }
                    else
                    {
                        file = await _fileService.EditAsync(model.File);
                        ShowAlertSuccess($"Updated file: {file.Name}");
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error editing file: {ex}", ex);
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
                _logger.LogError($"Error deleting file: {ex}", ex);
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
                    _logger.LogError($"Error creating file category: {ex}", ex);
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
                    _logger.LogError($"Error editing file: {ex}", ex);
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
                _logger.LogError($"Error deleting file category: {ex}", ex);
                ShowAlertDanger("Unable to delete category: ", ex.Message);
            }

            return RedirectToAction(nameof(Categories), new { page = model.PaginateModel.CurrentPage });
        }

        public async Task<IActionResult> ViewFile(int id)
        {
            var file = await _fileService.GetByIdAsync(id);
            var extension = System.IO.Path.GetExtension(file.FilePath);
            var fileName = $"{file.Name}{extension}";
            byte[] fileBytes;

            try
            {
                using (var fileStream = System.IO.File.OpenRead(file.FilePath))
                {
                    using (var ms = new System.IO.MemoryStream())
                    {
                        fileStream.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }
                }

                var typeProvider = new FileExtensionContentTypeProvider();
                typeProvider.TryGetContentType(file.FilePath, out string fileType);

                Response.Headers.Add("Content-Disposition", "inline; filename=" + fileName);
                return File(fileBytes, fileType);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error viewing file {file.Id} : {ex}", ex);
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }

        public IActionResult ValidateFileBeforeUpload(string fileName, int fileSize)
        {
            string result = ValidateFile(fileName, fileSize);
            return Json(result);
        }

        public async Task<IActionResult> UploadAttachment(IFormFile fileData, int fileSize, int sectionId)
        {
            string result = FileValidationPassed;  //TODO File Validation

            if (result == FileValidationPassed)
            {
                try
                {
                    if (fileData != null)
                    {
                        var section = await _sectionService.GetByIdAsync(sectionId);
                        var category = await _categoryService.GetAttachmentCategoryAsync(section.Id);

                        File file = new File
                        {
                            Name = fileData.FileName,
                            Description = "Attachment",
                            IsFeatured = false,
                            SectionId = section.Id,
                            CategoryId = category.Id
                        };

                        byte[] fileBytes;

                        using (var fileStream = fileData.OpenReadStream())
                        {
                            using (var ms = new System.IO.MemoryStream())
                            {
                                fileStream.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }

                            file.Extension = System.IO.Path.GetExtension(fileData.FileName);

                            var fileType = await _fileTypeService.GetByExtensionAsync(file.Extension);
                            file.Icon = fileType.Icon;

                            var newFile = await _fileService.CreateAsync(file, fileBytes);
                            _logger.LogInformation($"Attached file: {newFile.FilePath}");

                            string sectionPath = null;
                            if (section.Path != null)
                            {
                                sectionPath = $"/{section.Path}";
                            }

                            var filePath = HttpContext.Request.Host +
                                $"{sectionPath}/Files/{nameof(FilesController.ViewFile)}/{newFile.Id}";
                            result = filePath;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error creating file: {ex}", ex);
                    ShowAlertDanger("Unable to add file: ", ex.Message);
                }
            }

            return Json(result);
        }

        private string ValidateFile(string fileName, int fileSize)
        {
            var result = "";
            var maxFileSize = MaxFileSize; //TODO get max filesize from config

            if (fileSize < maxFileSize)
            {
                var typeProvider = new FileExtensionContentTypeProvider();
                typeProvider.TryGetContentType(fileName, out string fileType);

                if (string.IsNullOrWhiteSpace(fileType))
                {
                    result = FileValidationFailedType;
                }
                else
                {
                    result = FileValidationPassed;
                }
            }
            else
            {
                result = FileValidationFailedSize;
            }

            _logger.LogInformation($"Validation for \"{fileName}\": {result}", fileName, result);

            return result;
        }
    }
}
