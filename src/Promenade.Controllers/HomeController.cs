using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("{culture:cultureConstraint?}")]
    public class HomeController : BasePageController<HomeController>
    {
        protected override PageType PageType
        { get { return PageType.Home; } }

        public static string Name { get { return "Home"; } }

        public HomeController(ServiceFacades.Controller<HomeController> context,
            PageService pageService,
            RedirectService redirectService,
            SocialCardService socialCardService)
            : base(context, pageService, redirectService, socialCardService)
        {
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            return await ReturnPageAsync(nameof(Index));
        }
    }
}
