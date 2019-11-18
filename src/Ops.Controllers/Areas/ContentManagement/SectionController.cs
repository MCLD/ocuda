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
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels;
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
        private readonly IHostingEnvironment _hostingEnvironment;

        public static string Name { get { return "Section"; } }
        public static string mimeType = "application/octet-stream";

        public SectionController(ServiceFacades.Controller<SectionController> context,
            ISectionService sectionService,
            IPostService postService,
            IFileService fileService,
            ILinkService linkService,
            IHostingEnvironment hostingEnvironment) : base(context)
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
            var viewModel = new SectionViewModel()
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
            else
            {
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

                var categories = await _postService.GetCategoriesBySectionIdAsync(section.Id);
                var posts = await _postService.GetSectionPaginatedPostsAsync(filter, section.Id);
                paginateModel.ItemCount = posts.Count;
                var viewModel = new SectionViewModel()
                {
                    Section = section,
                    SectionCategories = categories,
                    FileLibraries = await _fileService.GetFileLibrariesBySection(section.Id),
                    LinkLibraries = await _linkService.GetLinkLibrariesBySection(section.Id),
                    PaginateModel = paginateModel,
                    AllPosts = posts.Data.Select(_ => {
                        _.Content = CommonMark.CommonMarkConverter.Convert(_.Content);
                        return _;
                    }).ToList()
                };
                var postIds = viewModel.AllPosts
                    .Select(_ => _.Id)
                    .Skip(page - 1)
                    .Take(filter.Take.Value)
                    .ToList();
                viewModel.PostCategories = await _postService.GetPostCategoriesByIds(postIds);
                return View(viewModel);
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
                var post = _postService.GetSectionPostByStub(postStub,section.Id);
                post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
                var viewModel = new SectionViewModel()
                {
                    Section = section,
                    SectionCategories = await _postService.GetCategoriesBySectionIdAsync(section.Id),
                    SectionsPosts = await _postService.GetTopSectionPostsAsync(5, section.Id),
                    Post = post
                };
                viewModel.PostCategories = await _postService.GetPostCategoriesById(viewModel.Post.Id);
                return View(viewModel);
            }
            catch
            {
                ShowAlertDanger($"The Post {postStub} does not exist.");
                return RedirectToAction(nameof(SectionController.Section), new { sectionStub });
            }
        }

        [Route("{sectionStub}/[action]/{postStub}")]
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
                var post = _postService.GetSectionPostByStub(postStub,section.Id);
                var viewModel = new SectionViewModel()
                {
                    Section = section,
                    SectionCategories = await _postService.GetCategoriesBySectionIdAsync(section.Id),
                    SectionsPosts = await _postService.GetTopSectionPostsAsync(5, section.Id),
                    Post = post,
                    Action = "Edit"
                };
                viewModel.SelectionPostCategories = new SelectList(viewModel.SectionCategories, "Id", "Name");
                viewModel.PostCategories = await _postService.GetPostCategoriesById(viewModel.Post.Id);
                viewModel.CategoryIds = viewModel.PostCategories.Select(_ => _.CategoryId).ToList();
                return View("ModifyPost", viewModel);
            }
            catch
            {
                ShowAlertDanger($"The PostDetails Id {postStub} does not exist.");
                return RedirectToAction(nameof(SectionController.Section), new { sectionStub });
            }
        }

        [Route("{sectionStub}/[action]")]
        [Route("{sectionStub}/[action]/{page}")]
        [Route("{sectionStub}/[action]/{categoryStub}/{page}")]
        public async Task<IActionResult> Post(string sectionStub, string categoryStub, int page = 1)
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
            var viewModel = new SectionViewModel()
            {
                Section = section,
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
                viewModel.PostCategories = await _postService.GetPostCategoriesByIds(posts.Data.Select(_=>_.Id).ToList());
                paginateModel.ItemCount = posts.Count;
                viewModel.PaginateModel = paginateModel;
                viewModel.AllCategoryPosts = posts.Data;
                return View(viewModel);
            }
            else
            {
                try
                {
                    var category = await _postService.GetSectionCategoryByStubAsync(categoryStub,section.Id);
                    var posts = await _postService
                        .GetSectionCategoryPaginatedPostListAsync(filter, section.Id, category.Id);
                    foreach (var post in posts.Data.ToList())
                    {
                        post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
                    }
                    paginateModel.ItemCount = posts.Count;
                    viewModel.PostCategories = await _postService.GetPostCategoriesByIds(posts.Data.Select(_ => _.Id).ToList());
                    viewModel.PaginateModel = paginateModel;
                    viewModel.AllCategoryPosts = posts.Data;
                    viewModel.Category = category;
                    return View(viewModel);
                }
                catch
                {
                    ShowAlertDanger($"The PostDetails Category {categoryStub} does not exist.");
                    return RedirectToAction(nameof(SectionController.Section), new { sectionStub });
                }
            }
        }

        [Route("{sectionStub}/[action]")]
        [RestoreModelState]
        public async Task<IActionResult> AddPost(string sectionStub)
        {
            var section = await ValidateSectionAccess(sectionStub);
            if (section == null)
            {
                ShowAlertDanger($"Could not find section {sectionStub}.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            var viewModel = new SectionViewModel()
            {
                Section = section,
                SectionCategories = await _postService.GetCategoriesBySectionIdAsync(section.Id),
                Action = "Add"
            };
            viewModel.SelectionPostCategories = new SelectList(viewModel.SectionCategories, "Id", "Name");
            return View("ModifyPost", viewModel);
        }

        [Route("[action]")]
        [HttpPost]
        [SaveModelState]
        public async Task<IActionResult> CreatePost(SectionViewModel viewModel)
        {
            var section = await _sectionService.GetByIdAsync(viewModel.Post.SectionId);
            viewModel.Section = section;
            viewModel.SectionCategories = await _postService.GetCategoriesBySectionIdAsync(section.Id);
            viewModel.SelectionPostCategories = new SelectList(viewModel.SectionCategories, "Id", "Name");
            if (string.IsNullOrEmpty(viewModel.Post.Stub))
            {
                ModelState.AddModelError("Post.Stub", "A 'Post Stub' is required.");
                ShowAlertDanger($"Could not create Post.");
                return View("ModifyPost", viewModel);
            }
            else if(_postService.GetSectionPostByStub(viewModel.Post.Stub,section.Id) != null)
            {
                ModelState.AddModelError("Post.Stub", "This 'Stub' already exists.");
                ShowAlertDanger($"Could not create Post.");
                return View("ModifyPost", viewModel);
            }
            if (string.IsNullOrEmpty(viewModel.Post.Title))
            {
                ModelState.AddModelError("Post.Title", "A 'Post Title' is required.");
                ShowAlertDanger($"Could not create Post.");
                return View("ModifyPost", viewModel);
            }
            if (string.IsNullOrEmpty(viewModel.Post.Content))
            {
                ModelState.AddModelError("Post.Content", "'Post Content' is required.");
                ShowAlertDanger($"Could not create Post.");
                return View("ModifyPost", viewModel);
            }
            try
            {
                viewModel.Post.CreatedAt = viewModel.Post.PublishedAt = DateTime.Now;
                viewModel.Post.CreatedBy = CurrentUserId;
                await _postService.CreatePost(viewModel.Post);
                var post = _postService.GetSectionPostByStub(viewModel.Post.Stub.Trim(),section.Id);
                await _postService.UpdatePostCategories(viewModel.CategoryIds, post.Id);
                ShowAlertSuccess($"Added post '{viewModel.Post.Title}'");
                return RedirectToAction(nameof(SectionController.PostDetails), new { sectionStub = section.Stub, postStub = post.Stub.Trim() });
            }
            catch
            {
                ShowAlertDanger("Could not create post.");
                return View("PostDetails", viewModel);
            }
        }

        [Route("[action]")]
        [HttpPost]
        [SaveModelState]
        public async Task<IActionResult> UpdatePost(SectionViewModel viewModel)
        {
            var section = await _sectionService.GetByIdAsync(viewModel.Post.SectionId);
            viewModel.Section = section;
            if (string.IsNullOrEmpty(viewModel.Post.Stub))
            {
                ModelState.AddModelError("Post.Stub", "A 'Post Stub' is required.");
                ShowAlertDanger($"Could not update Post.");
                return View("ModifyPost", viewModel);
            }
            if (string.IsNullOrEmpty(viewModel.Post.Title))
            {
                ModelState.AddModelError("Post.Title", "A 'Post Title' is required.");
                ShowAlertDanger($"Could not update Post.");
                return View("ModifyPost", viewModel);
            }
            if (string.IsNullOrEmpty(viewModel.Post.Content))
            {
                ModelState.AddModelError("Post.Content", "'Post Content' is required.");
                ShowAlertDanger($"Could not update Post.");
                return View("ModifyPost", viewModel);
            }
            try {
                await _postService.UpdatePost(viewModel.Post);
                await _postService.UpdatePostCategories(viewModel.CategoryIds, viewModel.Post.Id);
                ShowAlertSuccess($"Updated post '{viewModel.Post.Title}'");
                return RedirectToAction(nameof(SectionController.PostDetails),
                    new { sectionStub = section.Stub, postStub = viewModel.Post.Stub });
            }
            catch
            {
                ShowAlertDanger($"Could not create Post.");
                return View("ModifyPost", viewModel);
            }
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> DeletePost(int postId, string sectionStub)
        {
            var post = await _postService.GetPostById(postId);
            try
            {
                await _postService.RemovePost(post);
                ShowAlertSuccess($"Deleted post '{post.Title}'");
            }
            catch
            {
                ShowAlertDanger($"Failed to Delete '{post.Title}'");
            }
            return RedirectToAction(nameof(SectionController.Post),
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
            var fileLibs = await _fileService.GetFileLibrariesBySection(section.Id);
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
            var viewModel = new SectionViewModel()
            {
                Section = section,
                FileLibrary = fileLib,
                Files = files.Data,
                FileTypes = await _fileService.GetAllFileTypesAsync(),
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
            var linkLibs = await _linkService.GetLinkLibrariesBySection(section.Id);
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
            var viewModel = new SectionViewModel()
            {
                Section = section,
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
            if (!Directory.Exists(filePath))
            {
                ShowAlertDanger($"'sections' directory not found at: { filePath}");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = library.Stub });
            }
            filePath = Path.Combine(filePath, section.Stub);
            if (!Directory.Exists(filePath))
            {
                ShowAlertDanger($"'{section.Stub}' directory not found at: { filePath}");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = library.Stub });
            }
            filePath = Path.Combine(filePath, library.Stub);
            if (!Directory.Exists(filePath))
            {
                ShowAlertDanger($"'{library.Stub}' directory not found at: { filePath}");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = library.Stub });
            }
            filePath = Path.Combine(filePath, file.Name + type.Extension);
            if (!System.IO.File.Exists(filePath))
            {
                ShowAlertDanger($"'{file.Name}' file not found at: { filePath}");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = library.Stub });
            }
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
            return File(fs,mimeType,file.Name+type.Extension);
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> AddFileLibrary(SectionViewModel viewModel)
        {
            var section = _sectionService.GetSectionByStub(viewModel.Section.Stub);
            if (section == null)
            {
                ShowAlertDanger("Could not find section.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            if (string.IsNullOrEmpty(viewModel.FileLibrary.Name))
            {
                ModelState.AddModelError("FileLibrary.Name", "A 'File Library Name' is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub});
            }
            if (string.IsNullOrEmpty(viewModel.FileLibrary.Stub))
            {
                ShowAlertDanger("Invalid name for File Library.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            viewModel.FileLibrary.SectionId = section.Id;
            await _fileService.CreateLibraryAsync(CurrentUserId,
                viewModel.FileLibrary, section.Id);
            var fileLibs = await _fileService.GetFileLibrariesBySection(section.Id);
            var fileLib = fileLibs.Find(_ => _.Stub == viewModel.FileLibrary.Stub.Trim());
            var fileTypes = _fileService.GetAllFileTypeIds();
            await _fileService.EditLibraryTypesAsync(fileLib, fileTypes);
            var filePath = Path.Combine(
                Directory.GetParent(_hostingEnvironment.WebRootPath).FullName, "shared");
            filePath = Path.Combine(filePath, "sections");
            if (!Directory.Exists(filePath))
            {
                ShowAlertDanger($"'sections' directory not found at: { filePath}");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            filePath = Path.Combine(filePath, section.Stub);
            if (!Directory.Exists(filePath))
            {
                ShowAlertDanger($"'{section.Stub}' directory not found at: { filePath}");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
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
                new { sectionStub = section.Stub});
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> UpdateFileLibrary(SectionViewModel viewModel)
        {
            var section = _sectionService.GetSectionByStub(viewModel.Section.Stub);
            if (section == null)
            {
                ShowAlertDanger("Invalid name for File Library.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
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
            var oldLib = await _fileService.GetLibraryByIdAsync(viewModel.FileLibrary.Id);
            var filePath = Path.Combine(
                Directory.GetParent(_hostingEnvironment.WebRootPath).FullName, "shared");
            filePath = Path.Combine(filePath, "sections");
            if (!Directory.Exists(filePath))
            {
                _logger.LogError($"'sections' directory not found at: { filePath}");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            filePath = Path.Combine(filePath, section.Stub);
            if (!Directory.Exists(filePath))
            {
                _logger.LogError($"'{section.Stub}' directory not found at: { filePath}");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            var newPath = Path.Combine(filePath, viewModel.FileLibrary.Stub);
            var oldPath = Path.Combine(filePath, oldLib.Stub);
            if (Directory.Exists(newPath))
            {
                ShowAlertDanger($"The File Library '{viewModel.FileLibrary.Name}' already exists for this section.");
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

                Directory.Delete(oldPath,true);
                oldLib.Name = viewModel.FileLibrary.Name;
                oldLib.Stub = viewModel.FileLibrary.Stub;
                var types = new List<int>();
                await _fileService.UpdateLibrary(oldLib);
                await _fileService.EditLibraryTypesAsync(oldLib, types);
            }
            ShowAlertSuccess($"Added '{viewModel.FileLibrary.Name}' to '{section.Name}'s File Library'");
            return RedirectToAction(nameof(SectionController.Section),
                new { sectionStub = section.Stub });
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> DeleteFileLibrary(SectionViewModel model)
        {
            var section = _sectionService.GetSectionByStub(model.Section.Stub);
            if (model.FileLibrary.Id == 0)
            {
                ShowAlertDanger("A 'File Library Id' is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }

            var fileLib = await _fileService.GetLibraryByIdAsync(model.FileLibrary.Id);
            if (fileLib == null)
            {
                ShowAlertDanger("Could not find file library");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }

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
            if (!Directory.Exists(filePath))
            {
                _logger.LogError($"'sections' directory not found at: { filePath}");
                ShowAlertDanger($"Failed to delete File Library '{fileLib.Name}'");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            filePath = Path.Combine(filePath, section.Stub);
            if (!Directory.Exists(filePath))
            {
                _logger.LogError($"'{section.Stub}' directory not found at: { filePath}");
                ShowAlertDanger($"Failed to delete File Library '{fileLib.Name}'");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            filePath = Path.Combine(filePath, fileLib.Stub);
            if (!Directory.Exists(filePath))
            {
                _logger.LogError($"The File Library '{fileLib.Stub}' does not exist for this section.");
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

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> DeleteFileFromLibrary(SectionViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.Section.Stub))
            {
                ShowAlertDanger("Could not find the section.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            var section = _sectionService.GetSectionByStub(viewModel.Section.Stub);
            if (viewModel.FileLibrary.Id == 0)
            {
                ShowAlertDanger($"Failed to find File Library");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            var fileLib = await _fileService.GetLibraryByIdAsync(viewModel.FileLibrary.Id);
            if (viewModel.File.Id == 0)
            {
                ShowAlertDanger($"Failed to find File");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub });
            }
            var file = await _fileService.GetByIdAsync(viewModel.File.Id);
            var filePath = Path.Combine(
                Directory.GetParent(_hostingEnvironment.WebRootPath).FullName, "shared");
            filePath = Path.Combine(filePath, "sections");
            var type = await _fileService.GetFileTypeByIdAsync(file.FileTypeId);
            if (!Directory.Exists(filePath))
            {
                _logger.LogError($"'sections' directory not found at: { filePath}");
                ShowAlertDanger($"Failed to delete File Library '{file.Name}'");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
            }
            filePath = Path.Combine(filePath, section.Stub);
            if (!Directory.Exists(filePath))
            {
                _logger.LogError($"'{section.Stub}' directory not found at: { filePath}");
                ShowAlertDanger($"Failed to delete File Library '{file.Name}'");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
            }
            filePath = Path.Combine(filePath, fileLib.Stub);
            if (!Directory.Exists(filePath))
            {
                _logger.LogError($"'{fileLib.Stub}' directory not found at: { filePath}");
                ShowAlertDanger($"Failed to delete File Library '{file.Name}'");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
            }
            filePath = Path.Combine(filePath, file.Name+type.Extension);
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogError($"'{file.Name}' file not found at: { filePath}");
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

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> AddFileToLibrary(SectionViewModel model)
        {
            var section = _sectionService.GetSectionByStub(model.Section.Stub);
            if (section == null)
            {
                ShowAlertDanger("Could not find the section.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            var fileLib = await _fileService.GetLibraryByIdAsync(model.FileLibrary.Id);
            if (fileLib == null)
            {
                ShowAlertDanger("Could not find the File Library");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = section.Stub});
            }
            if (string.IsNullOrEmpty(model.File.Name))
            {
                ModelState.AddModelError("File.Name", "A 'File Name' is required.");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
            }
            if ((model.UploadFile) == null)
            {
                ModelState.AddModelError("UploadFile", "An 'Uploaded File' is required.");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
            }
            model.File.FileLibraryId = fileLib.Id;
            var extension = Path.GetExtension(model.UploadFile.FileName).ToLower();
            var libraryTypes = await _fileService.GetFileLibrariesFileTypes(fileLib.Id);
            if (libraryTypes == null)
            {
                ShowAlertDanger("This library doesn't have any file types.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            var fileType = libraryTypes
                .ToList()
                .FirstOrDefault(_ => _.Extension.ToLower() == extension);
            if (fileType == null)
            {
                _logger.LogError($"'{fileLib.Name}' does not allow '{extension}' files to be uploaded");
                ShowAlertDanger($"Failed to delete File Library '{model.File.Name}'");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
            }
            var filePath = Path.Combine(
                Directory.GetParent(_hostingEnvironment.WebRootPath).FullName, "shared");
            filePath = Path.Combine(filePath, "sections");
            if (!Directory.Exists(filePath))
            {
                _logger.LogError($"'sections' directory not found at: { filePath}");
                ShowAlertDanger($"Failed to delete File Library '{model.File.Name}'");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
            }
            filePath = Path.Combine(filePath, section.Stub);
            if (!Directory.Exists(filePath))
            {
                _logger.LogError($"'{section.Stub}' directory not found at '{ filePath}'");
                ShowAlertDanger($"Failed to delete File Library '{model.File.Name}'");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
            }
            filePath = Path.Combine(filePath, fileLib.Stub);
            if (!Directory.Exists(filePath))
            {
                _logger.LogError($"'{fileLib.Stub}' directory not found at '{filePath}'");
                ShowAlertDanger($"Failed to delete File Library '{model.File.Name}'");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
            }
            filePath = Path.Combine(filePath, model.File.Name + extension);
            if (System.IO.File.Exists(filePath))
            {
                ShowAlertDanger($"File name '{model.File.Name}' already exists.");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = section.Stub, fileLibStub = fileLib.Stub });
            }
            if (model.UploadFile.Length > 0)
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.UploadFile.CopyToAsync(fileStream);
                }
            }
            await _fileService.CreatePrivateFileAsync(CurrentUserId, model.File, model.UploadFile);
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
                    new { sectionStub = model.Section.Stub});
            }
            if (string.IsNullOrEmpty(model.LinkLibrary.Stub))
            {
                ShowAlertDanger("Invalid name for File Library.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub = model.Section.Stub});
            }
            var section = _sectionService.GetSectionByStub(model.Section.Stub);
            model.LinkLibrary.SectionId = section.Id;
            var linkLib = await _linkService.CreateLibraryAsync(CurrentUserId, model.LinkLibrary,section.Id);
            ShowAlertSuccess($"Added Link Library '{linkLib.Name}' to '{section.Name}'");
            return RedirectToAction(nameof(SectionController.Section),
                new { sectionStub = model.Section.Stub});
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> UpdateLinkLibrary(SectionViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.Section.Stub))
            {
                ModelState.AddModelError("Post.Content", "'Post Content' is required.");
                ShowAlertDanger($"Could not update link library.");
                return View("AddPost", viewModel);
            }
            var section = _sectionService.GetSectionByStub(viewModel.Section.Stub);
            viewModel.Section = section;
            if (string.IsNullOrEmpty(viewModel.LinkLibrary.Stub))
            {
                ModelState.AddModelError("Post.Stub", "A 'Post Stub' is required.");
                ShowAlertDanger($"Could not update link library.");
                return View("AddPost", viewModel);
            }
            if (string.IsNullOrEmpty(viewModel.LinkLibrary.Name))
            {
                ModelState.AddModelError("Post.Title", "A 'Post Title' is required.");
                ShowAlertDanger($"Could not update link library.");
                return View("AddPost", viewModel);
            }
            var oldLib = await _linkService.GetLibraryByIdAsync(viewModel.LinkLibrary.Id);
            oldLib.Name = viewModel.LinkLibrary.Name;
            oldLib.Stub = viewModel.LinkLibrary.Stub;
            await _linkService.UpdateLibraryAsync(oldLib);
            ShowAlertSuccess($"Updated link library '{oldLib.Name}'");
            return RedirectToAction(nameof(SectionController.Section),
                new { sectionStub = section.Stub });
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> AddLinkToLibrary(SectionViewModel model)
        {
            if (string.IsNullOrEmpty(model.Link.Name))
            {
                ModelState.AddModelError("Link.Name", "A 'Link Name' is required.");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = model.Section.Stub, fileLibId = model.LinkLibrary.Stub });
            }
            if (string.IsNullOrEmpty(model.Link.Url))
            {
                ModelState.AddModelError("Link.Url", "A 'Link Url' is required.");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = model.Section.Stub, fileLibId = model.LinkLibrary.Stub });
            }
            if (string.IsNullOrEmpty(model.Link.Icon))
            {
                ModelState.AddModelError("Link.Icon", "A 'Link Icon' is required.");
                return RedirectToAction(nameof(SectionController.FileLibrary),
                    new { sectionStub = model.Section.Stub, fileLibId = model.LinkLibrary.Stub });
            }
            model.Link.LinkLibraryId = model.LinkLibrary.Id;
            var link = await _linkService.CreateAsync(CurrentUserId, model.Link);
            var linkLib = await _linkService.GetLibraryByIdAsync(model.LinkLibrary.Id);

            ShowAlertSuccess($"Added '{link.Name}' to '{linkLib.Name}'");
            return RedirectToAction(nameof(SectionController.LinkLibrary),
                new { sectionStub = model.Section.Stub, linkLibStub = linkLib.Stub });
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> DeleteLinkFromLibrary(SectionViewModel model)
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
                new { sectionStub = model.Section.Stub, linkLibStub = model.LinkLibrary.Stub });
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> UpdateLinkFromLibrary(SectionViewModel model)
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
                new { sectionStub = model.Section.Stub, linkLibStub = model.LinkLibrary.Stub });
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<RedirectToActionResult> DeleteLinkLibrary(SectionViewModel model)
        {
            var section = _sectionService.GetSectionByStub(model.Section.Stub);

            if (model.LinkLibrary.Id == 0)
            {
                ShowAlertDanger("A 'Link Library Id' is required.");
                return RedirectToAction(nameof(SectionController.Section),
                    new { sectionStub =  section.Stub });
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

        [Route("{sectionStub}/[action]")]
        public async Task<Section> ValidateSectionAccess(string sectionStub)
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
