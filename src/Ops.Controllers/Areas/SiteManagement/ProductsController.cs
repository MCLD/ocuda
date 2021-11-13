using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Products;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class ProductsController : BaseController<ProductsController>
    {
        private readonly IProductService _productService;

        public static string Name { get { return "Products"; } }
        public static string Area { get { return "SiteManagement"; } }

        public ProductsController(ServiceFacades.Controller<ProductsController> context,
            IProductService productService)
            : base(context)
        {
            _productService = productService
                ?? throw new ArgumentNullException(nameof(productService));
        }

        [Route("{slug}")]
        public async Task<IActionResult> Product(string slug)
        {
            var product = await _productService.GetBySlugAsnyc(slug);

            if (product == null)
            {
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            var locationInventories = await _productService
                .GetLocationInventoriesForProductAsync(product.Id);

            var viewModel = new ProductViewModel
            {
                Product = product,
                LocationInventories = locationInventories
            };

            return View(viewModel);
        }

        [Route("{slug}/{locationId}")]
        public async Task<IActionResult> LocationInventory(string slug, int locationId)
        {
            var product = await _productService.GetBySlugAsnyc(slug);

            if (product == null)
            {
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            var locationInventory = await _productService.GetInventoryByProductAndLocation(
                product.Id, locationId);

            if (locationInventory == null)
            {
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            var viewModel = new LocationInventoryViewModel
            {
                Product = product,
                LocationInventory = locationInventory
            };

            return View(viewModel);
        }
    }
}
