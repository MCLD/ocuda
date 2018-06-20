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

            foreach(var post in postList)
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
    }
}
