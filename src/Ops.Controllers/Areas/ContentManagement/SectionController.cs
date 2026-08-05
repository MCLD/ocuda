using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Controllers.ViewModels.Home;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Filters;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area("ContentManagement")]
    [Route("[area]/[controller]")]
    public class SectionController(
        ServiceFacades.Controller<SectionController> context,
        IFileService fileService,
        ILinkService linkService,
        IOcudaCache cache,
        IPermissionGroupService permissionGroupService,
        IPostService postService,
        ISectionService sectionService)
        : BaseController<SectionController>(context)
    {
        public const long MaximumFileUploadBytes = 50 * 1024 * 1024;

        public static string Area
        {
            get { return "ContentManagement"; }
        }

        public static string Name
        {
            get { return "Section"; }
        }

        private static IDictionary<int, string> FileLibrarySortOptions
        {
            get
            {
                return new Dictionary<int, string>
                {
                    { (int)FileLibrarySort.AlphabeticalName, "Alphabetical by name" },
                    { (int)FileLibrarySort.CreatedDate, "Date created" },
                    { (int)FileLibrarySort.DocumentDateMonthDescending, "Document date" },
                    { (int)FileLibrarySort.ThumbnailsAlphabetical, "Thumbnails, alphabetical by name" },
                };
            }
        }

        private static ICollection<FileType> GetThumbnailTypes
        {
            get
            {
                return [new FileType { Extension = ".jpg" }];
            }
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("[action]/{sectionSlug}")]
        public async Task<IActionResult> DeleteSection(string sectionSlug)
        {
            try
            {
                await sectionService.DeleteSectionAsync(sectionSlug);
                await ClearSectionCache();
            }
            catch (OcudaException oex)
            {
                _logger.LogError(
                    oex,
                    "User {Username} was unable to delete section with slug {sectionSlug}: {ErrorMessage}",
                    CurrentUsername,
                    sectionSlug,
                    oex.Message);
                ShowAlertDanger($"Unable to delete section: {oex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("[action]")]
        [SaveModelState]
        public async Task<IActionResult> AddSection(SectionIndexViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var issues = new StringBuilder("Could not create section:<ul>");
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        issues.Append("<li>").Append(error.ErrorMessage).Append("</li>");
                    }
                }

                issues.Append("</ul>");
                ShowAlertDanger(issues.ToString());
            }
            else
            {
                try
                {
                    await sectionService.CreateSectionAsync(new Section
                    {
                        Icon = viewModel.SectionIcon,
                        Name = viewModel.SectionName,
                        Slug = viewModel.SectionSlug,
                        SupervisorsOnly = viewModel.SupervisorsOnly,
                    });

                    await ClearSectionCache();

                    ShowAlertInfo($"Section {viewModel.SectionName} created and section cache cleared.");
                }
                catch (OcudaException oex)
                {
                    _logger.LogError(oex,
                        "Couldn't create section {SectionName}: {ErrorMessage}",
                        viewModel.SectionName,
                        oex.Message);

                    ShowAlertDanger($"Could not create section: {oex.Message}");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("[action]/{sectionSlug}/{fileLibrarySlug}/{permissionGroupId:int}")]
        public async Task<IActionResult> AddFilePermissionGroup(
            string sectionSlug,
            string fileLibrarySlug,
            int permissionGroupId)
        {
            Section section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to add a file permission group: section {Section} - file library {FileLibrarySlug}",
                    CurrentUsername,
                    sectionSlug,
                    fileLibrarySlug);
                return RedirectToUnauthorized();
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            try
            {
                await permissionGroupService.AddToPermissionGroupAsync<PermissionGroupReplaceFiles>(
                    fileLibrary.Id,
                    permissionGroupId);
                AlertInfo = "Group added for file replacement.";
            }
            catch (OcudaException oex)
            {
                AlertDanger = $"Problem adding permission: {oex.Message}";
            }

            return RedirectToAction(nameof(ReplaceFilePermissions), new
            {
                SectionSlug = sectionSlug,
                FileLibrarySlug = fileLibrarySlug,
            });
        }

        [HttpPost("[action]")]
        [RequestSizeLimit(MaximumFileUploadBytes)]
        public async Task<IActionResult> AddFileToLibrary(FileLibraryViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            var section = !string.IsNullOrEmpty(viewModel.SectionSlug)
                ? await GetSectionAsManagerAsync(viewModel.SectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to add a file to a file library: section {SectionSlug} - file library {FileLibraryId}",
                    CurrentUsername,
                    viewModel.SectionSlug,
                    viewModel.FileLibraryId);
                return RedirectToUnauthorized();
            }

            var fileLibrary = await fileService.GetLibraryByIdAsync(viewModel.FileLibraryId);

            var extension = Path.GetExtension(viewModel.UploadFile.FileName);

            string path;
            try
            {
                path = await fileService.VerifyAddFileAsync(
                    section,
                    fileLibrary.Id,
                    extension,
                    viewModel.File.Name + extension);
                viewModel.File.FileLibraryId = fileLibrary.Id;
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger(oex.Message);

                return RedirectToAction(nameof(Controllers.HomeController.Files),
                    Controllers.HomeController.Name,
                    new
                    {
                        area = string.Empty,
                        fileLibrarySlug = fileLibrary.Slug,
                        page = viewModel.CurrentPage,
                        sectionSlug = section.Slug,
                    });
            }

            bool uploadSuccess = false;

            try
            {
                if (viewModel.UploadFile.Length > 0)
                {
                    await using var fileStream = new FileStream(path, FileMode.Create);
                    await viewModel.UploadFile.CopyToAsync(fileStream);
                    await fileService.AddFileLibraryFileAsync(viewModel.File, viewModel.UploadFile);
                    uploadSuccess = true;
                }
                else
                {
                    ShowAlertDanger($"Empty file {viewModel.File.Name} not uploaded successfully.");
                }
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Error with upload: {oex.Message}");
                _logger.LogError("Unable to handle uploaded file details for library {Id}, file {Filename}: {ErrorMessage}",
                    fileLibrary.Id,
                    viewModel.File.Name + extension,
                    oex.Message);
            }

            if (uploadSuccess)
            {
                ShowAlertSuccess($"Added to {fileLibrary.Name}: {viewModel.File.Name}");
            }
            else
            {
                System.IO.File.Delete(path);
            }

            return RedirectToAction(nameof(Controllers.HomeController.Files),
                Controllers.HomeController.Name,
                new
                {
                    area = string.Empty,
                    fileLibrarySlug = fileLibrary.Slug,
                    page = viewModel.CurrentPage,
                    sectionSlug = section.Slug,
                });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddLinkToLibrary(LinkLibraryViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            var section = !string.IsNullOrEmpty(viewModel.SectionSlug)
                ? await GetSectionAsManagerAsync(viewModel.SectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to add a link to a link library: section {Section} - link library {LinkLibraryId}",
                    CurrentUsername,
                    viewModel.SectionSlug,
                    viewModel.LinkLibraryId);
                return RedirectToUnauthorized();
            }

            if (ModelState.IsValid)
            {
                viewModel.Link.LinkLibraryId = viewModel.LinkLibraryId;
                var link = await linkService.CreateAsync(viewModel.Link);
                var linkLib = await linkService.GetLibraryByIdAsync(viewModel.LinkLibraryId);
                viewModel.LinkLibrary = linkLib;

                ShowAlertSuccess($"Added '{link.Name}' to '{linkLib.Name}'");
                return RedirectToAction(nameof(Controllers.HomeController.Links),
                    Controllers.HomeController.Name,
                    new
                    {
                        area = string.Empty,
                        sectionSlug = viewModel.SectionSlug,
                        linkLibrarySlug = linkLib.Slug,
                    });
            }
            else
            {
                ShowAlertDanger("Could not add Link to Library");
                return RedirectToAction(nameof(Controllers.HomeController.Links),
                    Controllers.HomeController.Name,
                    new
                    {
                        area = string.Empty,
                        sectionSlug = viewModel.SectionSlug,
                        linkLibrarySlug = viewModel.LinkLibrarySlug,
                    });
            }
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("[action]/{slug}/{permissionGroupId}")]
        public async Task<IActionResult> AddPermissionGroup(string slug, int permissionGroupId)
        {
            var section = !string.IsNullOrEmpty(slug)
                ? await GetSectionAsManagerAsync(slug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to add a permission group: section {Section}",
                    CurrentUsername,
                    slug);
                return RedirectToUnauthorized();
            }

            try
            {
                await permissionGroupService
                    .AddToPermissionGroupAsync<PermissionGroupSectionManager>(section.Id,
                permissionGroupId);
                AlertInfo = "Group added for section management.";
            }
            catch (OcudaException oex)
            {
                AlertDanger = $"Problem adding permission: {oex.Message}";
            }

            return RedirectToAction(nameof(Permissions), new { slug });
        }

        [HttpGet("{sectionSlug}/[action]")]
        [RestoreModelState]
        public async Task<IActionResult> AddPost(string sectionSlug, int? page)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to add a post: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                return RedirectToUnauthorized();
            }

            var viewModel = new PostViewModel
            {
                CanPromote = await HasAppPermissionAsync(permissionGroupService,
                    ApplicationPermission.IntranetFrontPageManagement),
                Page = page > 1 ? page : null,
                Post = new Post(),
                SectionSlug = section.Slug,
                SectionId = section.Id,
                SectionName = section.Name,
            };
            return View("Post", viewModel);
        }

        [HttpPost("{sectionSlug}/[action]")]
        [SaveModelState]
        public async Task<IActionResult> AddPost(string sectionSlug, PostViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to add a post: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                return RedirectToUnauthorized();
            }

            Post post = null;

            var slug = viewModel.Post.Slug?.Trim();

            if (await postService.GetSectionPostBySlugAsync(slug, section.Id) != null)
            {
                ModelState.AddModelError(
                    $"{nameof(Post)}.{nameof(Post.Slug)}",
                    "This slug already exists.");
                ShowAlertDanger("Could not create Post: slug already exists.");
                return RedirectToAction(nameof(AddPost), viewModel);
            }

            if (ModelState.IsValid)
            {
                if (viewModel.Publish)
                {
                    var date = viewModel.PublishAtDate ?? DateTime.Now;
                    var time = viewModel.PublishAtTime ?? DateTime.Now;
                    viewModel.Post.PublishedAt = date.CombineWithTime(time);
                }

                if (viewModel.PinUntilDate.HasValue && viewModel.PinUntilTime.HasValue)
                {
                    viewModel.Post.PinnedUntil = viewModel.PinUntilDate.Value
                        .CombineWithTime(viewModel.PinUntilTime.Value);
                }

                try
                {
                    post = await postService.CreatePostAsync(viewModel.Post);

                    if (viewModel.Publish)
                    {
                        ShowAlertSuccess($"Added post: {post.Title}");
                    }
                    else
                    {
                        ShowAlertSuccess($"Created draft: {post.Title}");
                    }
                }
                catch (OcudaException oex)
                {
                    _logger.LogError(oex, "Issue creating post: {ErrorMessage}", oex.Message);
                    ShowAlertDanger($"Could not create post: {oex.Message}");
                    return RedirectToAction(nameof(SectionController.AddPost),
                        new
                        {
                            sectionSlug = section.Slug,
                        });
                }
            }
            else
            {
                ShowAlertDanger($"Error with post: {ModelState.ErrorCount} validation errors.");
                return RedirectToAction(nameof(SectionController.AddPost),
                    new
                    {
                        sectionSlug = section.Slug,
                    });
            }

            return RedirectToAction(
                nameof(Controllers.HomeController.SectionIndex),
                Controllers.HomeController.Name,
                new
                {
                    area = string.Empty,
                    slug = sectionSlug,
                });
        }

        [HttpPost("{sectionSlug}/[action]/{fileLibrarySlug}/{fileId:int}")]
        public async Task<IActionResult> DeleteThumbnail(
            int fileId,
            string fileLibrarySlug,
            string sectionSlug,
            ManageThumbnailsViewModel viewModel)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to delete a thumbnail: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                return RedirectToUnauthorized();
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            var path = await fileService
                .GetThumbnailPathAsync(viewModel.DeleteThumbnailId, fileLibrarySlug, sectionSlug);

            bool fileExists = System.IO.File.Exists(path);
            bool fileDeleteFailure = false;
            bool fileDatabaseFailure = false;

            if (fileExists)
            {
                try
                {
                    System.IO.File.Delete(path);
                }
                catch (SystemException ex)
                {
                    fileDeleteFailure = true;
                    _logger.LogWarning(
                        "Unable to delete thumbnail file {Path}: {ErrorMessage}",
                        path,
                        ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("Asked to delete file but it's already gone: {Path}", path);
            }

            if (!fileDeleteFailure)
            {
                try
                {
                    await fileService.DeleteThumbnailAsync(viewModel.DeleteThumbnailId);
                }
                catch (OcudaException oex)
                {
                    fileDatabaseFailure = true;
                    _logger.LogWarning(
                        "Couldn't delete thumbnail from database: {ErrorMessage}",
                        oex.Message);
                }
            }

            string message = $"For thumbnail id {viewModel.DeleteThumbnailId}:"
                + (fileExists
                    ? fileDeleteFailure
                        ? " file could not be deleted"
                        : " file was deleted"
                    : " file could not be found")
                + (fileDatabaseFailure
                    ? " and thumbnail could not be removed from the database."
                    : " and thumbnail was removed from the database.");

            if (!fileExists || fileDeleteFailure || fileDatabaseFailure)
            {
                ShowAlertWarning(message);
            }
            else
            {
                ShowAlertSuccess(message);
            }

            return RedirectToAction(nameof(ManageThumbnails),
                new
                {
                    fileId,
                    fileLibrarySlug,
                    sectionSlug,
                });
        }

        [HttpPost("{sectionSlug}/[action]/{fileLibrarySlug}/{fileId:int}")]
        public async Task<IActionResult> AddThumbnail(
            string fileLibrarySlug,
            int fileId,
            string sectionSlug,
            ManageThumbnailsViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to add a thumbnail: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                return RedirectToUnauthorized();
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            var extension = Path.GetExtension(viewModel.UploadFile.FileName);

            if (!GetThumbnailTypes.Select(_ => _.Extension).Contains(extension))
            {
                ShowAlertWarning($"Invalid file extension, acceptable thumbnail extensions: {string.Join(", ", GetThumbnailTypes.Select(_ => _.Extension))}");
            }
            else if (!(viewModel.UploadFile?.Length > 0))
            {
                ShowAlertDanger($"Empty file {viewModel.UploadFile?.Name} not uploaded successfully.");
            }
            else
            {
                string path = null;
                try
                {
                    path = fileService.GetUnusedThumbnailPath(
                        viewModel.UploadFile.FileName,
                        fileLibrary.Slug,
                        section.Slug);

                    await using var fileStream = new FileStream(path, FileMode.Create);
                    await viewModel.UploadFile.CopyToAsync(fileStream);
                    await fileService.AddThumbnailAsync(fileId, Path.GetFileName(path));
                    ShowAlertSuccess("Thumbnail uploaded!");
                }
                catch (OcudaException oex)
                {
                    ShowAlertWarning($"Unable to handle thumbnail upload: {oex.Message}");
                    _logger.LogError(
                        "Unable to handle uploaded thumbnail for file {FileId}: {ErrorMessage}",
                        fileId,
                        oex.Message);

                    if (!string.IsNullOrEmpty(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
            }

            return RedirectToAction(nameof(ManageThumbnails),
                new
                {
                    fileId,
                    fileLibrarySlug,
                    sectionSlug,
                });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("{sectionSlug}/[action]/{fileLibrarySlug}")]
        public async Task<IActionResult> AddTypeToLibrary(
            string fileLibrarySlug,
            int fileTypeId,
            string sectionSlug)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to add file type to a file library: section {SectionSlug} file library {FileLibrarySlug}",
                    CurrentUsername,
                    sectionSlug,
                    fileLibrarySlug);
                return RedirectToUnauthorized();
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            var type = await fileService.GetFileTypeByIdAsync(fileTypeId);

            var libraryTypes = await fileService.GetFileLibrariesFileTypesAsync(fileLibrary.Id);

            var libraryTypeIds = libraryTypes.Select(_ => _.Id).Union([fileTypeId]).ToArray();

            try
            {
                await fileService.EditLibraryTypesAsync(fileLibrary, libraryTypeIds);
                ShowAlertSuccess($"Added ability to upload files of type: {type.Extension}");
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to add file types to library: {oex.Message}");
            }

            return RedirectToAction(
                nameof(SectionController.FileLibrary),
                new
                {
                    fileLibrarySlug,
                    sectionSlug,
                });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpGet("{sectionSlug}/[action]/{fileLibrarySlug}")]
        public async Task<IActionResult> AvailableFileTypes(
            string fileLibrarySlug,
            string sectionSlug)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to view available file types: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                return RedirectToUnauthorized();
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            var types = await fileService.GetAllFileTypesAsync();
            var libraryTypes = await fileService.GetFileLibrariesFileTypesAsync(fileLibrary.Id);
            var availableTypes = types.Where(_ => !libraryTypes.Select(__ => __.Id).Contains(_.Id));
            return Json(new PortableList<FileType>
            {
                Items = availableTypes.Select(_ => new FileType
                {
                    Extension = _.Extension,
                    Icon = _.Icon,
                    Id = _.Id,
                }),
            });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("[action]")]
        public async Task<IActionResult> ClearSectionCache()
        {
            if (string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)))
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to clear the sections from cache",
                    CurrentUsername);
                return RedirectToUnauthorized();
            }

            await cache.RemoveAsync(Cache.OpsSections);
            ShowAlertInfo("Section cache cleared.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{sectionSlug}/[action]/{fileLibrarySlug}")]
        public async Task<IActionResult> EditFileLibraryFile(
            string fileLibrarySlug,
            string sectionSlug,
            FileLibraryViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to edit a file library: section {SectionSlug} file library {FileLibrarySlug}",
                    CurrentUsername,
                    sectionSlug,
                    fileLibrarySlug);
                return RedirectToUnauthorized();
            }

            var file = await fileService.GetFileLibraryFileAsync(viewModel.File.Id);

            if (file == null)
            {
                return NotFound();
            }

            try
            {
                await fileService.EditFileLibraryFileAsync(
                    sectionSlug,
                    fileLibrarySlug,
                    viewModel.File);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger(oex.Message);
            }

            return RedirectToAction(nameof(Controllers.HomeController.Files),
                Controllers.HomeController.Name,
                new
                {
                    area = string.Empty,
                    fileLibrarySlug,
                    page = viewModel.CurrentPage,
                    sectionSlug,
                });
        }

        [HttpPost("{sectionSlug}/[action]/{fileLibrarySlug}")]
        public async Task<IActionResult> DeleteFileFromLibrary(
            string fileLibrarySlug,
            string sectionSlug,
            FileLibraryViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to delete a file from a file library: section {SectionSlug} file library {FileLibrarySlug}",
                    CurrentUsername,
                    sectionSlug,
                    fileLibrarySlug);
                return RedirectToUnauthorized();
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            try
            {
                await fileService.DeleteFileLibraryFileAsync(
                    section.Slug,
                    fileLibrary.Slug,
                    viewModel.File);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Error deleting file: {oex.Message}");
            }

            return RedirectToAction(nameof(Controllers.HomeController.Files),
                Controllers.HomeController.Name,
                new
                {
                    area = string.Empty,
                    fileLibrarySlug,
                    page = viewModel.CurrentPage,
                    sectionSlug,
                });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("{sectionSlug}/[action]/{fileLibrarySlug}")]
        public async Task<IActionResult> DeleteFileLibrary(
            string fileLibrarySlug,
            string sectionSlug)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to delete a file library: section {SectionSlug} file library {FileLibrarySlug}",
                    CurrentUsername,
                    sectionSlug,
                    fileLibrarySlug);
                return RedirectToUnauthorized();
            }

            try
            {
                var fileLibrary = await fileService
                    .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

                var files = await fileService.GetPaginatedListAsync(
                    section,
                    new FilesFilter
                    {
                        FileLibrary = fileLibrary,
                        OnlyCount = true,
                    });

                if (files.Count > 0)
                {
                    ShowAlertDanger(
                        $"Please delete all files before deleting file library: {fileLibrary.Name}.");
                    return RedirectToAction(nameof(SectionController.FileLibrary),
                        new
                        {
                            fileLibrarySlug,
                            sectionSlug,
                        });
                }

                await fileService.DeleteFileLibraryAsync(section, fileLibrary.Id);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger(oex.Message);
            }

            return RedirectToAction(nameof(SectionController.Section),
                new
                {
                    sectionSlug = section.Slug,
                });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteLinkFromLibrary(LinkLibraryViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var section = !string.IsNullOrEmpty(model.SectionSlug)
                ? await GetSectionAsManagerAsync(model.SectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried delete a link from a link library: section {SectionSlug}",
                    CurrentUsername,
                    model?.SectionSlug);
                return RedirectToUnauthorized();
            }

            if (model.Link.Id != 0)
            {
                var link = await linkService.GetByIdAsync(model.Link.Id);
                try
                {
                    await linkService.DeleteAsync(model.Link.Id);
                    ShowAlertSuccess($"Deleted link '{link.Name}'");
                }
                catch (OcudaException oex)
                {
                    _logger.LogError(oex, "Unable to delete link: {ErrorMessage}", oex.Message);
                    ShowAlertDanger($"Failed to Delete '{link.Name}'");
                }

                return RedirectToAction(nameof(Controllers.HomeController.Links),
                    Controllers.HomeController.Name,
                    new
                    {
                        area = string.Empty,
                        sectionSlug = model.SectionSlug,
                        linkLibrarySlug = model.LinkLibrarySlug,
                    });
            }
            else
            {
                ShowAlertDanger("Failed to Delete Link");
                return RedirectToAction(nameof(Controllers.HomeController.Links),
                    Controllers.HomeController.Name,
                    new
                    {
                        area = string.Empty,
                        sectionSlug = model.SectionSlug,
                        linkLibrarySlug = model.LinkLibrarySlug,
                    });
            }
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("{sectionSlug}/[action]/{linkLibrarySlug}")]
        public async Task<IActionResult> DeleteLinkLibrary(
            string linkLibrarySlug,
            string sectionSlug)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to delete a link library: section {SectionSlug} link library {LinkLibrarySlug}",
                    CurrentUsername,
                    sectionSlug,
                    linkLibrarySlug);
                return RedirectToUnauthorized();
            }

            try
            {
                var linkLibrary = await linkService
                    .GetBySectionIdSlugAsync(section.Id, linkLibrarySlug);

                var links = await linkService.GetPaginatedListAsync(new LinksFilter
                {
                    LinkLibrary = linkLibrary,
                    OnlyCount = true,
                });

                if (links.Count > 0)
                {
                    ShowAlertDanger(
                        $"Please delete all links before deleting file library: {linkLibrary.Name}.");
                    return RedirectToAction(nameof(SectionController.FileLibrary),
                        new
                        {
                            linkLibrarySlug,
                            sectionSlug,
                        });
                }

                await linkService.DeleteLibraryAsync(linkLibrary.Id);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger(oex.Message);
            }

            return RedirectToAction(nameof(SectionController.Section),
                new
                {
                    sectionSlug = section.Slug,
                });
        }

        [HttpPost("{sectionSlug}/[action]/{postId}")]
        public async Task<IActionResult> DeletePost(int postId, string sectionSlug, int? page)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to delete a post: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                return RedirectToUnauthorized();
            }

            var post = await postService.GetPostByIdAsync(postId);
            try
            {
                await postService.RemovePostAsync(post);
                ShowAlertSuccess($"Deleted post: {post.Title}");
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex, "Unable to delete post: {ErrorMessage}", oex.Message);
                ShowAlertDanger($"Failed to delete post: {post.Title}");
            }

            return RedirectToAction(
                nameof(Controllers.HomeController.SectionIndex),
                Controllers.HomeController.Name,
                new
                {
                    area = string.Empty,
                    page = page > 1 ? page : null,
                    slug = sectionSlug,
                });
        }

        [HttpGet("{sectionSlug}/[action]/{postSlug}")]
        [RestoreModelState]
        public async Task<IActionResult> EditPost(string sectionSlug, string postSlug, int? page)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to edit a post: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                return RedirectToUnauthorized();
            }

            try
            {
                var post = await postService.GetSectionPostBySlugAsync(postSlug, section.Id);
                return View("Post", new PostViewModel
                {
                    CanPromote = await HasAppPermissionAsync(permissionGroupService,
                        ApplicationPermission.IntranetFrontPageManagement),
                    Page = page > 1 ? page : null,
                    PinUntilDate = post.PinnedUntil,
                    PinUntilTime = post.PinnedUntil,
                    Post = post,
                    SectionId = section.Id,
                    SectionName = section.Name,
                    SectionSlug = section.Slug,
                });
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex, "Unable to edit post: {ErrorMessage}", oex.Message);
                ShowAlertDanger($"The PostDetails Id {postSlug} does not exist.");
                return RedirectToAction(nameof(SectionController.Section), new { sectionSlug });
            }
        }

        [HttpPost("{sectionSlug}/[action]/{postSlug}")]
        [SaveModelState]
        public async Task<IActionResult> EditPost(
            string sectionSlug,
            PostViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to edit a post: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                return RedirectToUnauthorized();
            }

            if (ModelState.IsValid)
            {
                if (viewModel.Publish)
                {
                    var date = viewModel.PublishAtDate ?? DateTime.Now;
                    var time = viewModel.PublishAtTime ?? DateTime.Now;
                    viewModel.Post.PublishedAt = date.CombineWithTime(time);
                }

                viewModel.Post.PinnedUntil = viewModel.PinUntilDate.HasValue
                    && viewModel.PinUntilTime.HasValue
                        ? viewModel.PinUntilDate.Value
                            .CombineWithTime(viewModel.PinUntilTime.Value)
                        : null;

                try
                {
                    await postService.UpdatePostAsync(viewModel.Post);
                    ShowAlertSuccess($"Updated post: {viewModel.Post.Title}");
                }
                catch (OcudaException oex)
                {
                    _logger.LogError(oex, "Error editing post: {ErrorMessage}", oex.Message);
                    ShowAlertDanger($"Could not edit post: {oex.Message}");
                }
            }
            else
            {
                ShowAlertDanger($"Could not edit post: {ModelState.ErrorCount} validation errors.");
            }

            return RedirectToAction(nameof(Controllers.HomeController.SectionIndex),
                Controllers.HomeController.Name,
                new
                {
                    area = string.Empty,
                    slug = section.Slug,
                });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpGet("{sectionSlug}/[action]")]
        [RestoreModelState]
        public async Task<IActionResult> FileLibrary(string sectionSlug)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;

            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to access file library admin: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                RedirectToUnauthorized();
            }

            return View(new FileLibraryManagementViewModel
            {
                IsNew = true,
                SectionName = section.Name,
                SectionSlug = sectionSlug,
                SortOrderOptions = FileLibrarySortOptions.Select(_ => new SelectListItem
                {
                    Value = _.Key.ToString(CultureInfo.InvariantCulture),
                    Text = _.Value,
                }),
            });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpGet("{sectionSlug}/[action]/{fileLibrarySlug}")]
        [RestoreModelState]
        public async Task<IActionResult> FileLibrary(string sectionSlug, string fileLibrarySlug)
        {
            Section section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to access file library admin: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                return RedirectToUnauthorized();
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            var fileTypeLink = Url.Action(nameof(AvailableFileTypes), new
            {
                sectionSlug,
                fileLibrarySlug,
            });

            return fileLibrary == null
                ? NotFound()
                : View(new FileLibraryManagementViewModel
                {
                    AssignedFileTypes = await fileService
                        .GetFileLibrariesFileTypesAsync(fileLibrary.Id),
                    FileLibrarySlug = fileLibrarySlug,
                    FileCount = await fileService.GetFileCountAsync(fileLibrary.Id),
                    GetAvailableTypesLink = fileTypeLink,
                    IsFeatured = fileLibrary.IsFeatured,
                    Name = fileLibrary.Name,
                    SectionName = section.Name,
                    SectionSlug = sectionSlug,
                    Slug = fileLibrary.Slug,
                    SortOrder = fileLibrary.SortOrder,
                    SortOrderOptions = FileLibrarySortOptions.Select(_ => new SelectListItem
                    {
                        Value = _.Key.ToString(CultureInfo.InvariantCulture),
                        Text = _.Value,
                    }),
                });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpGet("[action]/{sectionSlug}/{fileLibrarySlug}")]
        public async Task<IActionResult> ReplaceFilePermissions(
            string sectionSlug,
            string fileLibrarySlug)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried replace file permissions: section {SectionSlug} file library {FileLibrarySlug}",
                    CurrentUsername,
                    sectionSlug,
                    fileLibrarySlug);
                return RedirectToUnauthorized();
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            var permissionGroups = await permissionGroupService.GetAllAsync();
            var fileLibraryPermissions = await permissionGroupService
                .GetPermissionsAsync<PermissionGroupReplaceFiles>(fileLibrary.Id);

            var viewModel = new FileLibraryPermissionsViewModel
            {
                Name = fileLibrary.Name,
                SectionName = section.Name,
                SectionSlug = section.Slug,
                FileLibrarySlug = fileLibrary.Slug,
            };

            foreach (var permissionGroup in permissionGroups)
            {
                if (fileLibraryPermissions.Any(_ => _.PermissionGroupId == permissionGroup.Id))
                {
                    viewModel.AssignedGroups.Add(permissionGroup.Id,
                        permissionGroup.PermissionGroupName);
                }
                else
                {
                    viewModel.AvailableGroups.Add(permissionGroup.Id,
                        permissionGroup.PermissionGroupName);
                }
            }

            return View(viewModel);
        }

        [HttpGet("")]
        [RestoreModelState]
        public async Task<IActionResult> Index()
        {
            var permissionGroupIds = UserClaims(ClaimType.PermissionId)
                .Select(_ => int.Parse(_, CultureInfo.InvariantCulture));

            var sections = await sectionService.GetManagedByCurrentUserAsync();

            foreach (var section in sections)
            {
                var posts = await postService.GetPaginatedPostsAsync(new BlogFilter
                {
                    SectionId = section.Id,
                    Skip = 0,
                    Take = 1,
                });
                var links = await linkService.GetBySectionIdAsync(section.Id);
                var fileLibraries = await fileService.GetBySectionIdAsync(section.Id);

                section.PostCount = posts.Count;
                section.LinkLibraryCount = links.Count;
                section.FileLibraryCount = fileLibraries.Count;
            }

            return View(new SectionIndexViewModel
            {
                Sections = sections,
            });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpGet("{sectionSlug}/[action]")]
        [RestoreModelState]
        public async Task<IActionResult> LinkLibrary(string sectionSlug)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;

            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to admin link libraries: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                RedirectToUnauthorized();
            }

            return View(new LinkLibraryManagementViewModel
            {
                IsNew = true,
                SectionName = section.Name,
                SectionSlug = sectionSlug,
            });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpGet("{sectionSlug}/[action]/{linkLibrarySlug}")]
        [RestoreModelState]
        public async Task<IActionResult> LinkLibrary(string sectionSlug, string linkLibrarySlug)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to admin link libraries: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                return RedirectToUnauthorized();
            }

            var linkLibrary = await linkService
                .GetBySectionIdSlugAsync(section.Id, linkLibrarySlug);

            return linkLibrary == null
                ? NotFound()
                : View(new LinkLibraryManagementViewModel
                {
                    LinkLibrarySlug = linkLibrarySlug,
                    LinkCount = await linkService.GetLinkCountAsync(linkLibrary.Id),
                    IsFeatured = linkLibrary.IsFeatured,
                    Name = linkLibrary.Name,
                    SectionName = section.Name,
                    SectionSlug = sectionSlug,
                    Slug = linkLibrary.Slug,
                });
        }

        [HttpGet("{sectionSlug}/[action]/{fileLibrarySlug}/{fileId:int}")]
        public async Task<IActionResult> ManageThumbnails(
            string fileLibrarySlug,
            int fileId,
            string sectionSlug)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to manage thumbnails: section {SectionSlug} file library {FileLibrarySlug}",
                    CurrentUsername,
                    sectionSlug,
                    fileLibrarySlug);
                return RedirectToUnauthorized();
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            var file = await fileService.GetByIdAsync(fileId);

            var thumbnails = PopulateThumbnailLinks(
                fileLibrarySlug,
                sectionSlug,
                await fileService.GetThumbnailsAsync([fileId]));

            return View(new ManageThumbnailsViewModel
            {
                FileId = fileId,
                FileLibraryName = fileLibrary.Name,
                FileLibrarySlug = fileLibrary.Slug,
                FileName = file.FullName,
                FileTypes = GetThumbnailTypes,
                SectionName = section.Name,
                SectionSlug = section.Slug,
                Thumbnails = thumbnails,
            });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpGet("[action]/{slug}")]
        public async Task<IActionResult> Permissions(string slug)
        {
            var section = !string.IsNullOrEmpty(slug)
                ? await GetSectionAsManagerAsync(slug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to access permissions: section {SectionSlug}",
                    CurrentUsername,
                    slug);
                return RedirectToUnauthorized();
            }

            var permissionGroups = await permissionGroupService.GetAllAsync();
            var sectionPermissions = await permissionGroupService
                .GetPermissionsAsync<PermissionGroupSectionManager>(section.Id);

            var viewModel = new PermissionsViewModel
            {
                Name = section.Name,
                Slug = section.Slug,
            };

            foreach (var permissionGroup in permissionGroups)
            {
                if (sectionPermissions.Any(_ => _.PermissionGroupId == permissionGroup.Id))
                {
                    viewModel.AssignedGroups.Add(permissionGroup.Id,
                        permissionGroup.PermissionGroupName);
                }
                else
                {
                    viewModel.AvailableGroups.Add(permissionGroup.Id,
                        permissionGroup.PermissionGroupName);
                }
            }

            return View(viewModel);
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("[action]/{sectionSlug}/{fileLibrarySlug}/{permissionGroupId:int}")]
        public async Task<IActionResult> RemoveFilePermissionGroup(
            string sectionSlug,
            string fileLibrarySlug,
            int permissionGroupId)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to remove permission file group: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                return RedirectToUnauthorized();
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            try
            {
                await permissionGroupService
                    .RemoveFromPermissionGroupAsync<PermissionGroupReplaceFiles>(fileLibrary.Id,
                permissionGroupId);
                AlertInfo = "Group removed for file replacement.";
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex, "Unable to remove permission: {ErrorMessage}", oex.Message);
                AlertDanger = $"Problem adding permission: {oex.Message}";
            }

            return RedirectToAction(nameof(ReplaceFilePermissions), new
            {
                SectionSlug = sectionSlug,
                FileLibrarySlug = fileLibrarySlug,
            });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("{sectionSlug}/[action]/{fileLibrarySlug}/{fileTypeId:int}")]
        public async Task<IActionResult> RemoveLibraryFileType(
            string fileLibrarySlug,
            int fileTypeId,
            string sectionSlug)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to remove a file library file type: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                return RedirectToUnauthorized();
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            var fileTypes = await fileService.GetFileLibrariesFileTypesAsync(fileLibrary.Id);

            var fileTypeIds = fileTypes.Select(_ => _.Id).ToList();

            if (fileTypeIds.Remove(fileTypeId))
            {
                await fileService.EditLibraryTypesAsync(fileLibrary, fileTypeIds);

                var typeDetail = await fileService.GetFileTypeByIdAsync(fileTypeId);

                ShowAlertSuccess(
                    $"Removed ability to upload files of type: {typeDetail.Extension}");
            }
            else
            {
                ShowAlertWarning("File type was not associated with that file library.");
            }

            return RedirectToAction(nameof(FileLibrary), new
            {
                fileLibrarySlug,
                sectionSlug,
            });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("[action]/{slug}/{permissionGroupId:int}")]
        public async Task<IActionResult> RemovePermissionGroup(string slug, int permissionGroupId)
        {
            var section = !string.IsNullOrEmpty(slug)
                ? await GetSectionAsManagerAsync(slug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to remove a permission group: section {SectionSlug}",
                    CurrentUsername,
                    slug);
                return RedirectToUnauthorized();
            }

            try
            {
                await permissionGroupService
                    .RemoveFromPermissionGroupAsync<PermissionGroupSectionManager>(section.Id,
                    permissionGroupId);
                AlertInfo = "Group removed from section management.";
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex, "Unable to remove permission: {ErrorMessage}", oex.Message);
                AlertDanger = $"Problem removing permission: {oex.Message}";
            }

            return RedirectToAction(nameof(Permissions), new { slug });
        }

        [HttpPost("{sectionSlug}/[action]/{fileLibrarySlug}")]
        public async Task<IActionResult> ReplaceFile(
            string fileLibrarySlug,
            string sectionSlug,
            FileLibraryViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            var section = await sectionService.GetBySlugAsync(sectionSlug);

            if (section == null)
            {
                return NotFound();
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            var hasReplaceRights = await HasPermissionAsync<PermissionGroupSectionManager>(
                permissionGroupService,
                section.Id)
                || await fileService.HasReplaceRightsAsync(fileLibrary.Id);

            if (!hasReplaceRights)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to replace a file: section {SectionSlug} - file library {FileLibrarySlug}",
                    CurrentUsername,
                    sectionSlug,
                    fileLibrarySlug);
                return RedirectToUnauthorized();
            }

            var file = await fileService.GetByIdAsync(viewModel.ReplaceFileId);
            var fileType = await fileService.GetFileTypeByIdAsync(file.FileTypeId);
            var extension = Path.GetExtension(viewModel.UploadFile.FileName);

            if (!fileType.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                ShowAlertWarning(
                    $"Could not replace file: uploaded file type ({extension}) did not match existing file type ({fileType.Extension}).");
            }
            else
            {
                var path = fileService.GetFileLibraryFilePath(sectionSlug, fileLibrarySlug, file);

                if (viewModel.UploadFile.Length > 0)
                {
                    await using var fileStream = new FileStream(path, FileMode.Truncate);
                    await viewModel.UploadFile.CopyToAsync(fileStream);
                    await fileService.ReplaceFileLibraryFileAsync(file.Id);
                    ShowAlertSuccess($"Replaced: {file.Name}");
                }
                else
                {
                    ShowAlertDanger($"Empty file {viewModel.File.Name} not uploaded successfully.");
                }
            }

            return RedirectToAction(nameof(Controllers.HomeController.Files),
                Controllers.HomeController.Name,
                new
                {
                    area = string.Empty,
                    fileLibrarySlug,
                    page = viewModel.CurrentPage,
                    sectionSlug,
                });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("{sectionSlug}/[action]")]
        [HttpPost("{sectionSlug}/[action]/{fileLibrarySlug}")]
        [SaveModelState]
        public async Task<IActionResult> SaveFileLibrary(
            FileLibraryManagementViewModel viewModel,
            string fileLibrarySlug,
            string sectionSlug)
        {
            if (viewModel == null)
            {
                return BadRequest();
            }

            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to save changes to a file library: section {SectionSlug} - file library {FileLibrarySlug}.",
                    CurrentUsername,
                    sectionSlug,
                    fileLibrarySlug);
                return RedirectToUnauthorized();
            }

            if (!string.IsNullOrEmpty(fileLibrarySlug))
            {
                var fileLibrary = await fileService
                    .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);
                if (fileLibrary == null)
                {
                    return NotFound();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(fileLibrarySlug))
                    {
                        await fileService.CreateLibraryAsync(
                            section,
                            new FileLibrary
                            {
                                IsFeatured = viewModel.IsFeatured,
                                Name = viewModel.Name,
                                SectionId = section.Id,
                                Slug = viewModel.Slug,
                                SortOrder = viewModel.SortOrder,
                            });
                        ShowAlertSuccess($"Created file library: {viewModel.Name.Trim()}");
                    }
                    else
                    {
                        await fileService.UpdateLibrary(
                            section,
                            fileLibrarySlug,
                            new FileLibrary
                            {
                                IsFeatured = viewModel.IsFeatured,
                                Name = viewModel.Name,
                                Slug = viewModel.Slug,
                                SortOrder = viewModel.SortOrder,
                            });
                        ShowAlertSuccess($"Updated file library: {viewModel.Name.Trim()}");
                    }
                }
                catch (OcudaException oex)
                {
                    ShowAlertDanger($"Error updating file library: {oex.Message}");
                }
            }

            return RedirectToAction(
                nameof(SectionController.FileLibrary),
                new
                {
                    fileLibrarySlug = viewModel.Slug.Trim(),
                    sectionSlug = section.Slug,
                });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("{sectionSlug}/[action]")]
        [HttpPost("{sectionSlug}/[action]/{linkLibrarySlug}")]
        [SaveModelState]
        public async Task<IActionResult> SaveLinkLibrary(
            LinkLibraryManagementViewModel viewModel,
            string linkLibrarySlug,
            string sectionSlug)
        {
            if (viewModel == null)
            {
                return BadRequest();
            }

            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to save changes to a link library: section {SectionSlug} - link library {LinkLibrarySlug}",
                    CurrentUsername,
                    sectionSlug,
                    linkLibrarySlug);
                return RedirectToUnauthorized();
            }

            if (!string.IsNullOrEmpty(linkLibrarySlug))
            {
                var linkLibrary = await linkService
                    .GetBySectionIdSlugAsync(section.Id, linkLibrarySlug);
                if (linkLibrary == null)
                {
                    return NotFound();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(linkLibrarySlug))
                    {
                        await linkService.CreateLibraryAsync(new LinkLibrary
                        {
                            IsFeatured = viewModel.IsFeatured,
                            Name = viewModel.Name,
                            SectionId = section.Id,
                            Slug = viewModel.Slug,
                        });
                        ShowAlertSuccess($"Created link library: {viewModel.Name.Trim()}");
                    }
                    else
                    {
                        await linkService.UpdateLibrary(
                            section,
                            linkLibrarySlug,
                            new LinkLibrary
                            {
                                IsFeatured = viewModel.IsFeatured,
                                Name = viewModel.Name,
                                Slug = viewModel.Slug,
                            });
                        ShowAlertSuccess($"Updated link library: {viewModel.Name.Trim()}");
                    }
                }
                catch (OcudaException oex)
                {
                    ShowAlertDanger($"Error updating link library: {oex.Message}");
                }
            }

            return RedirectToAction(
                nameof(LinkLibrary),
                new
                {
                    linkLibrarySlug = viewModel.Slug.Trim(),
                    sectionSlug = section.Slug,
                });
        }

        [HttpGet("{sectionSlug}")]
        public async Task<IActionResult> Section(string sectionSlug)
        {
            var section = !string.IsNullOrEmpty(sectionSlug)
                ? await GetSectionAsManagerAsync(sectionSlug)
                : null;
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to view section management: section {SectionSlug}",
                    CurrentUsername,
                    sectionSlug);
                return RedirectToUnauthorized();
            }

            var posts = await postService.GetPaginatedPostsAsync(new BlogFilter(1, 5)
            {
                SectionId = section.Id,
            });

            var viewModel = new SectionViewModel
            {
                PostCount = posts.Count,
                Section = section,
            };

            viewModel.Posts.AddRange(posts.Data);

            viewModel.FileLibraries.AddRange(await fileService.GetBySectionIdAsync(section.Id));
            foreach (var fileLibrary in viewModel.FileLibraries)
            {
                var files = await fileService.GetPaginatedListAsync(
                    section,
                    new FilesFilter(1, 1)
                    {
                        FileLibrary = fileLibrary,
                    });
                fileLibrary.TotalFilesInLibrary = files.Count;
            }

            viewModel.LinkLibraries.AddRange(await linkService.GetBySectionIdAsync(section.Id));
            foreach (var linkLibrary in viewModel.LinkLibraries)
            {
                var links = await linkService.GetPaginatedListAsync(new LinksFilter(1, 1)
                {
                    LinkLibrary = linkLibrary,
                });
                linkLibrary.TotalLinksInLibrary = links.Count;
            }

            viewModel.CanBeDeleted = viewModel.PostCount == 0
                && !(viewModel.FileLibraries?.Count > 1)
                && !(viewModel.LinkLibraries?.Count > 1);

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateLinkFromLibrary(LinkLibraryViewModel model)
        {
            var section = await GetSectionAsManagerAsync(model?.SectionSlug);
            if (section == null)
            {
                _logger.LogWarning(
                    "Access denied: user {Username} tried to update a link library link: section {SectionSlug}",
                    CurrentUsername,
                    model?.SectionSlug);
                return RedirectToUnauthorized();
            }

            if (ModelState.IsValid)
            {
                var link = await linkService.GetByIdAsync(model.Link.Id);

                if (!IsSiteManager())
                {
                    model.Link.Icon = link.Icon;
                }

                try
                {
                    var updateLink = await linkService.EditAsync(model.Link);
                    ShowAlertSuccess($"Updated link '{updateLink.Name}'");
                }
                catch (OcudaException oex)
                {
                    _logger.LogError(oex, "Unable to update link: {ErrorMessage}", oex.Message);
                    ShowAlertDanger($"Failed to Update '{link.Name}'");
                }

                return RedirectToAction(nameof(Controllers.HomeController.Links),
                    Controllers.HomeController.Name,
                    new
                    {
                        area = string.Empty,
                        sectionSlug = model.SectionSlug,
                        linkLibrarySlug = model.LinkLibrarySlug,
                    });
            }
            else
            {
                ShowAlertDanger("Could not Update Link");
                return RedirectToAction(nameof(Controllers.HomeController.Links),
                    Controllers.HomeController.Name,
                    new
                    {
                        area = string.Empty,
                        sectionSlug = model.SectionSlug,
                        linkLibrarySlug = model.LinkLibrarySlug,
                    });
            }
        }

        private async Task<Section> GetSectionAsManagerAsync(string sectionSlug)
        {
            if (string.IsNullOrEmpty(sectionSlug))
            {
                return null;
            }

            var section = await sectionService.GetBySlugAsync(sectionSlug);
            return await HasPermissionAsync<PermissionGroupSectionManager>(permissionGroupService,
                section.Id)
                ? section
                : null;
        }
    }
}
