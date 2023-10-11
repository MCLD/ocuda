using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class ThanksController : GeneralBasePageController<ThanksController>
    {
        public ThanksController(ServiceFacades.Controller<ThanksController> context,
            ServiceFacades.PageController pageContext)
            : base(context, pageContext)
        {
        }

        public static string Name {  get { return "Thanks"; } }

        protected override PageType PageType
        { get { return PageType.Thanks; } }
    }
}