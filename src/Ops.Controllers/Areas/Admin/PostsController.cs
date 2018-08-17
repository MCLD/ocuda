﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Posts;
using Ocuda.Ops.Controllers.Authorization;
using Ocuda.Ops.Controllers.Filter;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

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

        public async Task<IActionResult> Index(string section, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BlogFilter(page, itemsPerPage)
            {
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

            foreach(var post in postList.Data)
            {
                var userInfo = await _userService.GetUserInfoById(post.CreatedBy);
                post.CreatedByName = userInfo.Item1;
                post.CreatedByUsername = userInfo.Item2;
            }

            var viewModel = new IndexViewModel()
            {
                PaginateModel = paginateModel,
                Posts = postList.Data,
                SectionId = currentSection.Id
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string title, string stub, int sectionId)
        {
            var post = new Post
            {
                IsDraft = true,
                SectionId = sectionId,
                Stub = stub,
                Title = title
            };

            try
            {
                var newPost = await _postService.CreateAsync(CurrentUserId, post);
                return Json(new { success = true, id = newPost.Id });
            }
            catch (OcudaException ex)
            {
                _logger.LogError($"Error adding post: {ex}", ex);
                ShowAlertDanger("Unable to add blog post: ", ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }


        [RestoreModelState]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _postService.GetByIdAsync(id);
            var attachments = await _fileService.GetByPostIdAsync(id);

            var viewModel = new DetailViewModel()
            {
                Action = nameof(Edit),
                Post = post,
                SectionId = post.SectionId,
                IsDraft = post.IsDraft,
                Attachments = attachments
            };

            return View("Detail", viewModel);
        }

        [HttpPost]
        [SaveModelState]
        public async Task<IActionResult> Edit(DetailViewModel model)
        {
            var currentPost = await _postService.GetByIdAsync(model.Post.Id);

            if (currentPost.IsDraft == true && model.Post.IsDraft == false)
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
                    var post = await _postService.EditAsync(model.Post);
                    ShowAlertSuccess($"Updated blog post: {post.Title}");
                    return RedirectToAction(nameof(Index));
                }
                catch (OcudaException ex)
                {
                    _logger.LogError($"Error editing post: {ex}", ex);
                    ShowAlertDanger("Unable to update blog post: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Edit));
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
    }
}
