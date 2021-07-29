using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Home;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers
{
    [Route("")]
    public class HomeController : BaseController<HomeController>
    {
        private readonly IPostService _postService;
        private readonly ISectionService _sectionService;
        private readonly IUserService _userService;

        public HomeController(ServiceFacades.Controller<HomeController> context,
            IPostService postService,
            ISectionService sectionService,
            IUserService userService) : base(context)
        {
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public static string Name { get { return "Home"; } }

        [HttpGet("[action]")]
        public IActionResult Authenticate(Uri returnUrl)
        {
            // by the time we get here the user is probably authenticated - if so we can take them
            // back to their initial destination
            if (HttpContext.Items[ItemKey.Nickname] != null)
            {
                return Redirect(returnUrl?.ToString() ?? nameof(Index));
            }

            TempData[TempDataKey.AlertWarning]
                = $"Could not authenticate you for access to {returnUrl}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page)
        {
            var showPage = page == default ? 1 : page;
            return await ShowPostsAsync(new BlogFilter(showPage, 5)
            {
                IsShownOnHomePage = true
            }, showPage);
        }

        [HttpGet("{stub}")]
        public async Task<IActionResult> SectionIndex(string stub, int page)
        {
            var section = await _sectionService.GetSectionByStubAsync(stub);
            if (section == null)
            {
                return NotFound();
            }

            if (section.SupervisorsOnly)
            {
                var reports = await _userService.GetDirectReportsAsync(CurrentUserId);
                if (reports?.Count == 0)
                {
                    return RedirectToUnauthorized();
                }
            }

            var showPage = page == default ? 1 : page;
            return await ShowPostsAsync(new BlogFilter(showPage, 5)
            {
                SectionId = section.Id
            }, showPage);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Unauthorized(Uri returnUrl)
        {
            var adminEmail = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Email.AdminAddress);

            string mailLink = null;
            if (!string.IsNullOrEmpty(adminEmail) && returnUrl != null)
            {
                mailLink = $"mailto:{adminEmail}?subject="
                    + Uri.EscapeUriString("Requesting intranet access")
                    + "&body="
                    + Uri.EscapeUriString($"I ({CurrentUsername}) request access to: {returnUrl}");
            }

            return View(new UnauthorizedViewModel
            {
                AdminEmail = mailLink,
                ReturnUrl = returnUrl?.ToString(),
                Username = CurrentUsername
            });
        }

        [HttpGet("[action]")]
        public IActionResult Whoami()
        {
            return Json(new UserInformation
            {
                Username = UserClaim(ClaimType.Username),
                Authenticated = !string.IsNullOrEmpty(UserClaim(ClaimType.Username)),
                AuthenticatedAt = UserClaim(ClaimType.AuthenticatedAt) != null
                    ? DateTime.Parse(UserClaim(ClaimType.AuthenticatedAt), CultureInfo.InvariantCulture)
                    : null
            });
        }

        private async Task<IActionResult> ShowPostsAsync(BlogFilter filter, int page)
        {
            var posts = await _postService.GetPaginatedPostsAsync(filter);

            var viewModel = new IndexViewModel
            {
                Posts = posts.Data,
                ItemCount = posts.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            foreach (var post in viewModel.Posts)
            {
                post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
            }

            return View("Index", viewModel);
        }
    }
}