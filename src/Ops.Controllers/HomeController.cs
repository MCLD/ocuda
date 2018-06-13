using System;
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

        public HomeController(ILogger<HomeController> logger, SectionService sectionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public IActionResult Index()
        {
            var viewModel = new IndexViewModel
            {
                SectionFiles = _sectionService.GetFiles(),
                SectionLinks = _sectionService.GetLinks(),
                SectionPosts = _sectionService.GetBlogPosts(),
                SectionCalendars = _sectionService.GetCalendars()
            };

            return View(viewModel);
        }
    }
}
