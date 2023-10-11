using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class NewsController : GeneralBasePageController<NewsController>
    {
        public NewsController(ServiceFacades.Controller<NewsController> context,
            ServiceFacades.PageController pageContext)
            : base(context, pageContext)
        {
        }

        protected override PageType PageType
        { get { return PageType.News; } }
    }
}