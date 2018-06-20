using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Posts;
using Ocuda.Utility.Models;
using Ops.Service;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    public class PostsController : BaseController
    {
        private readonly PostService _postService;

        public PostsController(PostService postService)
        {
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var postList = await _postService.GetPostsAsync();

            foreach (var post in postList)
            {
                post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
            }

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

            var viewModel = new IndexViewModel()
            {
                PaginateModel = paginateModel,
                Posts = postList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                        .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            var viewModel = new DetailViewModel()
            {
                Action = nameof(Create)
            };

            return View("Detail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.Post.SectionId = 1; //TODO: Use actual SectionId
                    var newPost = await _postService.CreatePostAsync(model.Post);
                    ShowAlertSuccess($"Added blog post: {newPost.Title}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to add blog post: ", ex.Message);
                }
            }

            model.Action = nameof(Create);
            return View("Detail", model);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var viewModel = new DetailViewModel()
            {
                Action = nameof(Edit),
                Post = await _postService.GetPostByIdAsync(id)
            };

            return View("Detail", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.Post.SectionId = 1; //TODO: Use actual SectionId
                    var post = await _postService.EditPostAsync(model.Post);
                    ShowAlertSuccess($"Updated blog post: {post.Title}");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ShowAlertDanger("Unable to update blog post: ", ex.Message);
                }
            }

            model.Action = nameof(Edit);
            return View("Detail", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(IndexViewModel model)
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

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }
    }
}
