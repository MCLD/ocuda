using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area("ContentManagement")]
    //[Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class SectionController : BaseController<SectionController>
    {
        private readonly ISectionService _sectionService;
        private readonly IPostService _postService;
        private readonly IFileService _fileService;
        private readonly ILinkService _linkService;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public static string Name { get { return "Section"; } }
        public static string Area { get { return "ContentManagement"; } }

        private static readonly string mimeType = "application/octet-stream";

        public SectionController(ServiceFacades.Controller<SectionController> context,
            ISectionService sectionService,
            IPostService postService,
            IFileService fileService,
            ILinkService linkService,
            IWebHostEnvironment hostingEnvironment) : base(context)
        {
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
            _postService = postService
                ?? throw new ArgumentNullException(nameof(postService));
            _fileService = fileService
                ?? throw new ArgumentNullException(nameof(fileService));
            _linkService = linkService
                ?? throw new ArgumentNullException(nameof(linkService));
            _hostingEnvironment = hostingEnvironment
                ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index()
        {
            var siteManager = UserClaim(ClaimType.SiteManager);
            List<string> sectionNames;
            List<Section> sections;

            if (string.IsNullOrEmpty(siteManager))
            {
                sectionNames = UserClaims(ClaimType.SectionManager);
                sections = await _sectionService.GetSectionsByNamesAsync(sectionNames);
            }
            else
            {
                sections = await _sectionService.GetAllSectionsAsync();
            }

            var viewModel = new SectionIndexViewModel
            {
                UserSections = sections
            };

            return View(viewModel);
        }

        [Route("{sectionStub}")]
        [Route("{sectionStub}/{page}")]
        public async Task<IActionResult> Section(string sectionStub, int page = 1)
        {
            var section = await ValidateSectionAccess(sectionStub);
            if (section == null)
            {
                ShowAlertDanger($"Could not find section {sectionStub}.");
                return RedirectToAction(nameof(SectionController.Index));
            }

            var filter = new BaseFilter(page, 5);

            var posts = await _postService.GetSectionPaginatedPostsAsync(filter, section.Id);

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
                FileLibraries = await _fileService.GetFileLibrariesBySectionAsync(section.Id),
                LinkLibraries = await _linkService.GetLinkLibrariesBySectionAsync(section.Id),
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

        [Route("{sectionStub}/[action]")]
        [Route("{sectionStub}/[action]/{page}")]
        [Route("{sectionStub}/[action]/{categoryStub}/{page}")]
        public async Task<IActionResult> Posts(string sectionStub, string categoryStub, int page = 1)
        {
            var section = await ValidateSectionAccess(sectionStub);
            if (section == null)
            {
                ShowAlertDanger($"Could not find section {sectionStub}.");
                return RedirectToAction(nameof(SectionController.Index));
            }

            var filter = new BaseFilter(page, 5);

            var paginateModel = new PaginateModel
            {
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
            var topPosts = await _postService.GetTopSectionPostsAsync(5, section.Id);
            foreach (var topPost in topPosts.ToList())
            {
                topPost.Content = CommonMark.CommonMarkConverter.Convert(topPost.Content);
            }
            var viewModel = new PostsViewModel
            {
                SectionStub = section.Stub,
                SectionName = section.Name,
                SectionCategories = await _postService.GetCategoriesBySectionIdAsync(section.Id),
                SectionsPosts = topPosts
            };
            if (string.IsNullOrEmpty(categoryStub))
            {
                var posts = await _postService.GetSectionPaginatedPostsAsync(filter, section.Id);
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
                    var category = await _postService.GetSectionCategoryByStubAsync(categoryStub, section.Id);
                    var posts = await _postService
                        .GetSectionCategoryPaginatedPostListAsync(filter, section.Id, category.Id);
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
                    ShowAlertDanger($"The PostDetails Category {categoryStub} does not exist.");
                    return RedirectToAction(nameof(SectionController.Section), new { sectionStub });
                }
            }
        }

        [Route("{sectionStub}/[action]/{postStub}")]
        public async Task<IActionResult> PostDetails(string sectionStub, string postStub)
        {
            var section = await ValidateSectionAccess(sectionStub);
            if (section == null)
            {
                ShowAlertDanger($"Could not find section {sectionStub}.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            try
            {
                var post = await _postService.GetSectionPostByStubAsync(postStub, section.Id);
                post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
                var viewModel = new PostDetailsViewModel
                {
                    SectionStub = section.Stub,
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
                ShowAlertDanger($"The Post {postStub} does not exist.");
                return RedirectToAction(nameof(SectionController.Section), new { sectionStub });
            }
        }

        [HttpGet("{sectionStub}/[action]")]
        [RestoreModelState]
        public async Task<IActionResult> AddPost(string sectionStub)
        {
            var section = await ValidateSectionAccess(sectionStub);
            if (section == null)
            {
                ShowAlertDanger($"Could not find section {sectionStub}.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            var viewModel = new AddPostViewModel
            {
                SectionStub = section.Stub,
                SectionId = section.Id,
                SectionName = section.Name,
                SectionCategories = await _postService.GetCategoriesBySectionIdAsync(section.Id)
            };
            viewModel.SelectionPostCategories = new SelectList(viewModel.SectionCategories, "Id", "Name");
            return View(viewModel);
        }

        [HttpPost]
        [Route("{sectionStub}/[action]")]
        [SaveModelState]
        public async Task<IActionResult> AddPost(AddPostViewModel viewModel)
        {
            var section = await _sectionService.GetByIdAsync(viewModel.Post.SectionId);
            if (await _postService.GetSectionPostByStubAsync(viewModel.Post.Stub, section.Id) != null)
            {
                ModelState.AddModelError("Post.Stub", "This 'Stub' already exists.");
                ShowAlertDanger($"Could not create Post.");
                return RedirectToAction(nameof(AddPost), viewModel);
            }
            if (ModelState.IsValid)
            {
                viewModel.SectionCategories = await _postService.GetCategoriesBySectionIdAsync(section.Id);
                viewModel.SelectionPostCategories = new SelectList(viewModel.SectionCategories, "Id", "Name");
                try
                {
                    await _postService.CreatePostAsync(viewModel.Post);

                    var post = await _postService.GetSectionPostByStubAsync(
                        viewModel.Post.Stub.Trim(), section.Id);
                    await _postService.UpdatePostCategoriesAsync(viewModel.CategoryIds, post.Id);

                    ShowAlertSuccess($"Added post '{viewModel.Post.Title}'");
                    return RedirectToAction(nameof(SectionController.PostDetails),
                        new { sectionStub = section.Stub, postStub = post.Stub.Trim() });
                }
                catch
                {
                    ShowAlertDanger("Could not create post.");
                    return RedirectToAction(nameof(SectionController.PostDetails),
                        new { sectionStub = section.Stub, postStub = viewModel.Post.Stub.Trim() });
                }
            }
            else
            {
                ShowAlertDanger("Could not create post.");
                return RedirectToAction(nameof(SectionController.PostDetails),
                    new { sectionStub = section.Stub, postStub = viewModel.Post.Stub.Trim() });
            }
        }

        [HttpGet("{sectionStub}/[action]/{postStub}")]
        [RestoreModelState]
        public async Task<IActionResult> EditPost(string sectionStub, string postStub)
        {
            var section = await ValidateSectionAccess(sectionStub);
            if (section == null)
            {
                ShowAlertDanger($"Could not find section {sectionStub}.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            try
            {
                var post = await _postService.GetSectionPostByStubAsync(postStub, section.Id);
                var sectionCategories = await _postService.GetCategoriesBySectionIdAsync(section.Id);
                var viewModel = new EditPostViewModel
                {
                    SectionStub = section.Stub,
                    SectionId = section.Id,
                    Post = post,
                    SelectionPostCategories = new SelectList(sectionCategories, "Id", "Name"),
                    PostCategories = await _postService.GetPostCategoriesByIdAsync(post.Id)
                };
                viewModel.CategoryIds = viewModel.PostCategories.Select(_ => _.CategoryId).ToList();
                return View(viewModel);
            }
            catch
            {
                ShowAlertDanger($"The PostDetails Id {postStub} does not exist.");
                return RedirectToAction(nameof(SectionController.Section), new { sectionStub });
            }
        }

        [Route("{sectionStub}/[action]/{postStub}")]
        [HttpPost]
        [SaveModelState]
        public async Task<IActionResult> EditPost(EditPostViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var section = await _sectionService.GetByIdAsync(viewModel.Post.SectionId);
                    await _postService.UpdatePostAsync(viewModel.Post);
                    await _postService.UpdatePostCategoriesAsync(viewModel.CategoryIds, viewModel.Post.Id);
                    ShowAlertSuccess($"Updated post '{viewModel.Post.Title}'");
                    return RedirectToAction(nameof(SectionController.PostDetails),
                        new { sectionStub = section.Stub, postStub = viewModel.Post.Stub });
                }
                catch
                {
                    ShowAlertDanger($"Could not edit Post.");
                    return RedirectToAction(nameof(SectionController.EditPost),
                        new { sectionStub = viewModel.SectionStub, postStub = viewModel.Post.Stub });
                }
            }
            else
            {
                ShowAlertDanger($"Could not edit Post.");
                return RedirectToAction(nameof(SectionController.EditPost),
                    new { sectionStub = viewModel.SectionStub, postStub = viewModel.Post.Stub });
            }
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> DeletePost(int postId, string sectionStub)
        {
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
                new { sectionStub });
        }

        [Route("{sectionStub}/[action]/{fileLibStub}")]
        [Route("{sectionStub}/[action]/{fileLibStub}/{page}")]
        public async Task<IActionResult> FileLibrary(string sectionStub,
            string fileLibStub, int page = 1)
        {
            var section = await ValidateSectionAccess(sectionStub);
            if (section == null)
            {
                ShowAlertDanger($"Could not find section {sectionStub}.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            var fileLibs = await _fileService.GetFileLibrariesBySectionAsync(section.Id);
            var fileLib = fileLibs.Find(_ => _.Stub == fileLibStub);
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);
            var filter = new BlogFilter(page, itemsPerPage)
            {
                FileLibraryId = fileLib.Id
            };
            var files = await _fileService.GetPaginatedListAsync(filter);
            var paginateModel = new PaginateModel
            {
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value,
                ItemCount = files.Count
            };
            var viewModel = new FileLibraryViewModel
            {
                SectionStub = section.Stub,
                SectionName = section.Name,
                FileLibraryStub = fileLib.Stub,
                FileLibraryId = fileLib.Id,
                FileLibraryName = fileLib.Name,
                Files = files.Data,
                FileTypes = await _fileService.GetFileLibrariesFileTypesAsync(fileLib.Id),
                PaginateModel = paginateModel
            };
            return View(viewModel);
        }

        [Route("{sectionStub}/[action]/{linkLibStub}")]
        [Route("{sectionStub}/[action]/{linkLibStub}/{page}")]
        public async Task<IActionResult> LinkLibrary(string sectionStub,
            string linkLibStub, int page = 1)
        {
            var section = await ValidateSectionAccess(sectionStub);
            if (section == null)
            {
                ShowAlertDanger($"Could not find section {sectionStub}.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            var linkLibs = await _linkService.GetLinkLibrariesBySectionAsync(section.Id);
            var linkLib = linkLibs.Find(_ => _.Stub == linkLibStub);
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
                SectionStub = section.Stub,
                SectionName = section.Name,
                LinkLibrary = linkLib,
                Links = links.Data,
                FileTypes = await _fileService.GetAllFileTypesAsync(),
                PaginateModel = paginateModel
            };
            return View(viewModel);
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult> GetFile(int fileLibId, int fileId)
        {
            var library = await _fileService.GetLibraryByIdAsync(fileLibId);
            var sect = await _sectionService.GetByIdAsync(library.SectionId);
            var section = await ValidateSectionAccess(sect.Stub);
            if (section == null)
            {
                ShowAlertDanger($"Could not find section {sect.Stub}.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            var file = await _fileService.GetByIdAsync(fileId);
            var type = await _fileService.GetFileTypeByIdAsync(file.FileTypeId);
            var filePath = Path.Combine(
                Directory.GetParent(_hostingEnvironment.WebRootPath).FullName, "shared");
            filePath = Path.Combine(filePath, "sections");
            filePath = Path.Combine(filePath, section.Stub);
            filePath = Path.Combine(filePath, library.Stub);
            filePath = Path.Combine(filePath, file.Name + type.Extension);
            if (!System.IO.File.Exists(filePath))
            {
                ShowAlertDanger($"'{file.Name}' file not found at: { filePath}");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = library.Stub });
            }
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
            return File(fs, mimeType, file.Name + type.Extension);
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> AddFileLibrary(SectionViewModel viewModel)
        {
            var section = await _sectionService.GetSectionByStubAsync(viewModel.Section.Stub);
            if (section == null)
            {
                ShowAlertDanger("Could not find section.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            if (string.IsNullOrEmpty(viewModel.FileLibrary.Name))
            {
                ModelState.AddModelError("FileLibrary.Name", "A 'File Library Name' is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            if (string.IsNullOrEmpty(viewModel.FileLibrary.Stub))
            {
                ShowAlertDanger("Invalid name for File Library.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            viewModel.FileLibrary.SectionId = section.Id;
            await _fileService.CreateLibraryAsync(viewModel.FileLibrary, section.Id);
            var fileLibs = await _fileService.GetFileLibrariesBySectionAsync(section.Id);
            var fileLib = fileLibs.Find(_ => _.Stub == viewModel.FileLibrary.Stub.Trim());
            var fileTypes = await _fileService.GetAllFileTypeIdsAsync();
            await _fileService.EditLibraryTypesAsync(fileLib, fileTypes);
            var filePath = Path.Combine(
                Directory.GetParent(_hostingEnvironment.WebRootPath).FullName, "shared");
            filePath = Path.Combine(filePath, "sections");
            filePath = Path.Combine(filePath, section.Stub);
            filePath = Path.Combine(filePath, fileLib.Stub);
            if (Directory.Exists(filePath))
            {
                ShowAlertDanger($"The File Library '{fileLib.Stub}' already exists for this section.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            else
            {
                Directory.CreateDirectory(filePath);
            }
            ShowAlertSuccess($"Added '{fileLib.Name}' to '{section.Name}'s File Library'");
            return RedirectToAction(nameof(SectionController.Section),
                new { sectionStub = section.Stub });
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> UpdateFileLibrary(FileLibraryViewModel viewModel)
        {
            var section = await _sectionService.GetSectionByStubAsync(viewModel.SectionStub);
            if (section == null)
            {
                ShowAlertDanger("Could not find section.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            if (string.IsNullOrEmpty(viewModel.FileLibraryName))
            {
                ModelState.AddModelError("FileLibrary.Name", "A 'File Library Name' is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            if (string.IsNullOrEmpty(viewModel.FileLibraryStub))
            {
                ShowAlertDanger("Invalid name for File Library.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            var oldLib = await _fileService.GetLibraryByIdAsync(viewModel.FileLibraryId);
            var filePath = Path.Combine(
                Directory.GetParent(_hostingEnvironment.WebRootPath).FullName, "shared");
            filePath = Path.Combine(filePath, "sections");
            filePath = Path.Combine(filePath, section.Stub);
            var newPath = Path.Combine(filePath, viewModel.FileLibraryStub);
            var oldPath = Path.Combine(filePath, oldLib.Stub);
            if (Directory.Exists(newPath))
            {
                ShowAlertDanger($"The File Library '{viewModel.FileLibraryName}' already exists for this section.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            else
            {
                Directory.CreateDirectory(newPath);

                var oldDir = new DirectoryInfo(oldPath);
                var files = oldDir.GetFiles();

                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(newPath, file.Name);
                    file.CopyTo(temppath, false);
                }

                Directory.Delete(oldPath, true);
                oldLib.Name = viewModel.FileLibraryName;
                oldLib.Stub = viewModel.FileLibraryStub;
                var types = new List<int>();
                await _fileService.UpdateLibrary(oldLib);
                await _fileService.EditLibraryTypesAsync(oldLib, types);
            }
            ShowAlertSuccess($"Added '{viewModel.FileLibraryName}' to '{section.Name}'s File Library'");
            return RedirectToAction(nameof(SectionController.Section),
                new { sectionStub = section.Stub });
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> DeleteFileLibrary(SectionViewModel model)
        {
            try
            {
                var section = await _sectionService.GetSectionByStubAsync(model.Section.Stub);
                var fileLib = await _fileService.GetLibraryByIdAsync(model.FileLibrary.Id);
                var libFiles = await _fileService.GetFileLibraryFilesAsync(fileLib.Id);
                if (libFiles.Count > 0)
                {
                    ShowAlertDanger($"Please delete all files before deleting {fileLib.Name}.");
                    return RedirectToAction(nameof(SectionController.Section),
                        new { sectionStub = section.Stub });
                }

                var filePath = Path.Combine(
                    Directory.GetParent(_hostingEnvironment.WebRootPath).FullName, "shared");
                filePath = Path.Combine(filePath, "sections");
                filePath = Path.Combine(filePath, section.Stub);
                filePath = Path.Combine(filePath, fileLib.Stub);
                if (!Directory.Exists(filePath))
                {
                    _logger.LogError("The File Library {FileLibraryName} does not exist for this section.",
                        fileLib.Stub);
                    ShowAlertDanger($"Failed to delete File Library '{fileLib.Name}'");
                    return RedirectToAction(nameof(SectionController.Section),
                        new { sectionStub = section.Stub });
                }
                else
                {
                    Directory.Delete(filePath);
                    await _fileService.DeleteFileTypesByLibrary(fileLib.Id);
                    await _fileService.DeleteLibraryAsync(fileLib.Id);
                    ShowAlertSuccess($"Deleted '{fileLib.Name}' from '{section.Name}'s");
                }
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            catch
            {
                ShowAlertDanger($"Failed to delete File Library.");
                return RedirectToAction(nameof(FileLibrary),
                    new { sectionStub = model.Section.Stub });
            }
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> DeleteFileFromLibrary(FileLibraryViewModel viewModel)
        {
            if (viewModel.File.Id != 0 && viewModel.FileLibraryId != 0
                && !string.IsNullOrEmpty(viewModel.SectionStub))
            {
                var section = await _sectionService.GetSectionByStubAsync(viewModel.SectionStub);
                var fileLib = await _fileService.GetLibraryByIdAsync(viewModel.FileLibraryId);
                var file = await _fileService.GetByIdAsync(viewModel.File.Id);
                var filePath = Path.Combine(
                    Directory.GetParent(_hostingEnvironment.WebRootPath).FullName, "shared");
                filePath = Path.Combine(filePath, "sections");
                var type = await _fileService.GetFileTypeByIdAsync(file.FileTypeId);
                filePath = Path.Combine(filePath, section.Stub);
                filePath = Path.Combine(filePath, fileLib.Stub);
                filePath = Path.Combine(filePath, file.Name + type.Extension);
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogError("{Filename} file not found at: {Path}",
                        file.Name,
                        filePath);
                    ShowAlertDanger($"Failed to delete File Library '{file.Name}'");
                    return RedirectToAction(nameof(SectionController.FileLibrary),
                        new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
                }
                else
                {
                    System.IO.File.Delete(filePath);
                    await _fileService.DeletePrivateFileAsync(file.Id);
                    ShowAlertSuccess($"Deleted '{file.Name}' from '{fileLib.Name}'");
                    return RedirectToAction(nameof(SectionController.FileLibrary),
                        new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
                }
            }
            else
            {
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = viewModel.SectionStub, fileLibStub = viewModel.FileLibraryStub });
            }
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> AddFileToLibrary(FileLibraryViewModel model)
        {
            var section = await _sectionService.GetSectionByStubAsync(model.SectionStub);
            var fileLib = await _fileService.GetLibraryByIdAsync(model.FileLibraryId);
            model.File.FileLibraryId = fileLib.Id;
            var extension = Path.GetExtension(model.UploadFile.FileName).ToLower();
            var libraryTypes = await _fileService.GetFileLibrariesFileTypesAsync(fileLib.Id);
            if (libraryTypes == null)
            {
                ShowAlertDanger("This library doesn't have any file types.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            var fileType = libraryTypes.FirstOrDefault(_ => _.Extension.ToLower() == extension);
            if (fileType == null)
            {
                _logger.LogError("{Library} does not allow '{Extension}' files to be uploaded",
                    fileLib.Name,
                    extension);
                ShowAlertDanger($"File extension '{extension}' is not allowed.");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
            }
            var filePath = Path.Combine(
                Directory.GetParent(_hostingEnvironment.WebRootPath).FullName, "shared/sections");
            filePath = Path.Combine(filePath, section.Stub);
            filePath = Path.Combine(filePath, fileLib.Stub);
            filePath = Path.Combine(filePath, model.File.Name + extension);
            if (System.IO.File.Exists(filePath))
            {
                ShowAlertDanger($"File name '{model.File.Name}' already exists.");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
            }
            if (model.UploadFile.Length > 0)
            {
                using var fileStream = new FileStream(filePath, FileMode.Create);
                await model.UploadFile.CopyToAsync(fileStream);
            }
            await _fileService.CreatePrivateFileAsync(model.File, model.UploadFile);
            ShowAlertSuccess($"Added '{model.File.Name}' to '{fileLib.Name}'");
            return RedirectToAction(nameof(SectionController.FileLibrary),
                new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> AddLinkLibrary(SectionViewModel model)
        {
            if (string.IsNullOrEmpty(model.LinkLibrary.Name))
            {
                ModelState.AddModelError("LinkLibrary.Name", "A 'Link Library Name' is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = model.Section.Stub });
            }
            if (string.IsNullOrEmpty(model.LinkLibrary.Stub))
            {
                ShowAlertDanger("Invalid name for File Library.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = model.Section.Stub });
            }
            var section = await _sectionService.GetSectionByStubAsync(model.Section.Stub);
            model.LinkLibrary.SectionId = section.Id;
            var linkLib = await _linkService.CreateLibraryAsync(model.LinkLibrary, section.Id);
            ShowAlertSuccess($"Added Link Library '{linkLib.Name}' to '{section.Name}'");
            return RedirectToAction(nameof(SectionController.Section),
                new { sectionStub = model.Section.Stub });
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> UpdateLinkLibrary(SectionViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.LinkLibrary.Name))
            {
                ModelState.AddModelError("LinkLibrary.Name", "A 'Link Library Name' is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = viewModel.Section.Stub });
            }
            if (string.IsNullOrEmpty(viewModel.LinkLibrary.Stub))
            {
                ShowAlertDanger("Invalid name for File Library.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = viewModel.Section.Stub });
            }
            if (viewModel.LinkLibrary.Id == 0)
            {
                ShowAlertDanger("A Link Library Id is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = viewModel.Section.Stub });
            }
            if (ModelState.IsValid)
            {
                var section = await _sectionService.GetSectionByStubAsync(viewModel.Section.Stub);
                var oldLib = await _linkService.GetLibraryByIdAsync(viewModel.LinkLibrary.Id);
                oldLib.Name = viewModel.LinkLibrary.Name;
                oldLib.Stub = viewModel.LinkLibrary.Stub;
                await _linkService.UpdateLibraryAsync(oldLib);
                ShowAlertSuccess($"Updated link library '{oldLib.Name}'");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            else
            {
                ShowAlertDanger($"Could Not Update {viewModel.LinkLibrary.Name}");
                return RedirectToAction(nameof(LinkLibrary),
                    new { sectionStub = viewModel.Section.Stub, linkLibStub = viewModel.LinkLibrary.Stub });
            }
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> AddLinkToLibrary(LinkLibraryViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Link.LinkLibraryId = model.LinkLibraryId;
                var link = await _linkService.CreateAsync(model.Link);
                var linkLib = await _linkService.GetLibraryByIdAsync(model.LinkLibraryId);
                model.LinkLibrary = linkLib;

                ShowAlertSuccess($"Added '{link.Name}' to '{linkLib.Name}'");
                return RedirectToAction(nameof(SectionController.LinkLibrary),
                    new { sectionStub = model.SectionStub, linkLibStub = linkLib.Stub });
            }
            else
            {
                ShowAlertDanger($"Could not add Link to Library");
                return RedirectToAction(nameof(SectionController.LinkLibrary),
                    new { sectionStub = model.SectionStub, linkLibStub = model.LinkLibraryStub });
            }
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> DeleteLinkFromLibrary(LinkLibraryViewModel model)
        {
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
                    new { sectionStub = model.SectionStub, linkLibStub = model.LinkLibraryStub });
            }
            else
            {
                ShowAlertDanger($"Failed to Delete Link");
                return RedirectToAction(nameof(SectionController.LinkLibrary),
                    new { sectionStub = model.SectionStub, linkLibStub = model.LinkLibraryStub });
            }
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> UpdateLinkFromLibrary(LinkLibraryViewModel model)
        {
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
                    new { sectionStub = model.SectionStub, linkLibStub = model.LinkLibraryStub });
            }
            else
            {
                ShowAlertDanger($"Could not Update Link");
                return RedirectToAction(nameof(SectionController.LinkLibrary),
                    new { sectionStub = model.SectionStub, linkLibStub = model.LinkLibraryStub });
            }
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> DeleteLinkLibrary(SectionViewModel model)
        {
            var section = await _sectionService.GetSectionByStubAsync(model.Section.Stub);

            if (model.LinkLibrary.Id == 0)
            {
                ShowAlertDanger("A 'Link Library Id' is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }

            var linkLib = await _linkService.GetLibraryByIdAsync(model.LinkLibrary.Id);
            if (linkLib == null)
            {
                ShowAlertDanger("Could not find the link library");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }

            var libLinks = await _linkService.GetLinkLibraryLinksAsync(linkLib.Id);
            if (libLinks.Count > 0)
            {
                ShowAlertDanger($"Please delete all links before deleting {linkLib.Name}.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            await _linkService.DeleteLibraryAsync(linkLib.Id);
            ShowAlertSuccess($"Deleted '{linkLib.Name}' from '{section.Name}'s");
            return RedirectToAction(nameof(SectionController.Section),
                new { sectionStub = section.Stub });
        }

        private async Task<Section> ValidateSectionAccess(string sectionStub)
        {
            var siteManager = UserClaim(ClaimType.SiteManager);
            if (string.IsNullOrEmpty(siteManager))
            {
                var sectionNames = UserClaims(ClaimType.SectionManager);
                return (await _sectionService.GetSectionsByNamesAsync(sectionNames))
                    .Find(_ => _.Stub == sectionStub);
            }
            else
            {
                return (await _sectionService.GetAllSectionsAsync())
                    .Find(_ => _.Stub == sectionStub);
            }
        }
    }
}
