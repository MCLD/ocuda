﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Home;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FileService _fileService;
        private readonly LinkService _linkService;
        private readonly PostService _postService;
        private readonly SectionService _sectionService;

        public HomeController(ILogger<HomeController> logger,
            ServiceFacade.Controller context,
            FileService fileService,
            LinkService linkService,
            PostService postService,
            SectionService sectionService) : base(context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
        }

        public async Task<IActionResult> Index(string section, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);

            var filter = new BlogFilter(page, 5)
            {
                SectionId = currentSection.Id
            };

            var fileList = await _fileService.GetPaginatedListAsync(filter);
            var linkList = await _linkService.GetPaginatedListAsync(filter);
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
                post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
            }

            var viewModel = new IndexViewModel
            {
                Files = fileList.Data,
                Links = linkList.Data,
                Posts = postList.Data,
                Calendars = _sectionService.GetCalendars(), //TODO update calendars
                PaginateModel = paginateModel
            };

            return View(viewModel);
        }

        public IActionResult Unauthorized(string returnUrl)
        {
            return View(new UnauthorizedViewModel
            {
                ReturnUrl = returnUrl,
                Username = CurrentUsername
            });
        }

        public IActionResult Authenticate(string returnUrl)
        {
            // by the time we get here the user is probably authenticated - if so we can take them
            // back to their initial destination
            if (HttpContext.Items[ItemKey.Nickname] != null)
            {
                return Redirect(returnUrl);
            }

            TempData[TempDataKey.AlertWarning]
                = $"Could not authenticate you for access to {returnUrl}.";
            return RedirectToAction("Index");
        }
    }
}
