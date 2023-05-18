using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area("ContentManagement")]
    [Route("[area]/[controller]")]
    public class SectionController : BaseController<SectionController>
    {
        private readonly IOcudaCache _cache;
        private readonly IFileService _fileService;
        private readonly ILinkService _linkService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly IPostService _postService;
        private readonly ISectionService _sectionService;

        public SectionController(ServiceFacades.Controller<SectionController> context,
            IFileService fileService,
            ILinkService linkService,
            IOcudaCache cache,
            IPostService postService,
            IPermissionGroupService permissionGroupService,
            ISectionService sectionService) : base(context)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public static string Area
        { get { return "ContentManagement"; } }

        public static string Name
        { get { return "Section"; } }

        [HttpPost("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> AddFileLibrary(SectionViewModel viewModel)
        {
            var section = await GetSectionAsManagerAsync(viewModel?.Section?.Slug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            if (string.IsNullOrEmpty(viewModel.FileLibrary.Name))
            {
                ModelState.AddModelError("FileLibrary.Name", "A 'File Library Name' is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionSlug = section.Slug });
            }

            if (string.IsNullOrEmpty(viewModel.FileLibrary.Slug))
            {
                ShowAlertDanger("Invalid name for File Library.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionSlug = section.Slug });
            }

            viewModel.FileLibrary.SectionId = section.Id;
            try
            {
                await _fileService.CreateLibraryAsync(viewModel.FileLibrary);
            }
            catch (Exception ex)
            {
                ShowAlertDanger($"Error: {ex.Message}");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionSlug = section.Slug });
            }

            ShowAlertSuccess("File library created!");
            return RedirectToAction(nameof(SectionController.Section),
                new { sectionSlug = section.Slug });
        }

        [HttpPost("[action]/{sectionSlug}/{fileLibrarySlug}/{permissionGroupId:int}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> AddFilePermissionGroup(string sectionSlug,
            string fileLibrarySlug,
            int permissionGroupId)
        {
            var section = await GetSectionAsManagerAsync(sectionSlug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            var fileLibrary = await _fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            try
            {
                await _permissionGroupService
                    .AddToPermissionGroupAsync<PermissionGroupReplaceFiles>(fileLibrary.Id,
                permissionGroupId);
                AlertInfo = "Group added for file replacement.";
            }
            catch (Exception ex)
            {
                AlertDanger = $"Problem adding permission: {ex.Message}";
            }

            return RedirectToAction(nameof(FileLibraryPermissions), new
            {
                SectionSlug = sectionSlug,
                FileLibrarySlug = fileLibrarySlug
            });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddFileToLibrary(FileLibraryViewModel model)
        {
            var section = await GetSectionAsManagerAsync(model?.SectionSlug);
            if (model == null || section == null)
            {
                return RedirectToUnauthorized();
            }

            var fileLibrary = await _fileService.GetLibraryByIdAsync(model.FileLibraryId);

            var extension = Path.GetExtension(model.UploadFile.FileName);

            string path;
            try
            {
                path = await _fileService.VerifyAddFileAsync(fileLibrary.Id,
                    extension,
                    model.File.Name + extension);
                model.File.FileLibraryId = fileLibrary.Id;
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger(oex.Message);

                return RedirectToAction(nameof(FileLibrary), new
                {
                    sectionSlug = section.Slug,
                    fileLibrarySlug = fileLibrary.Slug,
                    page = model.CurrentPage
                });
            }

            if (model.UploadFile.Length > 0)
            {
                using var fileStream = new FileStream(path, FileMode.Create);
                await model.UploadFile.CopyToAsync(fileStream);
                await _fileService.AddFileLibraryFileAsync(model.File, model.UploadFile);
                ShowAlertSuccess($"Added to {fileLibrary.Name}: {model.File.Name}");
            }
            else
            {
                ShowAlertDanger($"Empty file {model.File.Name} not uploaded successfully.");
            }

            return RedirectToAction(nameof(FileLibrary), new
            {
                sectionSlug = section.Slug,
                fileLibrarySlug = fileLibrary.Slug,
                page = model.CurrentPage
            });
        }

        [HttpPost("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> AddLinkLibrary(SectionViewModel model)
        {
            var section = await GetSectionAsManagerAsync(model?.Section?.Slug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            if (string.IsNullOrEmpty(model.LinkLibrary.Name))
            {
                ModelState.AddModelError("LinkLibrary.Name", "A 'Link Library Name' is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionSlug = model.Section.Slug });
            }
            if (string.IsNullOrEmpty(model.LinkLibrary.Slug))
            {
                ShowAlertDanger("Invalid name for File Library.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionSlug = model.Section.Slug });
            }
            model.LinkLibrary.SectionId = section.Id;
            var linkLib = await _linkService.CreateLibraryAsync(model.LinkLibrary, section.Id);
            ShowAlertSuccess($"Added Link Library '{linkLib.Name}' to '{section.Name}'");
            return RedirectToAction(nameof(SectionController.Section),
                new { sectionSlug = model.Section.Slug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddLinkToLibrary(LinkLibraryViewModel model)
        {
            var section = await GetSectionAsManagerAsync(model?.SectionSlug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            if (ModelState.IsValid)
            {
                model.Link.LinkLibraryId = model.LinkLibraryId;
                var link = await _linkService.CreateAsync(model.Link);
                var linkLib = await _linkService.GetLibraryByIdAsync(model.LinkLibraryId);
                model.LinkLibrary = linkLib;

                ShowAlertSuccess($"Added '{link.Name}' to '{linkLib.Name}'");
                return RedirectToAction(nameof(SectionController.LinkLibrary),
                    new { sectionSlug = model.SectionSlug, linkLibSlug = linkLib.Slug });
            }
            else
            {
                ShowAlertDanger("Could not add Link to Library");
                return RedirectToAction(nameof(SectionController.LinkLibrary),
                    new { sectionSlug = model.SectionSlug, linkLibSlug = model.LinkLibrarySlug });
            }
        }

        [HttpPost("[action]/{slug}/{permissionGroupId}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> AddPermissionGroup(string slug, int permissionGroupId)
        {
            var section = await GetSectionAsManagerAsync(slug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await _permissionGroupService
                    .AddToPermissionGroupAsync<PermissionGroupSectionManager>(section.Id,
                permissionGroupId);
                AlertInfo = "Group added for section management.";
            }
            catch (Exception ex)
            {
                AlertDanger = $"Problem adding permission: {ex.Message}";
            }

            return RedirectToAction(nameof(Permissions), new { slug });
        }

        [HttpGet("{sectionSlug}/[action]")]
        [RestoreModelState]
        public async Task<IActionResult> AddPost(string sectionSlug)
        {
            var section = await GetSectionAsManagerAsync(sectionSlug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            var viewModel = new PostViewModel
            {
                CanPromote = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.IntranetFrontPageManagement),
                Post = new Post(),
                SectionSlug = section.Slug,
                SectionId = section.Id,
                SectionName = section.Name
            };
            return View("Post", viewModel);
        }

        [HttpPost("{sectionSlug}/[action]")]
        [SaveModelState]
        public async Task<IActionResult> AddPost(PostViewModel viewModel)
        {
            Section section = viewModel?.Post?.SectionId != null
                ? await GetSectionAsManagerAsync(viewModel.Post.SectionId)
                : null;

            Post post = null;

            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            var slug = viewModel.Post.Slug?.Trim();

            if (await _postService.GetSectionPostBySlugAsync(slug, section.Id) != null)
            {
                ModelState.AddModelError($"{nameof(Post)}.{nameof(Post.Slug)}", "This slug already exists.");
                ShowAlertDanger("Could not create Post: slug already exists.");
                return RedirectToAction(nameof(AddPost), viewModel);
            }

            if (ModelState.IsValid)
            {
                if (viewModel.Publish)
                {
                    if (!viewModel.PublishAtDate.HasValue && !viewModel.PublishAtTime.HasValue)
                    {
                        viewModel.Post.PublishedAt = DateTime.Now;
                    }
                    else
                    {
                        viewModel.Post.PublishedAt = viewModel.PublishAtDate.Value
                            .CombineWithTime(viewModel.PublishAtTime.Value);
                    }
                }

                if (viewModel.PinUntilDate.HasValue && viewModel.PinUntilTime.HasValue)
                {
                    viewModel.Post.PinnedUntil = viewModel.PinUntilDate.Value
                        .CombineWithTime(viewModel.PinUntilTime.Value);
                }

                try
                {
                    post = await _postService.CreatePostAsync(viewModel.Post);

                    if (viewModel.Publish)
                    {
                        ShowAlertSuccess($"Added post: {post.Title}");
                    }
                    else
                    {
                        ShowAlertSuccess($"Created draft: {post.Title}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Issue creating post: {ErrorMessage}", ex.Message);
                    ShowAlertDanger($"Could not create post: {ex.Message}");
                    return RedirectToAction(nameof(SectionController.PostDetails),
                        new { sectionSlug = section.Slug, postSlug = slug });
                }
            }
            else
            {
                ShowAlertDanger($"Could not create post: {ModelState.ErrorCount} validation errors.");
                return RedirectToAction(nameof(SectionController.PostDetails),
                    new { sectionSlug = section.Slug, postSlug = slug });
            }

            return RedirectToAction(nameof(Ocuda.Ops.Controllers.HomeController.SectionIndex),
                Ocuda.Ops.Controllers.HomeController.Name,
                new
                {
                    area = "",
                    slug = section.Slug
                });
        }

        [HttpPost("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> ClearSectionCache()
        {
            if (string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)))
            {
                return RedirectToUnauthorized();
            }

            await _cache.RemoveAsync(Cache.OpsSections);
            ShowAlertInfo("Section cache cleared.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteFileFromLibrary(FileLibraryViewModel viewModel)
        {
            var section = await GetSectionAsManagerAsync(viewModel.SectionSlug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            if (viewModel.File.Id == default
                || string.IsNullOrEmpty(viewModel.FileLibrarySlug)
                || string.IsNullOrEmpty(viewModel.SectionSlug))
            {
                ShowAlertWarning("You must supply a valid file to delete.");

                return RedirectToAction(nameof(Section), new { sectionSlug = section.Slug });
            }
            else
            {
                try
                {
                    await _fileService.DeletePrivateFileAsync(section.Id,
                        viewModel.FileLibrarySlug,
                        viewModel.File.Id);
                }
                catch (OcudaException oex)
                {
                    ShowAlertDanger(oex.Message);
                }

                return RedirectToAction(nameof(FileLibrary), new
                {
                    sectionSlug = section.Slug,
                    fileLibrarySlug = viewModel.FileLibrarySlug,
                    page = viewModel.CurrentPage
                });
            }
        }

        [HttpPost("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> DeleteFileLibrary(SectionViewModel model)
        {
            var section = await GetSectionAsManagerAsync(model?.Section?.Slug);
            if (section == null || model == null)
            {
                return RedirectToUnauthorized();
            }

            try
            {
                var fileLibrary = await _fileService.GetLibraryByIdAsync(model.FileLibrary.Id);
                var files = await _fileService.GetPaginatedListAsync(new BlogFilter
                {
                    FileLibraryId = fileLibrary.Id,
                    Take = 1
                });

                if (files.Count > 0)
                {
                    ShowAlertDanger($"Please delete all files before deleting {fileLibrary.Name}.");
                    return RedirectToAction(nameof(SectionController.Section),
                        new { sectionSlug = section.Slug });
                }

                await _fileService.DeleteLibraryAsync(section.Id, fileLibrary.Id);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger(oex.Message);
            }
            return RedirectToAction(nameof(FileLibrary),
                new { sectionSlug = model.Section.Slug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteLinkFromLibrary(LinkLibraryViewModel model)
        {
            var section = await GetSectionAsManagerAsync(model?.SectionSlug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            if (model.Link.Id != 0)
            {
                var link = await _linkService.GetByIdAsync(model.Link.Id);
                try
                {
                    await _linkService.DeleteAsync(model.Link.Id);
                    ShowAlertSuccess($"Deleted link '{link.Name}'");
                }
                catch
                {
                    ShowAlertDanger($"Failed to Delete '{link.Name}'");
                }
                return RedirectToAction(nameof(SectionController.LinkLibrary),
                    new { sectionSlug = model.SectionSlug, linkLibSlug = model.LinkLibrarySlug });
            }
            else
            {
                ShowAlertDanger("Failed to Delete Link");
                return RedirectToAction(nameof(SectionController.LinkLibrary),
                    new { sectionSlug = model.SectionSlug, linkLibSlug = model.LinkLibrarySlug });
            }
        }

        [HttpPost("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> DeleteLinkLibrary(SectionViewModel model)
        {
            var section = await GetSectionAsManagerAsync(model?.Section?.Slug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            if (model.LinkLibrary.Id == 0)
            {
                ShowAlertDanger("A 'Link Library Id' is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionSlug = section.Slug });
            }

            var linkLib = await _linkService.GetLibraryByIdAsync(model.LinkLibrary.Id);
            if (linkLib == null)
            {
                ShowAlertDanger("Could not find the link library");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionSlug = section.Slug });
            }

            var libLinks = await _linkService.GetLinkLibraryLinksAsync(linkLib.Id);
            if (libLinks.Count > 0)
            {
                ShowAlertDanger($"Please delete all links before deleting {linkLib.Name}.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionSlug = section.Slug });
            }
            await _linkService.DeleteLibraryAsync(linkLib.Id);
            ShowAlertSuccess($"Deleted '{linkLib.Name}' from '{section.Name}'s");
            return RedirectToAction(nameof(SectionController.Section),
                new { sectionSlug = section.Slug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeletePost(int postId, string sectionSlug)
        {
            var section = await GetSectionAsManagerAsync(sectionSlug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            var post = await _postService.GetPostByIdAsync(postId);
            try
            {
                await _postService.RemovePostAsync(post);
                ShowAlertSuccess($"Deleted post '{post.Title}'");
            }
            catch
            {
                ShowAlertDanger($"Failed to Delete '{post.Title}'");
            }
            return RedirectToAction(nameof(SectionController.Posts),
                new { sectionSlug });
        }

        [HttpGet("{sectionSlug}/[action]/{postSlug}")]
        [RestoreModelState]
        public async Task<IActionResult> EditPost(string sectionSlug, string postSlug)
        {
            var section = await GetSectionAsManagerAsync(sectionSlug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            try
            {
                var post = await _postService.GetSectionPostBySlugAsync(postSlug, section.Id);
                return View("Post", new PostViewModel
                {
                    CanPromote = await HasAppPermissionAsync(_permissionGroupService,
                        ApplicationPermission.IntranetFrontPageManagement),
                    PinUntilDate = post.PinnedUntil,
                    PinUntilTime = post.PinnedUntil,
                    Post = post,
                    SectionId = section.Id,
                    SectionName = section.Name,
                    SectionSlug = section.Slug
                });
            }
            catch
            {
                ShowAlertDanger($"The PostDetails Id {postSlug} does not exist.");
                return RedirectToAction(nameof(SectionController.Section), new { sectionSlug });
            }
        }

        [HttpPost("{sectionSlug}/[action]/{postSlug}")]
        [SaveModelState]
        public async Task<IActionResult> EditPost(PostViewModel viewModel)
        {
            var section = await GetSectionAsManagerAsync(viewModel.Post.SectionId);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            if (ModelState.IsValid)
            {
                if (viewModel.Publish)
                {
                    if (!viewModel.PublishAtDate.HasValue && !viewModel.PublishAtTime.HasValue)
                    {
                        viewModel.Post.PublishedAt = DateTime.Now;
                    }
                    else
                    {
                        viewModel.Post.PublishedAt = viewModel.PublishAtDate.Value
                            .CombineWithTime(viewModel.PublishAtTime.Value);
                    }
                }

                if (viewModel.PinUntilDate.HasValue && viewModel.PinUntilTime.HasValue)
                {
                    viewModel.Post.PinnedUntil = viewModel.PinUntilDate.Value
                        .CombineWithTime(viewModel.PinUntilTime.Value);
                }
                else
                {
                    viewModel.Post.PinnedUntil = null;
                }

                try
                {
                    await _postService.UpdatePostAsync(viewModel.Post);
                    ShowAlertSuccess($"Updated post: {viewModel.Post.Title}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error editing post: {ErrorMessage}", ex.Message);
                    ShowAlertDanger($"Could not edit post: {ex.Message}");
                }
            }
            else
            {
                ShowAlertDanger($"Could not edit post: {ModelState.ErrorCount} validation errors.");
            }

            return RedirectToAction(nameof(Ocuda.Ops.Controllers.HomeController.SectionIndex),
                Ocuda.Ops.Controllers.HomeController.Name,
                new
                {
                    area = "",
                    slug = section.Slug
                });
        }

        [HttpGet("{sectionSlug}/[action]/{fileLibrarySlug}")]
        [HttpGet("{sectionSlug}/[action]/{fileLibrarySlug}/{page}")]
        public async Task<IActionResult> FileLibrary(string sectionSlug,
            string fileLibrarySlug,
            int page)
        {
            var section = await _sectionService.GetBySlugAsync(sectionSlug);
            if (section == null)
            {
                ShowAlertDanger($"Section not found with slug {sectionSlug}.");
                return RedirectToAction(nameof(Index));
            }

            if (page == default)
            {
                page = 1;
            }

            var fileLibrary = await _fileService.GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BlogFilter(page, itemsPerPage)
            {
                FileLibraryId = fileLibrary.Id
            };

            var filesAndCount = await _fileService.GetPaginatedListAsync(filter);

            return View(new FileLibraryViewModel
            {
                HasAdminRights = IsSiteManager()
                    || await HasPermissionAsync<PermissionGroupSectionManager>(_permissionGroupService,
                        section.Id),
                CurrentPage = page,
                FileLibraryId = fileLibrary.Id,
                FileLibraryName = fileLibrary.Name,
                FileLibrarySlug = fileLibrary.Slug,
                Files = filesAndCount.Data,
                FileTypes = await _fileService.GetFileLibrariesFileTypesAsync(fileLibrary.Id),
                ItemCount = filesAndCount.Count,
                ItemsPerPage = filter.Take.Value,
                HasReplaceRights = await _fileService.HasReplaceRightsAsync(fileLibrary.Id),
                SectionName = section.Name,
                SectionSlug = section.Slug
            });
        }

        [HttpGet("[action]/{sectionSlug}/{fileLibrarySlug}")]
        public async Task<IActionResult> FileLibraryPermissions(string sectionSlug,
            string fileLibrarySlug)
        {
            var section = await GetSectionAsManagerAsync(sectionSlug);

            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            var fileLibrary = await _fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            var permissionGroups = await _permissionGroupService.GetAllAsync();
            var fileLibraryPermissions = await _permissionGroupService
                .GetPermissionsAsync<PermissionGroupReplaceFiles>(fileLibrary.Id);

            var viewModel = new FileLibraryPermissionsViewModel
            {
                Name = fileLibrary.Name,
                SectionName = section.Name,
                SectionSlug = section.Slug,
                FileLibrarySlug = fileLibrary.Slug
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
        public async Task<IActionResult> Index()
        {
            var permissionGroupIds = UserClaims(ClaimType.PermissionId)
                .Select(_ => int.Parse(_, CultureInfo.InvariantCulture));

            return View(new SectionIndexViewModel
            {
                IsSiteManager = IsSiteManager(),
                Sections = await _sectionService.GetManagedByCurrentUserAsync()
            });
        }

        [HttpGet("{sectionSlug}/[action]/{linkLibSlug}")]
        [HttpGet("{sectionSlug}/[action]/{linkLibSlug}/{page}")]
        public async Task<IActionResult> LinkLibrary(string sectionSlug,
            string linkLibSlug, int page = 1)
        {
            var section = await GetSectionAsManagerAsync(sectionSlug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            var linkLibs = await _linkService.GetBySectionIdAsync(section.Id);
            var linkLib = linkLibs.Find(_ => _.Slug == linkLibSlug);
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);
            var filter = new BlogFilter(page, itemsPerPage)
            {
                LinkLibraryId = linkLib.Id
            };
            var links = await _linkService.GetPaginatedListAsync(filter);
            var paginateModel = new PaginateModel
            {
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value,
                ItemCount = links.Count
            };
            var viewModel = new LinkLibraryViewModel
            {
                SectionSlug = section.Slug,
                SectionName = section.Name,
                LinkLibrary = linkLib,
                Links = links.Data,
                FileTypes = await _fileService.GetAllFileTypesAsync(),
                PaginateModel = paginateModel
            };
            return View(viewModel);
        }

        [HttpGet("[action]/{slug}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Permissions(string slug)
        {
            var section = await GetSectionAsManagerAsync(slug);

            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            var permissionGroups = await _permissionGroupService.GetAllAsync();
            var sectionPermissions = await _permissionGroupService
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

        [HttpGet("{sectionSlug}/[action]/{postSlug}")]
        public async Task<IActionResult> PostDetails(string sectionSlug, string postSlug)
        {
            var section = await GetSectionAsManagerAsync(sectionSlug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            try
            {
                var post = await _postService.GetSectionPostBySlugAsync(postSlug, section.Id);
                post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
                var viewModel = new PostDetailsViewModel
                {
                    SectionSlug = section.Slug,
                    SectionName = section.Name,
                    SectionCategories = await _postService.GetCategoriesBySectionIdAsync(section.Id),
                    SectionsPosts = await _postService.GetTopSectionPostsAsync(5, section.Id),
                    Post = post
                };
                viewModel.PostCategories = await _postService.GetPostCategoriesByIdAsync(viewModel.Post.Id);
                return View(viewModel);
            }
            catch
            {
                ShowAlertDanger($"The Post {postSlug} does not exist.");
                return RedirectToAction(nameof(SectionController.Section), new { sectionSlug });
            }
        }

        [HttpGet("{sectionSlug}/[action]")]
        [HttpGet("{sectionSlug}/[action]/{page}")]
        [HttpGet("{sectionSlug}/[action]/{categorySlug}/{page}")]
        public async Task<IActionResult> Posts(string sectionSlug, string categorySlug, int page = 1)
        {
            var section = await GetSectionAsManagerAsync(sectionSlug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            var filter = new BlogFilter(page, 5)
            {
                SectionId = section.Id
            };

            var paginateModel = new PaginateModel
            {
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value,
            };

            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }
            var topPosts = await _postService.GetTopSectionPostsAsync(5, section.Id);
            foreach (var topPost in topPosts.ToList())
            {
                topPost.Content = CommonMark.CommonMarkConverter.Convert(topPost.Content);
            }
            var viewModel = new PostsViewModel
            {
                SectionSlug = section.Slug,
                SectionName = section.Name,
                SectionCategories = await _postService.GetCategoriesBySectionIdAsync(section.Id),
                SectionsPosts = topPosts
            };
            if (string.IsNullOrEmpty(categorySlug))
            {
                var posts = await _postService.GetPaginatedPostsAsync(filter);
                foreach (var post in posts.Data.ToList())
                {
                    post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
                }
                viewModel.PostCategories = await _postService.GetPostCategoriesByIdsAsync(
                    posts.Data.Select(_ => _.Id).ToList());
                paginateModel.ItemCount = posts.Count;
                viewModel.PaginateModel = paginateModel;
                viewModel.AllCategoryPosts = posts.Data;
                return View(viewModel);
            }
            else
            {
                try
                {
                    var category = await _postService.GetSectionCategoryBySlugAsync(categorySlug, section.Id);
                    filter.CategoryId = category.Id;
                    var posts = await _postService.GetPaginatedPostsAsync(filter);
                    foreach (var post in posts.Data.ToList())
                    {
                        post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
                    }
                    paginateModel.ItemCount = posts.Count;
                    viewModel.PostCategories = await _postService.GetPostCategoriesByIdsAsync(
                        posts.Data.Select(_ => _.Id).ToList());
                    viewModel.PaginateModel = paginateModel;
                    viewModel.AllCategoryPosts = posts.Data;
                    return View(viewModel);
                }
                catch
                {
                    ShowAlertDanger($"The PostDetails Category {categorySlug} does not exist.");
                    return RedirectToAction(nameof(SectionController.Section), new { sectionSlug });
                }
            }
        }

        [HttpPost("[action]/{sectionSlug}/{fileLibrarySlug}/{permissionGroupId:int}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> RemoveFilePermissionGroup(string sectionSlug,
            string fileLibrarySlug,
            int permissionGroupId)
        {
            var section = await GetSectionAsManagerAsync(sectionSlug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            var fileLibrary = await _fileService
                .GetBySectionIdSlugAsync(section.Id, fileLibrarySlug);

            try
            {
                await _permissionGroupService
                    .RemoveFromPermissionGroupAsync<PermissionGroupReplaceFiles>(fileLibrary.Id,
                permissionGroupId);
                AlertInfo = "Group removed for file replacement.";
            }
            catch (Exception ex)
            {
                AlertDanger = $"Problem adding permission: {ex.Message}";
            }

            return RedirectToAction(nameof(FileLibraryPermissions), new
            {
                SectionSlug = sectionSlug,
                FileLibrarySlug = fileLibrarySlug
            });
        }

        [HttpPost("[action]/{slug}/{permissionGroupId:int}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> RemovePermissionGroup(string slug, int permissionGroupId)
        {
            var section = await GetSectionAsManagerAsync(slug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await _permissionGroupService
                    .RemoveFromPermissionGroupAsync<PermissionGroupSectionManager>(section.Id,
                    permissionGroupId);
                AlertInfo = "Group removed from section management.";
            }
            catch (Exception ex)
            {
                AlertDanger = $"Problem removing permission: {ex.Message}";
            }

            return RedirectToAction(nameof(Permissions), new { slug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ReplaceFile(FileLibraryViewModel viewModel)
        {
            var section = await GetSectionAsManagerAsync(viewModel.SectionSlug);
            if (section == null)
            {
                var hasReplaceRights = await _fileService
                    .HasReplaceRightsAsync(viewModel.FileLibraryId);
                if (!hasReplaceRights)
                {
                    return RedirectToUnauthorized();
                }
                else
                {
                    section = await _sectionService.GetBySlugAsync(viewModel.SectionSlug);
                }
            }

            var file = await _fileService.GetByIdAsync(viewModel.ReplaceFileId);
            var fileType = await _fileService.GetFileTypeByIdAsync(file.FileTypeId);
            var extension = Path.GetExtension(viewModel.UploadFile.FileName);

            if (!fileType.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                ShowAlertWarning($"Could not replace file: uploaded file type ({extension}) did not match existing file type ({fileType.Extension}).");
            }
            else
            {
                var path = await _fileService.GetFilePathAsync(section.Id,
                    viewModel.FileLibrarySlug,
                    file.Id);

                if (viewModel.UploadFile.Length > 0)
                {
                    using var fileStream = new FileStream(path, FileMode.Truncate);
                    await viewModel.UploadFile.CopyToAsync(fileStream);
                    await _fileService.ReplaceFileLibraryFileAsync(file.Id);
                    ShowAlertSuccess($"Replaced {file.Name}");
                }
                else
                {
                    ShowAlertDanger($"Empty file {viewModel.File.Name} not uploaded successfully.");
                }
            }

            return RedirectToAction(nameof(FileLibrary), new
            {
                sectionSlug = viewModel.SectionSlug,
                fileLibrarySlug = viewModel.FileLibrarySlug,
                page = viewModel.CurrentPage
            });
        }

        [HttpGet("{sectionSlug}")]
        [HttpGet("{sectionSlug}/{page}")]
        public async Task<IActionResult> Section(string sectionSlug, int page)
        {
            var section = await GetSectionAsManagerAsync(sectionSlug);
            if (section == null)
            {
                return RedirectToAction(nameof(Controllers.HomeController.SectionIndex),
                    Controllers.HomeController.Name,
                    new
                    {
                        area = "",
                        slug = sectionSlug
                    });
            }

            if (page == 0)
            {
                page = 1;
            }

            var filter = new BlogFilter(page, 5)
            {
                SectionId = section.Id
            };

            var posts = await _postService.GetPaginatedPostsAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = posts.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };
            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new SectionViewModel
            {
                Section = section,
                SectionCategories = await _postService.GetCategoriesBySectionIdAsync(section.Id),
                FileLibraries = await _fileService.GetBySectionIdAsync(section.Id),
                LinkLibraries = await _linkService.GetBySectionIdAsync(section.Id),
                PaginateModel = paginateModel,
                AllPosts = posts.Data.Select(_ =>
                {
                    _.Content = CommonMark.CommonMarkConverter.Convert(_.Content);
                    return _;
                }).ToList()
            };

            var postIds = viewModel.AllPosts
                .Select(_ => _.Id)
                .Skip(page - 1)
                .Take(filter.Take.Value)
                .ToList();
            viewModel.PostCategories = await _postService.GetPostCategoriesByIdsAsync(postIds);

            return View(viewModel);
        }

        [HttpPost("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> UpdateFileLibrary(FileLibraryViewModel viewModel)
        {
            var section = await GetSectionAsManagerAsync(viewModel?.SectionSlug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            if (string.IsNullOrEmpty(viewModel.FileLibraryName))
            {
                ModelState.AddModelError("FileLibrary.Name", "A 'File Library Name' is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionSlug = section.Slug });
            }

            if (string.IsNullOrEmpty(viewModel.FileLibrarySlug))
            {
                ShowAlertDanger("Invalid name for File Library.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionSlug = section.Slug });
            }

            try
            {
                await _fileService.UpdateLibrary(new Models.Entities.FileLibrary
                {
                    Id = viewModel.FileLibraryId,
                    Name = viewModel.FileLibraryName,
                    Slug = viewModel.FileLibrarySlug
                });
                ShowAlertSuccess("File library updated.");
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger(oex.Message);
            }

            return RedirectToAction(nameof(SectionController.Section),
                new { sectionSlug = section.Slug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateLinkFromLibrary(LinkLibraryViewModel model)
        {
            var section = await GetSectionAsManagerAsync(model?.SectionSlug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            if (ModelState.IsValid)
            {
                var link = await _linkService.GetByIdAsync(model.Link.Id);
                try
                {
                    var updateLink = await _linkService.EditAsync(model.Link);
                    ShowAlertSuccess($"Updated link '{updateLink.Name}'");
                }
                catch
                {
                    ShowAlertDanger($"Failed to Update '{link.Name}'");
                }
                return RedirectToAction(nameof(SectionController.LinkLibrary),
                    new { sectionSlug = model.SectionSlug, linkLibSlug = model.LinkLibrarySlug });
            }
            else
            {
                ShowAlertDanger("Could not Update Link");
                return RedirectToAction(nameof(SectionController.LinkLibrary),
                    new { sectionSlug = model.SectionSlug, linkLibSlug = model.LinkLibrarySlug });
            }
        }

        [HttpPost("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> UpdateLinkLibrary(SectionViewModel viewModel)
        {
            var section = await GetSectionAsManagerAsync(viewModel?.Section?.Slug);
            if (section == null)
            {
                return RedirectToUnauthorized();
            }

            if (string.IsNullOrEmpty(viewModel.LinkLibrary.Name))
            {
                ModelState.AddModelError("LinkLibrary.Name", "A 'Link Library Name' is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionSlug = viewModel.Section.Slug });
            }
            if (string.IsNullOrEmpty(viewModel.LinkLibrary.Slug))
            {
                ShowAlertDanger("Invalid name for File Library.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionSlug = viewModel.Section.Slug });
            }
            if (viewModel.LinkLibrary.Id == 0)
            {
                ShowAlertDanger("A Link Library Id is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionSlug = viewModel.Section.Slug });
            }
            if (ModelState.IsValid)
            {
                var oldLib = await _linkService.GetLibraryByIdAsync(viewModel.LinkLibrary.Id);
                oldLib.Name = viewModel.LinkLibrary.Name;
                oldLib.Slug = viewModel.LinkLibrary.Slug;
                await _linkService.UpdateLibraryAsync(oldLib);
                ShowAlertSuccess($"Updated link library '{oldLib.Name}'");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionSlug = section.Slug });
            }
            else
            {
                ShowAlertDanger($"Could Not Update {viewModel.LinkLibrary.Name}");
                return RedirectToAction(nameof(LinkLibrary),
                    new { sectionSlug = viewModel.Section.Slug, linkLibSlug = viewModel.LinkLibrary.Slug });
            }
        }

        private async Task<Section> GetSectionAsManagerAsync(int sectionId)
        {
            var section = await _sectionService.GetByIdAsync(sectionId);
            return await HasPermissionAsync<PermissionGroupSectionManager>(_permissionGroupService,
                section.Id)
                ? section
                : null;
        }

        private async Task<Section> GetSectionAsManagerAsync(string sectionSlug)
        {
            if (string.IsNullOrEmpty(sectionSlug))
            {
                return null;
            }

            var section = await _sectionService.GetBySlugAsync(sectionSlug);
            return await HasPermissionAsync<PermissionGroupSectionManager>(_permissionGroupService,
                section.Id)
                ? section
                : null;
        }
    }
}