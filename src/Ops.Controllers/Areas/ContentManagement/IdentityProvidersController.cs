using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.IdentityProviders;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Filters;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area(nameof(ContentManagement))]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
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

        [HttpGet("[action]")]
        [RestoreModelState(Key = nameof(IdentityProvidersController))]
        public IActionResult Add()
        {
            return View("Detail");
        }

        [HttpPost("[action]")]
        [SaveModelState(Key = nameof(IdentityProvidersController))]
        public async Task<IActionResult> Add(DetailViewModel viewModel)
        {
            if (viewModel == null || !ModelState.IsValid)
            {
                return RedirectToAction(nameof(Add));
            }

            var provider = new Models.Entities.IdentityProvider
            {
                AssertionConsumerLink = viewModel.AssertionConsumerLink,
                CreatedBy = CurrentUserId,
                EndpointLink = viewModel.EndpointLink,
                EntityId = viewModel.EntityId,
                FormName = "SAMLResponse",
                IsActive = true,
                IsDefault = true,
                Name = viewModel.Name,
                ProviderType = Models.Entities.IdentityProviderType.SAML2,
                Slug = viewModel.Slug
            };

            try
            {
                await _identifyProviderService.AddProviderAsync(provider, viewModel.Certificate);
            }
            catch (OcudaException oex)
            {
                ModelState.AddModelError("InsertError", oex.Message);
                return RedirectToAction(nameof(Add));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Delete(int providerId)
        {
            try
            {
                await _identifyProviderService.DeleteAsync(providerId);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger(oex.Message);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("")]
        [HttpGet("[action]/{page}")]
        public async Task<IActionResult> Index(int page)
        {
            int currentPage = page == default ? 1 : page;

            var filter = new BaseFilter(currentPage);

            var providers = await _identifyProviderService.PageAsync(filter);

            var viewModel = new IndexViewModel
            {
                CurrentPage = currentPage,
                ItemCount = providers.Count,
                ItemsPerPage = filter.Take.Value,
                Providers = providers.Data
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            return View(viewModel);
        }
    }
}