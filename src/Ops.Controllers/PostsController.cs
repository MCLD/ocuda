using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Posts;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers
{
    public class PostsController : BaseController<PagesController>
    {
        private readonly IPostService _postService;
        private readonly ISectionService _sectionService;
        private readonly IUserService _userService;

        public PostsController(ServiceFacades.Controller<PagesController> context,
            IPostService postService, 
            ISectionService sectionService,
            IUserService userService) : base(context)
        {
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
            _sectionService = sectionService 
                ?? throw new ArgumentNullException(nameof(sectionService));
            _userService = userService
                ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<IActionResult> Index(string section, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BlogFilter(page, itemsPerPage)
            {
                SectionId = currentSection.Id,
                IsPublished = true
            };

            var postList = await _postService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel()
            {
                ItemCount = postList.Count,
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

            foreach (var post in postList.Data)
            {
                var userInfo = await _userService.GetUserInfoById(post.CreatedBy);
                post.CreatedByName = userInfo.Item1;
                post.CreatedByUsername = userInfo.Item2;
                post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
            }

            var viewModel = new IndexViewModel()
            {
                PaginateModel = paginateModel,
                Posts = postList.Data
            };

            return View(viewModel);
        }

        public new async Task<IActionResult> Display(string section, int categoryId, string id)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var currentCategory = await _postService.GetCategoryByIdAsync(categoryId);

            if (currentCategory?.SectionId != currentSection.Id)
            {
                return RedirectToAction(nameof(HomeController.Index));
            }

            var post = await _postService.GetByStubAndCategoryIdAsync(id, currentSection.Id);

            if (post != null)
            {
                var userInfo = await _userService.GetUserInfoById(post.CreatedBy);
                post.CreatedByName = userInfo.Item1;
                post.CreatedByUsername = userInfo.Item2;
                post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
                return View(post);
            }
            else
            {
                ShowAlertDanger($"Could not find post '{id}' in '{currentSection.Name}'.");
                return RedirectToAction(nameof(PostsController.Index));
            }
        }
    }
}
