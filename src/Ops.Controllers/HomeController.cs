using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Home;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers
{
    public class HomeController : BaseController<HomeController>
    {
        private readonly IFileService _fileService;
        private readonly ILinkService _linkService;
        private readonly IPostService _postService;
        private readonly ISectionService _sectionService;
        private readonly IUserService _userService;

        public HomeController(ServiceFacades.Controller<HomeController> context,
            IFileService fileService,
            ILinkService linkService,
            IPostService postService,
            ISectionService sectionService,
            IUserService userService) : base(context)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<IActionResult> Index(string section, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);

            var filter = new BlogFilter(page, 5)
            {
                SectionId = currentSection.Id,
                IsHomepage = currentSection.IsDefault,
                IsPublished = true
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
                var userInfo = await _userService.GetUserInfoById(post.CreatedBy);
                post.CreatedByName = userInfo.Item1;
                post.CreatedByUsername = userInfo.Item2;
                post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
            }

            var viewModel = new IndexViewModel
            {
                Files = fileList.Data,
                Links = linkList.Data,
                Posts = postList.Data,
                //Calendars = _sectionService.GetCalendars(), //TODO update calendars
                PaginateModel = paginateModel,
                Section = currentSection
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
