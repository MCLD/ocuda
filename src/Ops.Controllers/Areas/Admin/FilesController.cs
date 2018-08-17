using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Files;
using Ocuda.Ops.Controllers.Authorization;
using Ocuda.Ops.Controllers.Filter;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Helpers;
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
        private readonly IThumbnailService _thumbnailService;
        private readonly IUserService _userService;

        private const string FileValidationPassed = "Valid";
        private const string FileValidationFailedNoFile = "No file selected.";
        private const string FileValidationFailedNoThumbnail = "Selected category requires a thumbnail.";
        private const string FileValidationFailedType = "File is not a valid type.";
        private const string FileValidationFailedSize = "File is too large to upload.";

        public const string DefaultCategoryDisplayName = "[No Category]";

        public FilesController(ServiceFacades.Controller<FilesController> context,
            ICategoryService categoryService,
            IFileService fileService,
            IFileTypeService fileTypeService,
            IPageService pageService,
            IPathResolverService pathResolver,
            IPostService postService,
            ISectionService sectionService,
            IThumbnailService thumbnailService,
            IUserService userService) : base(context)
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
            _thumbnailService = thumbnailService
                ?? throw new ArgumentNullException(nameof(thumbnailService));
            _userService = userService
                ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<IActionResult> Index(string section, int? categoryId = null, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BlogFilter(page, itemsPerPage)
            {
                SectionId = currentSection.Id,
                CategoryId = categoryId,
                CategoryType = CategoryType.File
            };

            var fileList = await _fileService.GetPaginatedListAsync(filter);
            var categories = await _categoryService.GetBySectionIdAsync(filter);

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

            foreach (var file in fileList.Data)
            {
                var userInfo = await _userService.GetUserInfoById(file.CreatedBy);
                file.CreatedByName = userInfo.Item1;
                file.CreatedByUsername = userInfo.Item2;
            }

            var viewModel = new IndexViewModel()
            {
                PaginateModel = paginateModel,
                Files = fileList.Data,
                Categories = categories,
            };

            if (categoryId.HasValue)
            {
                var name = (await _categoryService.GetByIdAsync(categoryId.Value)).Name;
                viewModel.CategoryName =
                    string.IsNullOrWhiteSpace(name) ? DefaultCategoryDisplayName : name;
            }

            return View(viewModel);
        }

        [RestoreModelState]
        public async Task<IActionResult> Create(string section, int categoryId)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var currentCategory = await _categoryService.GetCategoryAndFileTypesByCategoryIdAsync(categoryId);

            if (currentCategory != null)
            {
                var fileExtensions = currentCategory.CategoryFileTypes.Select(_ => _.FileType.Extension);
                var acceptedFileExtensions = string.Join(',', fileExtensions);

                var maxThumbnailCount = await _siteSettingService.GetSettingIntAsync(
                    Models.Keys.SiteSetting.FileManagement.MaxThumbnailCount);

                var filter = new BlogFilter()
                {
                    SectionId = currentSection.Id,
                    CategoryType = CategoryType.File
                };

                var viewModel = new DetailViewModel()
                {
                    Action = nameof(Create),
                    SectionId = currentSection.Id,
                    Category = currentCategory,
                    CategoryName = currentCategory.Name,
                    ThumbnailRequired = currentCategory.ThumbnailRequired,
                    MaxThumbnailCount = maxThumbnailCount,
                    AcceptedFileExtensions = acceptedFileExtensions
                };

                return View("Detail", viewModel);
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [SaveModelState]
        public async Task<IActionResult> Create(DetailViewModel model)
        {
            if (model.FileData == null)
            {
                ModelState.AddModelError("File", FileValidationFailedNoFile);
                ShowAlertDanger(FileValidationFailedNoFile);
            }
            else
            {
                var maxFileSize = await _siteSettingService
                    .GetSettingIntAsync(Models.Keys.SiteSetting.FileManagement.MaxUploadBytes);

                if (model.FileData.Length > maxFileSize)
                {
                    ModelState.AddModelError("File", FileValidationFailedSize);
                    ShowAlertDanger(FileValidationFailedSize);
                }
                else
                {
                    var extension = System.IO.Path.GetExtension(model.FileData.FileName);
                    if (string.IsNullOrWhiteSpace(extension))
                    {
                        ModelState.AddModelError("File", FileValidationFailedType);
                        ShowAlertDanger(FileValidationFailedType);
                    }
                    else if (!string.IsNullOrWhiteSpace(model.AcceptedFileExtensions)
                        && !model.AcceptedFileExtensions.Split(',').Any(_ => _ == extension))
                    {
                        ModelState.AddModelError("File", FileValidationFailedType);
                        ShowAlertDanger(FileValidationFailedType);
                    }
                    else
                    {
                        var typeProvider = new FileExtensionContentTypeProvider();
                        typeProvider.TryGetContentType(model.FileData.FileName, out string contentType);

                        if (string.IsNullOrWhiteSpace(contentType))
                        {
                            ModelState.AddModelError("File", FileValidationFailedType);
                            ShowAlertDanger(FileValidationFailedType);
                        }
                    }
                }

                if (model.ThumbnailRequired && model.ThumbnailFiles == null && model.ThumbnailIds == null)
                {
                    ModelState.AddModelError("Thumbnails", FileValidationFailedNoThumbnail);
                    ShowAlertDanger(FileValidationFailedNoThumbnail);
                }

            }

            if (ModelState.IsValid)
            {
                try
                {
                    model.File.SectionId = model.SectionId;
                    model.File.CategoryId = model.Category.Id;

                    var newFile = await _fileService.CreatePrivateFileAsync(CurrentUserId,
                        model.File, model.FileData, model.ThumbnailFiles);

                    ShowAlertSuccess($"Added file: {newFile.Name}");
                    return RedirectToAction(nameof(Index));
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger("Unable to add file: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Create), new { model.Category });
        }

        [RestoreModelState]
        public async Task<IActionResult> Edit(int id)
        {
            var file = await _fileService.GetByIdAsync(id);
            var category = await _categoryService.GetCategoryAndFileTypesByCategoryIdAsync(file.CategoryId.Value);
            var fileExtensions = category.CategoryFileTypes.Select(_ => _.FileType.Extension);
            var acceptedFileExtensions = string.Join(',', fileExtensions);

            var maxThumbnailCount = await _siteSettingService.GetSettingIntAsync(
                Models.Keys.SiteSetting.FileManagement.MaxThumbnailCount);

            if (file.Thumbnails.Count > 0)
            {
                foreach (var thumbnail in file.Thumbnails)
                {
                    thumbnail.Url = _thumbnailService.GetUrl(thumbnail);
                }
            }

            var filter = new BlogFilter()
            {
                SectionId = file.SectionId,
                CategoryType = CategoryType.File
            };

            var viewModel = new DetailViewModel()
            {
                Action = nameof(Edit),
                SectionId = file.SectionId,
                Category = category,
                CategoryName = category.Name,
                File = file,
                MaxThumbnailCount = maxThumbnailCount,
                ThumbnailRequired = category.ThumbnailRequired,
                AcceptedFileExtensions = acceptedFileExtensions
            };

            return View("Detail", viewModel);
        }

        [HttpPost]
        [SaveModelState]
        public async Task<IActionResult> Edit(DetailViewModel model)
        {
            if (model.FileData != null)
            {
                var maxFileSize = await _siteSettingService
                    .GetSettingIntAsync(Models.Keys.SiteSetting.FileManagement.MaxUploadBytes);

                if (model.FileData.Length > maxFileSize)
                {
                    ModelState.AddModelError("File", FileValidationFailedSize);
                    ShowAlertDanger(FileValidationFailedSize);
                }
                else
                {
                    var extension = System.IO.Path.GetExtension(model.FileData.FileName);
                    if (!string.IsNullOrWhiteSpace(model.AcceptedFileExtensions)
                        && !model.AcceptedFileExtensions.Split(',').Any(_ => _ == extension))
                    {
                        ModelState.AddModelError("File", FileValidationFailedType);
                        ShowAlertDanger(FileValidationFailedType);
                    }
                    else
                    {
                        var typeProvider = new FileExtensionContentTypeProvider();
                        typeProvider.TryGetContentType(model.FileData.FileName, out string contentType);

                        if (string.IsNullOrWhiteSpace(contentType))
                        {
                            ModelState.AddModelError("File", FileValidationFailedType);
                            ShowAlertDanger(FileValidationFailedType);
                        }
                    }
                }
            }

            if (model.ThumbnailRequired && model.ThumbnailFiles == null && model.ThumbnailIds == null)
            {
                ModelState.AddModelError("Thumbnails", FileValidationFailedNoThumbnail);
                ShowAlertDanger(FileValidationFailedNoThumbnail);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var file = await _fileService.EditPrivateFileAsync(CurrentUserId,
                        model.File, model.FileData, model.ThumbnailFiles, model.ThumbnailIds ?? new int[] { });
                    ShowAlertSuccess($"Updated file: {file.Name}");
                    return RedirectToAction(nameof(Index));
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger("Unable to update file: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Edit));
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

        #region Categories
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Categories(string section, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);
            var fileTypes = await _fileTypeService.GetAllExtensionsAsync();

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
                SectionId = currentSection.Id,
                FileTypes = fileTypes
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> CreateCategory(
            string name, int sectionId, bool thumbnail, int[] fileTypeIds)
        {
            var category = new Category
            {
                CategoryType = CategoryType.File,
                IsDefault = false,
                Name = name,
                SectionId = sectionId,
                ThumbnailRequired = thumbnail
            };

            try
            {
                var newCategory =
                    await _categoryService.CreateCategoryAsync(CurrentUserId, category, fileTypeIds);
                ShowAlertSuccess($"Added file category: {newCategory.Name}");
                return Json(new { success = true });
            }
            catch (OcudaException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> EditCategory(
            int id, string name, bool thumbnail, int[] fileTypeIds)
        {
            try
            {
                var category =
                    await _categoryService.EditCategoryAsync(CurrentUserId, id, name, thumbnail, fileTypeIds);
                ShowAlertSuccess($"Updated file category: {category.Name}");
                return Json(new { success = true });
            }
            catch (OcudaException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
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
        #endregion

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

        public async Task<IActionResult> UploadAttachment(
            IFormFile fileData, int sectionId, int contentId, string contentType)
        {
            string result = await ValidateFile(fileData.FileName, fileData.Length);

            if (result == FileValidationPassed)
            {
                try
                {
                    if (fileData != null)
                    {
                        var section = await _sectionService.GetByIdAsync(sectionId);

                        BlogFilter filter = new BlogFilter
                        {
                            CategoryType = CategoryType.File,
                            SectionId = section.Id
                        };

                        var attachmentCategory = await _categoryService.GetAttachmentAsync(filter);

                        File file = new File
                        {
                            Name = System.IO.Path.GetFileNameWithoutExtension(fileData.FileName),
                            IsFeatured = false,
                            SectionId = section.Id,
                            CategoryId = attachmentCategory.Id
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

                        var newFile =
                            await _fileService.CreatePublicFileAsync(CurrentUserId, file, fileData);

                        _logger.LogInformation(
                            $"Attached file '{newFile.Name}{newFile.Extension}' to {contentType}Id: {contentId}");

                        result = _pathResolver
                                    .GetPublicContentUrl($"section{sectionId}",
                                                         $"file{newFile.Id}{newFile.Extension}");
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

        [HttpPost]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            try
            {
                await _fileService.DeletePublicFileAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting attachment: {ex.Message}", ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> ValidateFileBeforeUpload(
            string fileName, long fileSize, string fileExtensions = null)
        {
            string result = await ValidateFile(fileName, fileSize, fileExtensions);
            return Json(result);
        }

        private async Task<string> ValidateFile(
            string fileName, long fileSize, string fileExtensions = null)
        {
            var result = "";
            var maxFileSize = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.FileManagement.MaxUploadBytes);

            if (fileSize > maxFileSize)
            {
                result = FileValidationFailedSize;
            }
            else
            {
                var extension = System.IO.Path.GetExtension(fileName);

                if (string.IsNullOrWhiteSpace(extension))
                {
                    result = FileValidationFailedType;
                }
                else if (!string.IsNullOrWhiteSpace(fileExtensions)
                    && !fileExtensions.Split(',').Any(_ => _ == extension))
                {
                    result = FileValidationFailedType;
                }
                else
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
            }

            _logger.LogInformation($"Validation for \"{fileName}\": {result}", fileName, result);

            return result;
        }

        public async Task<IActionResult> ValidateThumbnailBeforeUpload(
            string fileName, long fileSize, int imgHeight, int imgWidth)
        {
            string result = await ValidateThumbnail(fileName, fileSize, imgHeight, imgWidth);
            return Json(result);
        }

        private async Task<string> ValidateThumbnail(
            string fileName, long fileSize, int imgHeight, int imgWidth)
        {
            var result = "";
            var maxHeight = 300;
            var maxWidth = 300;

            if (imgHeight <= maxHeight && imgWidth <= maxWidth)
            {
                var maxFileSize = await _siteSettingService
                    .GetSettingIntAsync(Models.Keys.SiteSetting.FileManagement.MaxUploadBytes);

                if (fileSize < maxFileSize)
                {
                    var extension = System.IO.Path.GetExtension(fileName);
                    var thumbnailTypes = (await _siteSettingService
                                            .GetSettingStringAsync(
                                                Models.Keys.SiteSetting.FileManagement.ThumbnailTypes))
                                            .Split(',').Select(_ => _.Trim());

                    if (!thumbnailTypes.Contains(extension))
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
            }
            else
            {
                result = $"Thumbnail cannot be greater than {maxHeight}x{maxWidth} pixels.";
            }

            _logger.LogInformation($"Validation for \"{fileName}\": {result}", fileName, result);

            return result;
        }

        public async Task<IActionResult> GetCategoryFileTypes(int categoryId)
        {
            var fileTypeIds = await _categoryService.GetFileTypeIdsByCategoryIdAsync(categoryId);
            var fileTypeIdsInUse = await _fileService.GetFileTypeIdsInUseByCategoryIdAsync(categoryId);
            return Json(new { success = true, fileTypeIds, fileTypeIdsInUse });
        }

        public async Task<string> GetPublicUrl(int fileId)
        {
            var file = await _fileService.GetByIdAsync(fileId);
            var sectionId = file.SectionId;
            return _pathResolver.GetPublicContentUrl($"section{sectionId}", $"file{file.Id}{file.Extension}");
        }
    }
}
