﻿using System;
using Ocuda.Ops.Controllers.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Ops.Service;
using System.Threading.Tasks;
using Ocuda.Ops.Controllers.ViewModels.Home;

namespace Ocuda.Ops.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SectionService _sectionService;
        private readonly FileService _fileService;
        private readonly LinkService _linkService;
        private readonly PostService _postService;

        public HomeController(ILogger<HomeController> logger, 
                              SectionService sectionService,
                              FileService fileService,
                              LinkService linkService,
                              PostService postService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
        }

        public IActionResult Index()
        {
            var viewModel = new IndexViewModel
            {
                Files = _fileService.GetFiles(),
                Links = _linkService.GetLinks(),
                Posts = _postService.GetPosts(),
                Calendars = _sectionService.GetCalendars()
            };

            return View(viewModel);
        }
    }
}
