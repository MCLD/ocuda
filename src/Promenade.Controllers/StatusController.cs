using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ServiceFacades;
using Ocuda.Promenade.Controllers.ViewModels.Status;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class StatusController : BaseController<StatusController>
    {
        private readonly LocationService _locationService;
        private readonly ProductService _productService;

        public StatusController(Controller<StatusController> context,
            LocationService locationService,
            ProductService productService) : base(context)
        {
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _productService = productService
                ?? throw new ArgumentNullException(nameof(productService));
        }

        public static string Name
        { get { return "Status"; } }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var products = await _productService.GetProductsAsync(forceReload);

            if (products.Count == 1)
            {
                return RedirectToAction(nameof(Product), new { slug = products.First().Slug });
            }
            else if (products.Count == 0)
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound);
            }

            throw new NotImplementedException();
            return View();
        }

        [Route("{slug}")]
        public async Task<IActionResult> Product(string slug)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var product = await _productService.GetProductAsync(slug, forceReload);

            if (product == null)
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound);
            }

            var inventories = await _productService.GetInventoriesAsync(product.Id, forceReload);
            var locations = await _locationService.GetLocationsStatusAsync(null, null);

            var locationInventories = new List<LocationInventory>();

            var hasItems = _localizer[i18n.Keys.Promenade.ProductInventoryHas, product.Name];
            var noItems = _localizer[i18n.Keys.Promenade.ProductInventoryDoesNotHave, product.Name];

            foreach (var inventory in inventories)
            {
                var location = locations.SingleOrDefault(_ => _.Id == inventory.LocationId);
                locationInventories.Add(new LocationInventory
                {
                    UpdatedAt = inventory.UpdatedAt ?? inventory.CreatedAt,
                    CurrentStatus = location.CurrentStatus.StatusMessage,
                    CurrentStatusClass = location.CurrentStatus.IsCurrentlyOpen
                        ? "text-success"
                        : location.CurrentStatus.IsSpecialHours ? "text-primary" : "text-danger",
                    InventoryStatus = inventory.ItemCount > 0 ? hasItems : noItems,
                    InventoryStatusClass = inventory.ItemCount > 0 ? "text-success" : "text-danger",
                    Name = location.Name,
                    Stub = location.Stub
                });
            }

            PageTitle = _localizer[i18n.Keys.Promenade.ProductInventoryCurrentAvailability,
                product.Name];

            var viewModel = new ProductInventoryViewModel
            {
                Title = PageTitle,
            };

            viewModel.LocationInventories.AddRange(locationInventories.OrderBy(_ => _.Name));

            return View(viewModel);
        }
    }
}
