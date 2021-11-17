using System;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class NavigationsController : BaseController<NavigationsController>
    {
        private readonly INavigationService _navigationService;

        public NavigationsController(ServiceFacades.Controller<NavigationsController> context,
            INavigationService navigationService)
            : base(context)
        {
            _navigationService = navigationService
                ?? throw new ArgumentNullException(nameof(navigationService));
        }
    }
}
