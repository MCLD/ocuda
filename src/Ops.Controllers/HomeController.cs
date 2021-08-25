using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Home;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers
{
    [Route("")]
    public class HomeController : BaseController<HomeController>
    {
        private readonly IFileService _fileService;
        private readonly ILinkService _linkService;
        private readonly IPostService _postService;
        private readonly ISectionService _sectionService;
        private readonly IUserService _userService;

        public HomeController(ServiceFacades.Controller<HomeController> context,
            IFileService fileService,
            ILinkService linkService,
            IPostService postService,
            ISectionService sectionService,
            IUserService userService) : base(context)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public static string Name { get { return "Home"; } }

        [HttpGet("[action]")]
        public IActionResult Authenticate(Uri returnUrl)
        {
            // by the time we get here the user is probably authenticated - if so we can take them
            // back to their initial destination
            if (HttpContext.Items[ItemKey.Nickname] != null)
            {
                return Redirect(returnUrl?.ToString() ?? nameof(Index));
            }

            TempData[TempDataKey.AlertWarning]
                = $"Could not authenticate you for access to {returnUrl}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "ControllerBase.File handles disposal (dotnet/AspNetCore.Docs#14585)")]
        [Route("[action]/{libraryId:int}/{fileId:int}")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> GetFile(int libraryId, int fileId)
        {
            var library = await _fileService.GetLibraryByIdAsync(libraryId);
            var section = await _sectionService.GetByIdAsync(library.SectionId);

            if (section == null)
            {
                return RedirectToUnauthorized();
            }
            if (section.SupervisorsOnly)
            {
                var isSupervisor = await _userService.IsSupervisor(CurrentUserId);
                if (!isSupervisor)
                {
                    return RedirectToUnauthorized();
                }
            }

            var filePath = await _fileService.GetFilePathAsync(section.Id,
                library.Slug,
                fileId);
            var filename = Path.GetFileName(filePath);

            if (!System.IO.File.Exists(filePath))
            {
                ShowAlertDanger($"File not found in file library {library.Name}: {filename}");
                _logger.LogError("File {FileName} not found at path {FilePath} for library {LibraryName} (id {LibraryId})",
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
        public async Task<IActionResult> Index(int page)
        {
            var showPage = page == default ? 1 : page;
            return await ShowPostsAsync(new BlogFilter(showPage, 5)
            {
                IsShownOnHomePage = true
            }, showPage);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> SectionIndex(string slug, int page)
        {
            var section = await _sectionService.GetBySlugAsync(slug);
            if (section == null)
            {
                return NotFound();
            }

            if (section.SupervisorsOnly)
            {
                var isSupervisor = await _userService.IsSupervisor(CurrentUserId);
                if (!isSupervisor)
                {
                    return RedirectToUnauthorized();
                }
            }

            if (section.IsHomeSection)
            {
                return RedirectToAction(nameof(Index));
            }

            var showPage = page == default ? 1 : page;
            return await ShowPostsAsync(new BlogFilter(showPage, 5)
            {
                SectionId = section.Id
            }, showPage);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Unauthorized(Uri returnUrl)
        {
            var adminEmail = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Email.AdminAddress);

            string mailLink = null;
            if (!string.IsNullOrEmpty(adminEmail) && returnUrl != null)
            {
                mailLink = $"mailto:{adminEmail}?subject="
                    + Uri.EscapeUriString("Requesting intranet access")
                    + "&body="
                    + Uri.EscapeUriString($"I ({CurrentUsername}) request access to: {returnUrl}");
            }

            return View(new UnauthorizedViewModel
            {
                AdminEmail = mailLink,
                ReturnUrl = returnUrl?.ToString(),
                Username = CurrentUsername
            });
        }

        [HttpGet("[action]")]
        public IActionResult Whoami()
        {
            return Json(new UserInformation
            {
                Username = UserClaim(ClaimType.Username),
                Authenticated = !string.IsNullOrEmpty(UserClaim(ClaimType.Username)),
                AuthenticatedAt = UserClaim(ClaimType.AuthenticatedAt) != null
                    ? DateTime.Parse(UserClaim(ClaimType.AuthenticatedAt), CultureInfo.InvariantCulture)
                    : null
            });
        }

        private async Task<IActionResult> ShowPostsAsync(BlogFilter filter, int page)
        {
            var posts = await _postService.GetPaginatedPostsAsync(filter);

            var viewModel = new IndexViewModel
            {
                Posts = posts.Data,
                ItemCount = posts.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            foreach (var post in viewModel.Posts)
            {
                post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
            }

            if (filter.IsShownOnHomePage == true)
            {
                filter.SectionId = await _sectionService.GetHomeSectionIdAsync();
                viewModel.SectionSlug = "Home";
            }
            else
            {
                if (filter.SectionId.HasValue)
                {
                    var section = await _sectionService.GetByIdAsync(filter.SectionId.Value);
                    viewModel.SectionName = section.Name;
                    viewModel.SectionSlug = section.Slug;
                    viewModel.SupervisorsOnly = section.SupervisorsOnly;
                }
            }

            if (filter.SectionId.HasValue)
            {
                var linkLibraries = await _linkService
                    .GetBySectionIdAsync(filter.SectionId.Value);

                if (linkLibraries?.Count > 0)
                {
                    foreach (var linkLibrary in linkLibraries)
                    {
                        linkLibrary.Links = await _linkService
                            .GetLinkLibraryLinksAsync(linkLibrary.Id);
                        viewModel.LinkLibraries.Add(linkLibrary);
                    }
                }

                var fileLibraries = await _fileService.GetBySectionIdAsync(filter.SectionId.Value);

                if (fileLibraries?.Count > 0)
                {
                    foreach (var fileLibrary in fileLibraries)
                    {
                        var fileLibraryFiles = await _fileService
                            .GetPaginatedListAsync(new BlogFilter
                            {
                                FileLibraryId = fileLibrary.Id
                            });

                        if (fileLibraryFiles.Count > 0)
                        {
                            fileLibrary.Files = fileLibraryFiles.Data;
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