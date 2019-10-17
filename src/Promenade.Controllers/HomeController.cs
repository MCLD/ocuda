using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;

namespace Ocuda.Promenade.Controllers
{
    [Route("{culture:cultureConstraint?}")]
    public class HomeController : BaseController<HomeController>
    {
        public HomeController(ServiceFacades.Controller<HomeController> context) : base(context)
        {
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
