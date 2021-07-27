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
        private readonly IUserService _userService;

        public HomeController(ServiceFacades.Controller<HomeController> context,
            IPostService postService,
            IUserService userService) : base(context)
        {
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
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

            var filter = new BlogFilter(showPage, 5)
            {
                IsShownOnHomePage = true
            };

            var posts = await _postService.GetPaginatedPostsAsync(filter);

            var viewModel = new IndexViewModel
            {
                Posts = posts.Data,
                ItemCount = posts.Count,
                CurrentPage = showPage,
                ItemsPerPage = filter.Take.Value
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            foreach (var post in viewModel.Posts)
            {
                var user = await _userService.GetByIdAsync(post.CreatedBy);
                post.CreatedByName = user.Name;
                post.CreatedByUser = user;
                post.Content = CommonMark.CommonMarkConverter.Convert(post.Content);
            }

            return View(viewModel);
        }

        [HttpGet("[action]")]
        public IActionResult Unauthorized(Uri returnUrl)
        {
            return View(new UnauthorizedViewModel
            {
                ReturnUrl = returnUrl?.ToString() ?? "",
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
    }
}