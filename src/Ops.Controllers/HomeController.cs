using System;
using Ocuda.Ops.Controllers.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public async Task<IActionResult> Index()
        {
            var postList = await _postService.GetPostsAsync();

            foreach (var post in postList)
            {
                post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
            }

            var viewModel = new IndexViewModel
            {
                Files = await _fileService.GetFilesAsync(),
                Links = await _linkService.GetLinksAsync(),
                Posts = postList,
                Calendars = _sectionService.GetCalendars()
            };

            return View(viewModel);
        }
    }
}
