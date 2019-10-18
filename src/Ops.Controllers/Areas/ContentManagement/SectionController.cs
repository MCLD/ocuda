using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area("ContentManagement")]
    //[Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[controller]")]
    public class SectionController : BaseController<SectionController>
    {
        private readonly ISectionService _sectionService;
        private readonly IPostService _postService;

        public static string Name { get { return "Section"; } }

        public SectionController(ServiceFacades.Controller<SectionController> context,
            ISectionService sectionService,
            IPostService postService) : base(context)
        {
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
            _postService = postService
                ?? throw new ArgumentNullException(nameof(postService));
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index()
        {
            var sectionNames = UserClaims(ClaimType.SectionManager);
            var viewModel = new SectionViewModel()
            {
                UserSections = await _sectionService.GetSectionsByNamesAsync(sectionNames)
            };
            return View(viewModel);
        }

        [Route("{sectionStub}")]
        [Route("{sectionStub}/{page}")]
        public async Task<IActionResult> Section(string sectionStub, int page=1)
        {
            var sectionNames = UserClaims(ClaimType.SectionManager);
            var section = (await _sectionService.GetSectionsByNamesAsync(sectionNames))
                .Find(_ =>_.Stub == sectionStub);
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

                var categories = await _postService.GetPostCategoriesBySectionIdAsync(section.Id);
                var viewModel = new SectionViewModel()
                {
                    Section = section,
                    SectionCategories = categories
                };
                var posts = await _postService.GetPaginatedSectionPostsAsync(filter,section.Id);
                paginateModel.ItemCount = posts.Count;
                viewModel.PaginateModel = paginateModel;
                viewModel.AllPosts = posts.Data;
                return View(viewModel);
            }
        }

        [Route("{sectionStub}/[action]/{postId}")]
        public async Task<IActionResult> Post(string sectionStub, int postId)
        {
            var sectionNames = UserClaims(ClaimType.SectionManager);
            var sections = await _sectionService.GetSectionsByNamesAsync(sectionNames);
            var section = sections.Find(_ => _.Stub == sectionStub);
            if (section == null)
            {
                ShowAlertDanger($"Could not find section {sectionStub}.");
                return RedirectToAction(nameof(SectionController.Index));
            }
            try
            {
                var post = await _postService.GetPostById(postId);
                var viewModel = new SectionViewModel()
                {
                    Section = section,
                    SectionCategories = await _postService.GetPostCategoriesBySectionIdAsync(section.Id),
                    SectionsPosts = await _postService.GetTopSectionPostsAsync(5, section.Id),
                    Post = post,
                    Category = await _postService.GetPostCategoryByIdAsync(post.PostCategoryId)
                };
                return View(viewModel);
            }
            catch
            {
                ShowAlertDanger($"The Post Id {postId} does not exist.");
                return RedirectToAction(nameof(SectionController.Section), new { sectionStub });
            }
        }

        [Route("{sectionStub}/[action]/{page}")]
        [Route("{sectionStub}/[action]/{categoryStub}/{page}")]
        public async Task<IActionResult> PostCategory(string sectionStub, string categoryStub, int page = 1)
        {
            var sectionNames = UserClaims(ClaimType.SectionManager);
            var sections = await _sectionService.GetSectionsByNamesAsync(sectionNames);
            var section = sections.Find(_ => _.Stub == sectionStub);
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
            var viewModel = new SectionViewModel()
            {
                Section = section,
                SectionCategories = await _postService.GetPostCategoriesBySectionIdAsync(section.Id)
            };
            if (string.IsNullOrEmpty(categoryStub))
            {
                var categories = await _postService.GetPaginatedPostCategoryListAsync(filter,section.Id);
                paginateModel.ItemCount = categories.Count;
                viewModel.PaginateModel = paginateModel;
                viewModel.AllPostCategories = categories.Data;
                return View(viewModel);
            }
            else
            {
                try
                {
                    var category = _postService.GetPostCategoryByStub(categoryStub);
                    var posts = await _postService.GetPaginatedPostListAsync(filter,category.Id);
                    paginateModel.ItemCount = posts.Count;
                    viewModel.PaginateModel = paginateModel;
                    viewModel.AllPostCategoryPosts = posts.Data;
                    viewModel.Category = category;
                    return View(viewModel);
                }
                catch
                {
                    ShowAlertDanger($"The Post Category {categoryStub} does not exist.");
                    return RedirectToAction(nameof(SectionController.Section), new { sectionStub });
                }
            }
        }

        [Route("{sectionStub}/[action]/{fileId}")]
        public IActionResult Files(string sectionStub, int fileId)
        {
            return View();
        }
    }
}
