using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.IdentityProviders;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area(nameof(ContentManagement))]
    [Route("[area]/[controller]")]
    public class IdentityProvidersController : BaseController<IdentityProvidersController>
    {
        private readonly IIdentityProviderService _identifyProviderService;

        public IdentityProvidersController(Controller<IdentityProvidersController> context,
            IIdentityProviderService identifyProviderService)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(identifyProviderService);

            _identifyProviderService = identifyProviderService;
        }

        public static string Area
        { get { return nameof(ContentManagement); } }

        public static string Name
        { get { return "IdentityProviders"; } }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var providers = await _identifyProviderService.GetConfiguredProvidersAsync();

            return View(new IdentityProvidersViewModel
            {
                Count = providers.Count,
                Providers = providers.Data
            });
        }
    }
}