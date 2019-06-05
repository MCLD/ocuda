using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Posts;
using Ocuda.Ops.Controllers.Authorization;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;
using Ocuda.Ops.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(SectionManagerRequirement))]
    public class PostsController : BaseController<PostsController>
    {
        private readonly IFileService _fileService;
        private readonly IPostService _postService;
        private readonly ISectionService _sectionService;
        private readonly IUserService _userService;

        public PostsController(ServiceFacades.Controller<PostsController> context,
            IFileService fileService,
            IPostService postService,
            ISectionService sectionService,
            IUserService userService) : base(context)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
            _userService = userService
                ?? throw new ArgumentNullException(nameof(userService));
        }

        #region Posts
        public async Task<IActionResult> Index(string section, int? category, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            PostCategory currentCategory = null;
            if (category.HasValue)
            {
                currentCategory = await _postService.GetCategoryByIdAsync(category.Value);

                if (currentCategory?.SectionId != currentSection.Id)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BlogFilter(page, itemsPerPage)
            {
                PostCategoryId = category,
                SectionId = currentSection.Id
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
            }

            var viewModel = new IndexViewModel()
            {
                PaginateModel = paginateModel,
                Posts = postList.Data,
                SectionId = currentSection.Id,
                CategoryId = category,
                Category = currentCategory,
                CategoryList = new SelectList(
                    await _postService.GetCategoriesBySectionIdAsync(currentSection.Id),
                    nameof(PostCategory.Id),
                    nameof(PostCategory.Name)
                )
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(IndexViewModel model)
        {
            JsonResponse response;

            model.Post.PublishedAt = null;

            try
            {
                var newPost = await _postService.CreateAsync(CurrentUserId, model.Post);
                response = new JsonResponse
                {
                    Success = true,
                    EntityId = newPost.Id
                };
            }
            catch (OcudaException ex)
            {
                response = new JsonResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }

            return Json(response);
        }


        [RestoreModelState]
        public async Task<IActionResult> Detail(string section, int id)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var post = await _postService.GetByIdAsync(id);
            
            if (post?.PostCategory.SectionId != currentSection.Id)
            {
                return RedirectToAction(nameof(Index));
            }

            var attachments = await _fileService.GetByPostIdAsync(id);

            var viewModel = new DetailViewModel()
            {
                Post = post,
                SectionId = currentSection.Id,
                DefaultSection = currentSection.IsDefault,
                Attachments = attachments
            };

            return View(viewModel);
        }

        [HttpPost]
        [SaveModelState]
        public async Task<IActionResult> Detail(DetailViewModel model)
        {
            var currentPost = await _postService.GetByIdAsync(model.Post.Id);

            if (!currentPost.PublishedAt.HasValue && model.Publish)
            {
                var stubInUse = await _postService.StubInUseAsync(model.Post);

                if (stubInUse)
                {
                    ModelState.AddModelError("Post_IsDraft", string.Empty);
                    ShowAlertDanger("The chosen stub is already in use. Please choose a different stub.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var post = await _postService.EditAsync(model.Post, model.Publish);
                    ShowAlertSuccess($"Updated blog post: {post.Title}");
                    return RedirectToAction(nameof(Index));
                }
                catch (OcudaException ex)
                {
                    _logger.LogError($"Error editing post: {ex}", ex);
                    ShowAlertDanger("Unable to update blog post: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Detail));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(IndexViewModel model)
        {
            try
            {
                await _postService.DeleteAsync(model.Post.Id);
                ShowAlertSuccess("Post deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting post: {ex}", ex);
                ShowAlertDanger("Unable to delete blog post: ", ex.Message);
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }

        [HttpPost]
        public async Task<JsonResult> StubInUse(Post item)
        {
            return Json(await _postService.StubInUseAsync(item));
        }
        #endregion

        #region Categories
        public async Task<IActionResult> Categories(string section, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BlogFilter(page, itemsPerPage)
            {
                SectionId = currentSection.Id
            };

            var categoryList = await _postService.GetPaginatedCategoryList(filter);

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

            var viewModel = new CategoriesViewModel
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
            JsonResponse response;

            try
            {
                var category = await _postService.CreateCategoryAsync(CurrentUserId, model.Category);
                ShowAlertSuccess($"Added post category: {category.Name}");
                response = new JsonResponse
                {
                    Success = true,
                };
            }
            catch (OcudaException ex)
            {
                response = new JsonResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }

            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(CategoriesViewModel model)
        {
            JsonResponse response;

            try
            {
                var category = await _postService.EditCategoryAsync(model.Category);
                ShowAlertSuccess($"Updated post category: {category.Name}");
                response = new JsonResponse
                {
                    Success = true,
                };
            }
            catch (OcudaException ex)
            {
                response = new JsonResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }

            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(CategoriesViewModel model)
        {
            try
            {
                await _postService.DeleteCategoryAsync(model.Category.Id);
                ShowAlertSuccess($"Delete post category: {model.Category.Name}.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting post category: {ex}", ex);
                ShowAlertDanger("Unable to post category: ", ex.Message);
            }

            return RedirectToAction(nameof(Categories), new { page = model.PaginateModel.CurrentPage });
        }
        #endregion
    }
}
