using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Posts;
using Ocuda.Ops.Service;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers
{
    public class PostController : BaseController
    {
        private readonly PostService _postService;

        public PostController(PostService postService)
        {
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var postList = await _postService.GetPostsAsync();

            var paginateModel = new PaginateModel()
            {
                ItemCount = await _postService.GetPostCountAsync(),
                CurrentPage = page,
                ItemsPerPage = 2
            };

            if (paginateModel.MaxPage > 0 && paginateModel.CurrentPage > paginateModel.MaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new PostListViewModel()
            {
                PaginateModel = paginateModel,
                Posts = postList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                        .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> AdminList(int page = 1)
        {
            var postList = await _postService.GetPostsAsync();

            var paginateModel = new PaginateModel()
            {
                ItemCount = await _postService.GetPostCountAsync(),
                CurrentPage = page,
                ItemsPerPage = 2
            };

            if (paginateModel.MaxPage > 0 && paginateModel.CurrentPage > paginateModel.MaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new AdminListViewModel()
            {
                PaginateModel = paginateModel,
                Posts = postList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                        .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        public IActionResult AdminCreate()
        {
            var viewModel = new AdminDetailViewModel()
            {
                Action = nameof(AdminCreate)
            };

            return View("AdminDetail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AdminCreate(AdminDetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var newPost = await _postService.CreatePostAsync(model.Post);
                    ShowAlertSuccess($"Added blog post: {newPost.Title}");
                    return RedirectToAction(nameof(AdminList));
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to add blog post: ", ex.Message);
                }
            }

            model.Action = nameof(AdminCreate);
            return View("AdminDetail", model);
        }


        public async Task<IActionResult> AdminEdit(int id)
        {
            var viewModel = new AdminDetailViewModel()
            {
                Action = nameof(AdminEdit),
                Post = await _postService.GetPostByIdAsync(id)
            };

            return View("AdminDetail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AdminEdit(AdminDetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var post = await _postService.EditPostAsync(model.Post);
                    ShowAlertSuccess($"Updated blog post: {post.Title}");
                    return RedirectToAction(nameof(AdminList));
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to update blog post: ", ex.Message);
                }
            }

            model.Action = nameof(AdminEdit);
            return View("AdminDetail", model);
        }

        [HttpPost]
        public async Task<IActionResult> AdminDelete(AdminListViewModel model)
        {
            try
            {
                await _postService.DeletePostAsync(model.Post.Id);
                ShowAlertSuccess("Post deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowAlertDanger("Unable to delete blog post: ", ex.Message);
            }

            return RedirectToAction(nameof(AdminList), new { page = model.PaginateModel.CurrentPage });
        }
    }
}
