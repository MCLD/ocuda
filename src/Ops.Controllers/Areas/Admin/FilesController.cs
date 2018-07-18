using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Files;
using Ocuda.Ops.Controllers.Authorization;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(SectionManagerRequirement))]
    public class FilesController : BaseController<FilesController>
    {
        private readonly ICategoryService _categoryService;
        private readonly IFileService _fileService;
        private readonly IFileTypeService _fileTypeService;
        private readonly IPageService _pageService;
        private readonly IPathResolverService _pathResolver;
        private readonly IPostService _postService;
        private readonly ISectionService _sectionService;

        private const string FileValidationPassed = "Valid";
        private const string FileValidationFailedType = "File is not a valid type.";
        private const string FileValidationFailedSize = "File is too large to upload.";

        public const string DefaultCategoryDisplayName = "[No Category]";

        public FilesController(ServiceFacade.Controller<FilesController> context,
            ICategoryService categoryService,
            IFileService fileService,
            IFileTypeService fileTypeService,
            IPageService pageService,
            IPathResolverService pathResolver,
            IPostService postService,
            ISectionService sectionService) : base(context)
        {
            _categoryService = categoryService
                ?? throw new ArgumentNullException(nameof(categoryService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _fileTypeService = fileTypeService
                ?? throw new ArgumentNullException(nameof(fileTypeService));
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public async Task<IActionResult> Index(string section, int? categoryId = null, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(SiteSettingKey.Pagination.ItemsPerPage);

            var filter = new BlogFilter(page, itemsPerPage)
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
                var name = (await _categoryService.GetCategoryByIdAsync(categoryId.Value)).Name;
                viewModel.CategoryName = 
                    string.IsNullOrWhiteSpace(name) ? DefaultCategoryDisplayName : name;
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

                        var maxFileSize = await _siteSettingService
                            .GetSettingIntAsync(SiteSettingKey.FileUpload.MaxFileSize);

                        if (model.FileData.Length < maxFileSize)
                        {
                            var typeProvider = new FileExtensionContentTypeProvider();
                            typeProvider.TryGetContentType(model.FileData.FileName, out string contentType);

                            if (!string.IsNullOrWhiteSpace(contentType))
                            {
                                model.File.Extension = System.IO.Path.GetExtension(model.FileData.FileName);

                                var fileType = await _fileTypeService.GetByExtensionAsync(model.File.Extension);
                                model.File.Icon = fileType.Icon;

                                // TODO add logic to this, make it constants
                                model.File.Type = "postattachment";

                                var newFile = await _fileService.CreatePrivateFileAsync(CurrentUserId, model.File, fileBytes);

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

                        var maxFileSize = await _siteSettingService
                            .GetSettingIntAsync(SiteSettingKey.FileUpload.MaxFileSize);

                        if (model.FileData.Length < maxFileSize)
                        {
                            var typeProvider = new FileExtensionContentTypeProvider();
                            typeProvider.TryGetContentType(model.FileData.FileName, out string contentType);

                            if (!string.IsNullOrWhiteSpace(contentType))
                            {
                                model.File.Extension = System.IO.Path.GetExtension(model.FileData.FileName);

                                var fileType = await _fileTypeService.GetByExtensionAsync(model.File.Extension);
                                model.File.Icon = fileType.Icon;

                                file = await _fileService.EditPrivateFileAsync(model.File, fileBytes);
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
                        file = await _fileService.EditPrivateFileAsync(model.File);
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
                await _fileService.DeletePrivateFileAsync(model.File.Id);
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
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(SiteSettingKey.Pagination.ItemsPerPage);

            var filter = new BlogFilter(page, itemsPerPage)
            {
                SectionId = currentSection.Id,
                CategoryType = CategoryType.File
            };

            var categoryList = await _categoryService.GetPaginatedCategoryListAsync(filter);

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
                Categories = categoryList.Data,
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
                    var newCategory = await _categoryService.CreateCategoryAsync(CurrentUserId, model.Category);
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

        public async Task<IActionResult> ViewPrivateFile(int id)
        {
            var file = await _fileService.GetByIdAsync(id);
            var fileBytes = await _fileService.ReadPrivateFileAsync(file);
            string fileName = $"{file.Name}{file.Extension}";
            try
            {
                var typeProvider = new FileExtensionContentTypeProvider();
                typeProvider.TryGetContentType(fileName, out string fileType);

                Response.Headers.Add("Content-Disposition", "inline; filename=" + fileName);
                return File(fileBytes, fileType);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error viewing file {file.Id} : {ex}", ex);
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }

        public async Task<IActionResult> ValidateFileBeforeUpload(string fileName, long fileSize)
        {
            string result = await ValidateFile(fileName, fileSize);
            return Json(result);
        }

        public async Task<IActionResult> UploadAttachment(IFormFile fileData,
            int sectionId,
            int contentId,
            string contentType)
        {
            string result = await ValidateFile(fileData.FileName, fileData.Length);

            if (result == FileValidationPassed)
            {
                try
                {
                    if (fileData != null)
                    {
                        var section = await _sectionService.GetByIdAsync(sectionId);
                        var category = 
                            await _categoryService.GetAttachmentCategoryAsync(CurrentUserId, section.Id);

                        File file = new File
                        {
                            Name = fileData.FileName,
                            IsFeatured = false,
                            SectionId = section.Id,
                            CategoryId = category.Id,
                        };

                        if (contentType == nameof(Post))
                        {
                            var post = await _postService.GetByIdAsync(contentId);
                            file.Description = $"Attached to post: {post.Stub}";
                            file.PostId = contentId;
                            file.PageId = null;
                        }
                        else if (contentType == nameof(Page))
                        {
                            var page = await _pageService.GetByIdAsync(contentId);
                            file.Description = $"Attached to page: {page.Stub}";
                            file.PageId = contentId;
                            file.PostId = null;
                        }

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

                            // TODO make this constant
                            file.Type = "postattachment";
                            var newFile = await _fileService.CreatePrivateFileAsync(CurrentUserId, file, fileBytes);
                            _logger.LogInformation($"Attached file: {newFile.Name}{newFile.Extension}");

                            string sectionPath = null;
                            if (section.Path != null)
                            {
                                sectionPath = $"/{section.Path}";
                            }

                            var filePath = HttpContext.Request.Host +
                                $"{sectionPath}/Files/{nameof(FilesController.ViewPrivateFile)}/{newFile.Id}";
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
            else if (result == FileValidationFailedSize)
            {
                result = "FailedSize";
            }
            else if (result == FileValidationFailedType)
            {
                result = "FailedType";
            }

            return Json(result);
        }

        private async Task<string> ValidateFile(string fileName, long fileSize)
        {
            var result = "";
            var maxFileSize = await _siteSettingService
                .GetSettingIntAsync(SiteSettingKey.FileUpload.MaxFileSize);

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
