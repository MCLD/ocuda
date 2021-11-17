using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Products;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class ProductsController : BaseController<ProductsController>
    {
        private readonly ILocationService _locationService;
        private readonly IProductService _productService;

        public ProductsController(ServiceFacades.Controller<ProductsController> context,
            ILocationService locationService,
            IProductService productService)
            : base(context)
        {
            _productService = productService
                ?? throw new ArgumentNullException(nameof(productService));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
        }

        public static string Area
        { get { return "SiteManagement"; } }

        public static string Name
        { get { return "Products"; } }

        [HttpPost("[action]/{productSlug}")]
        public async Task<IActionResult> Confirm(string productSlug,
            bool isReplenishment,
            string adjustmentsJson)
        {
            var product = await _productService.GetBySlugAsync(productSlug);

            if (product == null)
            {
                AlertDanger = "Could not find that product.";
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            var adjustments = JsonSerializer.Deserialize<Dictionary<int, int>>(adjustmentsJson);

            var results = await _productService
                .BulkInventoryStatusUpdateAsync(product.Id, isReplenishment, adjustments);

            if (results.Count > 0)
            {
                var builder = new System.Text.StringBuilder("One or more errors occurred:<ul>");
                foreach (var result in results)
                {
                    builder.Append("<li>").Append(result).Append("</li>");
                }
                builder.Append("</ul>");
                ShowAlertWarning(builder.ToString());
            }

            return RedirectToAction(nameof(Product), new { productSlug });
        }

        [HttpPost("[action]/{productSlug}")]
        public async Task<IActionResult> EditMapping(string productSlug,
            int? locationMapId,
            string importLocation,
            int locationId)
        {
            try
            {
                if (string.IsNullOrEmpty(importLocation))
                {
                    ShowAlertWarning("Please enter text to map to the location.");
                    return RedirectToAction(nameof(Mapping), new { productSlug });
                }

                if (locationMapId.HasValue)
                {
                    await _locationService.UpdateLocationMappingAsync(locationMapId.Value,
                        importLocation,
                        locationId);
                }
                else
                {
                    var product = await _productService.GetBySlugAsync(productSlug);

                    if (product == null)
                    {
                        AlertDanger = "Could not find that product.";
                        return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
                    }

                    await _locationService.AddLocationMappingAsync(product.Id,
                        importLocation,
                        locationId);
                }
            }
            catch (OcudaException oex)
            {
                if (oex.InnerException != null)
                {
                    ShowAlertDanger(oex.InnerException.Message);
                }
                else
                {
                    ShowAlertDanger(oex.Message);
                }
            }

            return RedirectToAction(nameof(Mapping), new { productSlug });
        }

        [HttpGet("{productSlug}/{locationSlug}")]
        public async Task<IActionResult> LocationInventory(string productSlug, string locationSlug)
        {
            var product = await _productService.GetBySlugAsync(productSlug);

            if (product == null)
            {
                AlertDanger = "Could not find that product.";
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            if (int.TryParse(locationSlug, out int locationId))
            {
                var locationLookup = await _locationService.GetLocationByIdAsync(locationId);

                if (locationLookup == null)
                {
                    AlertDanger = "Could not find that location.";
                    return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
                }

                return RedirectToAction(nameof(LocationInventory), new
                {
                    productSlug,
                    locationSlug = locationLookup.Stub
                });
            }

            var location = await _locationService.GetLocationByStubAsync(locationSlug);

            if (location == null)
            {
                AlertDanger = "Could not find that location.";
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            var locationInventory = await _productService.GetInventoryByProductAndLocationAsync(
                product.Id, location.Id);

            if (locationInventory == null)
            {
                AlertDanger = "Unable to find a product inventory for that location.";
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            var viewModel = new LocationInventoryViewModel
            {
                Product = product,
                LocationInventory = locationInventory
            };

            return View(viewModel);
        }

        [HttpGet("{productSlug}/{locationId:int}")]
        public async Task<IActionResult> LocationInventoryById(string productSlug, int locationId)
        {
            var locationLookup = await _locationService.GetLocationByIdAsync(locationId);

            if (locationLookup == null)
            {
                AlertDanger = "Could not find that location.";
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            return RedirectToAction(nameof(LocationInventory), new
            {
                productSlug,
                locationSlug = locationLookup.Stub
            });
        }

        [HttpGet("[action]/{productSlug}")]
        public async Task<IActionResult> Mapping(string productSlug)
        {
            var viewmodel = new MappingViewModel
            {
                Product = await _productService.GetBySlugAsync(productSlug)
            };

            if (viewmodel.Product == null)
            {
                AlertDanger = "Could not find that product.";
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            viewmodel.LocationMap = await _locationService.GetAllLocationProductMapsAsync(viewmodel.Product.Id);
            viewmodel.Locations = await _locationService.GetAllLocationsAsync();

            return View(viewmodel);
        }

        [HttpGet("{productSlug}")]
        public async Task<IActionResult> Product(string productSlug)
        {
            var product = await _productService.GetBySlugAsync(productSlug);

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

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveMapping(string productSlug,
            int locationMapId)
        {
            await _locationService.DeleteMappingAsync(locationMapId);

            return RedirectToAction(nameof(Mapping), new { productSlug });
        }

        [HttpGet("[action]/{productSlug}")]
        public IActionResult UpdateInventory()
        {
            return View();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateInventoryStatus(LocationInventoryViewModel model)
        {
            try
            {
                await _productService.UpdateInventoryStatusAsync(model.ProductId,
                    model.LocationId,
                    model.ItemCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory status: {Message}", ex.Message);
                ShowAlertDanger("Error updating inventory status");
                return RedirectToAction(nameof(LocationInventory), new
                {
                    slug = model.ProductSlug,
                    locationId = model.LocationId
                });
            }

            ShowAlertSuccess($"Inventory status updated for {model.LocationName}");
            return RedirectToAction(nameof(Product), new { productSlug = model.ProductSlug });
        }

        [HttpPost("[action]/{productSlug}")]
        public async Task<IActionResult> UpdateThreshhold(string productSlug,
            string locationSlug,
            int threshholdValue)
        {
            var location = await _locationService.GetLocationByStubAsync(locationSlug);
            var product = await _productService.GetBySlugAsync(productSlug);

            await _productService.UpdateThreshholdAsync(product.Id, location.Id, threshholdValue);

            return RedirectToAction(nameof(LocationInventory), new { productSlug, locationSlug });
        }

        [HttpGet("[action]/{productSlug}")]
        public async Task<IActionResult> UploadDistribution(string productSlug)
        {
            var product = await _productService.GetBySlugAsync(productSlug);

            if (product == null)
            {
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            return View("Upload", new UploadViewModel
            {
                Product = product
            });
        }

        [HttpGet("[action]/{productSlug}")]
        public async Task<IActionResult> UploadReplenishment(string productSlug)
        {
            var product = await _productService.GetBySlugAsync(productSlug);

            if (product == null)
            {
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            return View("Upload", new UploadViewModel
            {
                IsReplenishment = true,
                Product = product
            });
        }

        [HttpPost("[action]/{productSlug}")]
        public async Task<IActionResult> Verify(string productSlug, UploadViewModel viewmodel)
        {
            var product = await _productService.GetBySlugAsync(productSlug);

            if (product == null || viewmodel == null)
            {
                AlertDanger = "Could not find that product.";
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            if (!ModelState.IsValid)
            {
                if (viewmodel.UploadType == "Replenishment")
                {
                    RedirectToAction(nameof(UploadReplenishment), new { productSlug });
                }
                else
                {
                    RedirectToAction(nameof(UploadDistribution), new { productSlug });
                }
            }

            var verifyViewModel = new VerifyUploadViewModel
            {
                IsReplenishment = viewmodel.IsReplenishment,
                LocationInventories = await _productService
                    .GetLocationInventoriesForProductAsync(product.Id),
                Locations = await _locationService.GetAllLocationsAsync(),
                Product = product
            };

            bool valid = true;

            using (_logger.BeginScope("Handling product inventory upload for {Filename}", viewmodel.FileName))
            {
                var tempFile = System.IO.Path.GetTempFileName();
                using (var fileStream = new System.IO.FileStream(tempFile, System.IO.FileMode.Create))
                {
                    await viewmodel.Inventory.CopyToAsync(fileStream);
                }

                try
                {
                    verifyViewModel.Adjustments = await _productService.ParseInventory(product.Id, tempFile);
                }
                catch (OcudaException oex)
                {
                    valid = false;
                    ShowAlertDanger(oex.Message);
                    if (oex?.Data != null && oex.Data.Count > 0)
                    {
                        verifyViewModel.Adjustments = oex.Data["Inventory"] as IDictionary<int, int>;
                        verifyViewModel.Issues = oex.Data["Issues"] as IDictionary<string, string>;
                    }
                }
            }

            if (valid)
            {
                verifyViewModel.AdjustmentsJson = JsonSerializer.Serialize(verifyViewModel.Adjustments);
            }

            return View(verifyViewModel);
        }
    }
}