using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement;
using Ocuda.Ops.Controllers.ViewModels.Home;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers
{
    [Route("")]
    public class HomeController(
        ServiceFacades.Controller<HomeController> context,
        IFileService fileService,
        ILinkService linkService,
        IPermissionGroupService permissionGroupService,
        IPostService postService,
        ISectionService sectionService,
        IUserService userService)
        : BaseController<HomeController>(context)
    {
        public static string Name
        {
            get { return "Home"; }
        }

        [HttpGet("{sectionSlug}/[action]/{fileLibrarySlug}")]
        public async Task<IActionResult> GetFileDetailsJson(string sectionSlug,
            string fileLibrarySlug,
            int fileId)
        {
            Section section;
            try
            {
                section = await GetSection(sectionSlug);
            }
            catch (OcudaException oex)
            {
                return oex.Data[OcudaExceptionData.HttpResult] is IActionResult
                    ? oex.Data[OcudaExceptionData.HttpResult] as IActionResult
                    : NotFound();
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            if (fileLibrary == null)
            {
                return NotFound();
            }

            var file = await fileService.GetByIdAsync(fileId);

            return file == null ? NotFound() : Json(new JsonResponse<string>
            {
                Data = file.Description,
                ServerResponse = true,
                Success = true,
            });
        }

        [HttpGet("{sectionSlug}/[action]/{fileLibrarySlug}/{thumbnailId}")]
        public async Task<IActionResult> GetThumbnail(
            string fileLibrarySlug,
            string sectionSlug,
            int thumbnailId)
        {
            Section section;
            try
            {
                section = await GetSection(sectionSlug);
            }
            catch (OcudaException oex)
            {
                return oex.Data[OcudaExceptionData.HttpResult] is IActionResult
                    ? oex.Data[OcudaExceptionData.HttpResult] as IActionResult
                    : NotFound();
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            if (fileLibrary == null)
            {
                return NotFound();
            }

            string filePath;
            try
            {
                filePath = await fileService.GetThumbnailPathAsync(
                    thumbnailId,
                    fileLibrary.Slug,
                    section.Slug);
            }
            catch (OcudaException)
            {
                return NotFound();
            }

            var fullPath = Path.GetFileName(filePath);

            if (!System.IO.File.Exists(filePath))
            {
                ShowAlertDanger($"Thumbnail not found for library {fileLibrary.Name}: {thumbnailId}");
                _logger.LogError(
                    "Thumbnail id {ThumbnailId} not found at path {FilePath} for library {LibraryName} (id {LibraryId})",
                    thumbnailId,
                    filePath,
                    fileLibrary.Name,
                    fileLibrary.Id);

                return NotFound();
            }

            if (!new FileExtensionContentTypeProvider().TryGetContentType(
                filePath,
                out string contentType))
            {
                _logger.LogError(
                    "Unable to determine file type for {FilePath}, using {ContentType",
                    filePath,
                    contentType);
            }

            return File(
                new FileStream(filePath, FileMode.Open, FileAccess.Read),
                contentType,
                fullPath);
        }

        [HttpGet("{sectionSlug}/[action]/{fileLibrarySlug}")]
        [HttpGet("{sectionSlug}/[action]/{fileLibrarySlug}/{page:int}")]
        public async Task<IActionResult> Files(
            string sectionSlug,
            string fileLibrarySlug,
            int? page)
        {
            Section section = null;
            try
            {
                section = await GetSection(sectionSlug);
            }
            catch (OcudaException oex)
            {
                return oex.Data[OcudaExceptionData.ActionResult] is IActionResult
                    ? oex.Data[OcudaExceptionData.ActionResult] as IActionResult
                    : RedirectToAction(nameof(Index));
            }

            var fileLibrary = await fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);
            if (fileLibrary == null)
            {
                ShowAlertDanger($"File library not found: {fileLibrarySlug}");
                return RedirectToAction(nameof(Index));
            }

            var currentPage = page ?? 1;

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new FilesFilter(currentPage, itemsPerPage)
            {
                FileLibrary = fileLibrary,
            };

            var filesAndCount = await fileService.GetPaginatedListAsync(section, filter);

            foreach (var file in filesAndCount.Data)
            {
                PopulateThumbnailLinks(
                    fileLibrary.Slug,
                    section.Slug,
                    file.Thumbnails);
            }

            var viewModel = new FileLibraryViewModel
            {
                CurrentPage = currentPage,
                GetFileDetailsLink = Url.Action(nameof(GetFileDetailsJson),
                new
                {
                    fileLibrarySlug,
                    sectionSlug,
                }),
                FileLibraryId = fileLibrary.Id,
                FileLibraryName = fileLibrary.Name,
                FileLibrarySlug = fileLibrary.Slug,
                FileLibrarySortOrder = fileLibrary.SortOrder,
                Files = filesAndCount.Data,
                FileTypes = await fileService.GetFileLibrariesFileTypesAsync(fileLibrary.Id),
                HasAdminRights = IsSiteManager(),
                HasManagementRights = await HasPermissionAsync<PermissionGroupSectionManager>(
                    permissionGroupService,
                    section.Id),
                HasReplaceRights = await fileService.HasReplaceRightsAsync(fileLibrary.Id),
                ItemCount = filesAndCount.Count,
                ItemsPerPage = filter.Take.Value,
                MaximumFileUploadBytes = SectionController.MaximumFileUploadBytes,
                SectionName = section.Name,
                SectionSlug = section.Slug,
                UseThumbnails = filesAndCount.Data.Any(_ => _.Thumbnails?.Count > 0),
            };

            return viewModel.PastMaxPage
                ? RedirectToRoute(new { page = viewModel.LastPage ?? 1 })
                : View(viewModel);
        }

        [HttpGet]
        [Route("[action]/{libraryId:int}/{fileId:int}")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> GetFile(int libraryId, int fileId)
        {
            var library = await fileService.GetLibraryByIdAsync(libraryId);
            var section = await sectionService.GetByIdAsync(library.SectionId);

            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            if (section.SupervisorsOnly)
            {
                var isSupervisor = await userService.IsSupervisor(CurrentUserId);
                if (!isSupervisor)
                {
                    return RedirectToUnauthorized();
                }
            }

            var file = await fileService.GetByIdAsync(fileId);
            if (file == null)
            {
                return NotFound();
            }

            var filePath = fileService.GetFileLibraryFilePath(section.Slug, library.Slug, file);
            var filename = Path.GetFileName(filePath);

            if (!System.IO.File.Exists(filePath))
            {
                ShowAlertDanger($"File not found in file library {library.Name}: {filename}");
                _logger.LogError(
                    "File {FileName} not found at path {FilePath} for library {LibraryName} (id {LibraryId})",
                    filename,
                    filePath,
                    library.Name,
                    library.Id);

                return RedirectToAction(nameof(SectionIndex),
                    new { slug = section.Slug });
            }

            return File(new FileStream(filePath, FileMode.Open, FileAccess.Read),
                System.Net.Mime.MediaTypeNames.Application.Octet,
                filename);
        }

        [HttpGet("")]
        [HttpGet("{page:int}")]
        public async Task<IActionResult> Index(int? page)
        {
            var currentPage = page ?? 1;
            return await ShowPostsAsync(new BlogFilter(currentPage, 5)
            {
                IsShownOnHomePage = true,
            },
            currentPage);
        }

        [HttpGet("{sectionSlug}/[action]/{linkLibrarySlug}")]
        [HttpGet("{sectionSlug}/[action]/{linkLibrarySlug}/{page:int}")]
        public async Task<IActionResult> Links(
            string sectionSlug,
            string linkLibrarySlug,
            int? page)
        {
            Section section = null;
            try
            {
                section = await GetSection(sectionSlug);
            }
            catch (OcudaException oex)
            {
                return oex.Data[OcudaExceptionData.ActionResult] is IActionResult
                    ? oex.Data[OcudaExceptionData.ActionResult] as IActionResult
                    : RedirectToAction(nameof(Index));
            }

            var linkLibraries = await linkService.GetBySectionIdAsync(section.Id);
            var linkLibrary = linkLibraries.Single(_ => _.Slug == linkLibrarySlug);
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var currentPage = page ?? 1;

            var filter = new LinksFilter(currentPage, itemsPerPage)
            {
                LinkLibrary = linkLibrary,
            };
            var links = await linkService.GetPaginatedListAsync(filter);

            var viewModel = new LinkLibraryViewModel
            {
                CurrentPage = currentPage,
                HasAdminRights = await HasPermissionAsync<PermissionGroupSectionManager>(
                    permissionGroupService,
                    section.Id),
                IsSiteManager = IsSiteManager(),
                ItemCount = links.Count,
                ItemsPerPage = filter.Take.Value,
                LinkLibrary = linkLibrary,
                SectionName = section.Name,
                SectionSlug = section.Slug,
            };

            viewModel.Links.AddRange(links.Data);
            viewModel.FileTypes.AddRange(await fileService.GetAllFileTypesAsync());

            return viewModel.PastMaxPage
                ? RedirectToRoute(new { page = viewModel.LastPage ?? 1 })
                : View(viewModel);
        }

        [HttpGet("{slug}")]
        [HttpGet("{slug}/{page:int}")]
        public async Task<IActionResult> SectionIndex(string slug, int? page)
        {
            Section section = null;
            try
            {
                section = await GetSection(slug);
            }
            catch (OcudaException oex)
            {
                return oex.Data[OcudaExceptionData.ActionResult] is IActionResult
                    ? oex.Data[OcudaExceptionData.ActionResult] as IActionResult
                    : RedirectToAction(nameof(Index));
            }

            if (section.IsHomeSection)
            {
                return RedirectToAction(nameof(Index));
            }

            var currentPage = page ?? 1;

            var filter = new BlogFilter(currentPage, 5)
            {
                SectionId = section.Id,
            };

            var isAdmin = await HasPermissionAsync<PermissionGroupSectionManager>(
                permissionGroupService,
                section.Id);

            return await ShowPostsAsync(filter, currentPage, isAdmin);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Unauthorized(Uri returnUrl)
        {
            var adminEmail = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Email.AdminAddress);

            string mailLink = null;
            if (!string.IsNullOrEmpty(adminEmail) && returnUrl != null)
            {
                string username = !string.IsNullOrEmpty(CurrentUsername)
                    ? $"({CurrentUsername}) "
                    : null;

                mailLink = $"mailto:{adminEmail}?subject="
                    + Uri.EscapeDataString("Requesting intranet access")
                    + "&body="
                    + Uri.EscapeDataString($"I {username}request access to: {returnUrl}");
            }

            return View(new UnauthorizedViewModel
            {
                AdminEmail = mailLink,
                ReturnUrl = returnUrl?.ToString(),
                Username = CurrentUsername,
            });
        }

        [HttpGet("[action]")]
        public IActionResult Whoami()
        {
            return Json(new UserInformation
            {
                Username = HttpContext.User?.Identity?.Name,
                Authenticated = HttpContext.User?.Identity.IsAuthenticated == true,
                AuthenticatedAt = UserClaim(ClaimType.AuthenticatedAt) != null
                    ? DateTime.Parse(UserClaim(ClaimType.AuthenticatedAt),
                        CultureInfo.InvariantCulture)
                    : null,
            });
        }

        private async Task<Section> GetSection(string slug)
        {
            var section = await sectionService.GetBySlugAsync(slug);
            if (section == null)
            {
                var ocudaException = new OcudaException("Section not found");
                ocudaException.Data[OcudaExceptionData.ActionResult] = NotFound();
                ocudaException.Data[OcudaExceptionData.HttpResult] = NotFound();
                throw ocudaException;
            }

            if (section.SupervisorsOnly)
            {
                var isSupervisor = await userService.IsSupervisor(CurrentUserId);
                if (!isSupervisor)
                {
                    var ocudaException = new OcudaException("Access Denied");
                    ocudaException.Data[OcudaExceptionData.ActionResult] = RedirectToUnauthorized();
                    ocudaException.Data[OcudaExceptionData.HttpResult] = Unauthorized();
                    throw ocudaException;
                }
            }

            return section;
        }

        private async Task<IActionResult> ShowPostsAsync(BlogFilter filter, int page)
        {
            return await ShowPostsAsync(filter, page, false);
        }

        private async Task<IActionResult> ShowPostsAsync(BlogFilter filter, int page, bool isAdmin)
        {
            filter.IncludeDrafts = isAdmin;

            var posts = await postService.GetPaginatedPostsAsync(filter);

            var viewModel = new IndexViewModel
            {
                ItemCount = posts.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value,
                SectionManager = isAdmin,
            };

            viewModel.Posts.AddRange(posts.Data);

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            foreach (var post in viewModel.Posts)
            {
                post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
            }

            int defaultItemsToShow = 10;

            var sectionId = filter.IsShownOnHomePage == true
                ? await sectionService.GetHomeSectionIdAsync()
                : filter.SectionId;

            var section = sectionId.HasValue
                ? await sectionService.GetByIdAsync(sectionId.Value)
                : null;

            if (filter.IsShownOnHomePage == true)
            {
                filter.SectionId = sectionId;
                viewModel.SectionSlug = "Home";
                defaultItemsToShow = 15;
            }
            else
            {
                if (section != null)
                {
                    viewModel.SectionName = section.Name;
                    viewModel.SectionSlug = section.Slug;
                    viewModel.SupervisorsOnly = section.SupervisorsOnly;
                }
            }

            if (filter.SectionId.HasValue)
            {
                var linkLibraries = await linkService
                    .GetBySectionIdAsync(filter.SectionId.Value);

                if (linkLibraries?.Count > 0)
                {
                    foreach (var linkLibrary in linkLibraries)
                    {
                        var links = await linkService.GetPaginatedListAsync(
                            new LinksFilter(1, defaultItemsToShow)
                            {
                                LinkLibrary = linkLibrary,
                            });

                        if (links?.Count > 0)
                        {
                            linkLibrary.Links.AddRange(links.Data);
                        }

                        viewModel.LinkLibraries.Add(linkLibrary);
                    }
                }

                var fileLibraries = await fileService
                    .GetBySectionIdAsync(filter.SectionId.Value);

                if (fileLibraries?.Count > 0)
                {
                    foreach (var fileLibrary in fileLibraries)
                    {
                        var fileLibraryFiles = await fileService
                            .GetPaginatedListAsync(
                                section,
                                new FilesFilter(1, defaultItemsToShow)
                                {
                                    FileLibrary = fileLibrary,
                                });

                        if (fileLibraryFiles?.Count > 0)
                        {
                            fileLibrary.Files.AddRange(fileLibraryFiles.Data);
                            fileLibrary.TotalFilesInLibrary = fileLibraryFiles.Count;
                            viewModel.FileLibraries.Add(fileLibrary);
                        }
                    }
                }
            }

            return View("Index", viewModel);
        }
    }
}
