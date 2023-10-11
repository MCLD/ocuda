using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class AboutController : GeneralBasePageController<AboutController>
    {
        public AboutController(ServiceFacades.Controller<AboutController> context,
            ServiceFacades.PageController pageContext)
            : base(context, pageContext)
        {
        }

        protected override PageType PageType
        { get { return PageType.About; } }
    }
}