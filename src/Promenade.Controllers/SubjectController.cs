using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class SubjectController : GeneralBasePageController<SubjectController>
    {
        public SubjectController(ServiceFacades.Controller<SubjectController> context,
            ServiceFacades.PageController pageContext)
            : base(context, pageContext)
        {
        }

        protected override PageType PageType
        { get { return PageType.Subject; } }
    }
}