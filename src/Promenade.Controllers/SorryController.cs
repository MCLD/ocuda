using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class SorryController : GeneralBasePageController<SorryController>
    {
        public SorryController(ServiceFacades.Controller<SorryController> context,
            ServiceFacades.PageController pageContext)
            : base(context, pageContext)
        {
        }

        public static string Name
        { get { return "Sorry"; } }

        protected override PageType PageType
        { get { return PageType.Sorry; } }
    }
}