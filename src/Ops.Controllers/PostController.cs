using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Post;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;
using Ops.Service;
using System.Linq;
using System.Threading.Tasks;

namespace Ocuda.Ops.Controllers
{
    public class PostController : BaseController
    {
        private readonly SectionService _sectionService;

        public PostController(SectionService sectionService)
        {
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public IActionResult Index(int page = 1)
        {
            var postList = _sectionService.GetBlogPosts();

            var paginateModel = new PaginateModel()
            {
                ItemCount = postList.Count(),
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
                SectionPosts = postList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                        .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        public IActionResult AdminList(int page = 1)
        {
            var postList = _sectionService.GetBlogPosts();

            var paginateModel = new PaginateModel()
            {
                ItemCount = postList.Count(),
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
                SectionPosts = postList.Skip((page - 1) * paginateModel.ItemsPerPage)
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
                    var newPost = await _sectionService.CreateSectionPostAsync(model.SectionPost);
                    ShowAlertSuccess($"Added blog post: {newPost.Title}");
                    return RedirectToAction(nameof(AdminList));
                }
                catch(Exception ex)
                {
                    ShowAlertDanger("Unable to add blog post: ", ex.Message);
                }
            }

            model.Action = nameof(AdminCreate);
            return View("AdminDetail", model);
        }


        public IActionResult AdminEdit(int id)
        {
            var viewModel = new AdminDetailViewModel()
            {
                Action = nameof(AdminEdit),
                SectionPost = _sectionService.GetSectionPostById(id)
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
                    var post = await _sectionService.EditSectionPostAsync(model.SectionPost);
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
                await _sectionService.DeleteSectionPostAsync(model.SectionPost.Id);
                ShowAlertSuccess("Post deleted successfully.");
            }
            catch(Exception ex)
            {
                ShowAlertDanger("Unable to delete blog post: ", ex.Message);
            }

            return RedirectToAction(nameof(AdminList), new { page = model.PaginateModel.CurrentPage });
        }
    }
}
