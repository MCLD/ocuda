using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Products;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class ProductsController : BaseController<ProductsController>
    {
        private readonly ILanguageService _languageService;
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly IProductService _productService;
        private readonly ISegmentService _segmentService;

        public ProductsController(ServiceFacades.Controller<ProductsController> context,
            ILanguageService languageService,
            ILocationService locationService,
            IPermissionGroupService permissionGroupService,
            IProductService productService,
            ISegmentService segmentService)
            : base(context)
        {
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _productService = productService
                ?? throw new ArgumentNullException(nameof(productService));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));
        }

        public static string Area
        { get { return "SiteManagement"; } }

        public static string Name
        { get { return "Products"; } }

        [HttpPost("[action]")]
        public async Task<IActionResult> ActivateLocation(string productSlug, int locationId)
        {
            var product = await _productService.GetBySlugAsync(productSlug);

            if (!CanManage(product))
            {
                return RedirectToUnauthorized();
            }

            await _productService.SetActiveLocation(productSlug, locationId, true);
            var location = await _locationService.GetLocationByIdAsync(locationId);
            ShowAlertSuccess("Location activated");

            return RedirectToAction(nameof(LocationInventory), new
            {
                productSlug,
                locationSlug = location.Stub
            });
        }

        [HttpPost("[action]/{productId}/{permissionGroupId}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> AddPermissionGroup(int productId, int permissionGroupId)
        {
            try
            {
                await _permissionGroupService
                    .AddToPermissionGroupAsync<PermissionGroupProductManager>(productId,
                    permissionGroupId);
                AlertInfo = "Product management permission added.";
            }
            catch (OcudaException oex)
            {
                AlertDanger = $"Problem adding permission: {oex.Message}";
            }

            return RedirectToAction(nameof(Permissions), new { productId });
        }

        [HttpPost]
        [Route("[action]/{productSlug}")]
        public async Task<IActionResult> AddSegment(string productSlug,
            string segmentText)
        {
            if (string.IsNullOrEmpty(productSlug))
            {
                ShowAlertDanger("Invalid add segment request: no product specified.");
                return RedirectToAction(nameof(Index));
            }

            var product = await _productService.GetBySlugAsync(productSlug);

            if (product == null)
            {
                ShowAlertDanger($"Product not found for stub {productSlug}.");
                return RedirectToAction(nameof(Index));
            }

            if (!CanManage(product))
            {
                return RedirectToUnauthorized();
            }

            var languages = await _languageService.GetActiveAsync();

            var defaultLanguage = languages.SingleOrDefault(_ => _.IsActive && _.IsDefault)
                ?? languages.FirstOrDefault(_ => _.IsActive);

            if (defaultLanguage == null)
            {
                ShowAlertDanger("No default language configured.");
                return RedirectToAction(nameof(Details), new { productSlug });
            }

            var segment = await _segmentService.CreateAsync(new Segment
            {
                IsActive = true,
                Name = $"Product - {product.Name}",
            });

            await _segmentService.CreateSegmentTextAsync(new SegmentText
            {
                SegmentId = segment.Id,
                LanguageId = defaultLanguage.Id,
                Text = segmentText
            });

            product.SegmentId = segment.Id;

            try
            {
                await _productService.LinkSegment(product.Id, segment.Id);
            }
            catch (OcudaException oex)
            {
                ShowAlertWarning($"Unable to link segment to product: {oex.Message}");
            }

            return RedirectToAction(nameof(Details), new { productSlug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddUpdate(DetailsViewModel viewModel)
        {
            if (viewModel?.IsNew == true)
            {
                ShowAlertDanger("Unable to create new products at this time - not implemented.");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var product = await _productService.UpdateProductAsync(viewModel.Product);
                if (!CanManage(product))
                {
                    return RedirectToUnauthorized();
                }
                if (viewModel.Product.CacheInventoryMinutes == 0)
                {
                    viewModel.Product.CacheInventoryMinutes = 5;
                    ShowAlertWarning("Cache set to default of 5 minutes.");
                }
                ShowAlertSuccess("Location updated.");
                return RedirectToAction(nameof(Details), new
                {
                    productSlug = product.Slug
                });
            }
        }

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

        [HttpPost("[action]")]
        public async Task<IActionResult> DeactivateLocation(string productSlug, int locationId)
        {
            var product = await _productService.GetBySlugAsync(productSlug);
            if (!CanManage(product))
            {
                return RedirectToUnauthorized();
            }
            await _productService.SetActiveLocation(productSlug, locationId, false);
            ShowAlertWarning("Location deactivated.");
            return RedirectToAction(nameof(Product), new { productSlug });
        }

        [HttpGet("[action]/{productSlug}")]
        public async Task<IActionResult> Details(string productSlug)
        {
            var product = await _productService.GetBySlugAsync(productSlug, true);

            if (product == null)
            {
                return RedirectToAction(nameof(ProductsController.Index), ProductsController.Name);
            }

            var canManage = CanManage(product);

            if (!canManage)
            {
                return RedirectToUnauthorized();
            }

            var viewModel = new DetailsViewModel
            {
                IsProductManager = canManage,
                Product = product
            };

            if (viewModel.Product.SegmentId.HasValue)
            {
                var segment = await _segmentService.GetByIdAsync(viewModel.Product.SegmentId.Value);
                if (segment != null)
                {
                    viewModel.SegmentName = segment.Name;
                }
            }

            return View(viewModel);
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

        [HttpGet("")]
        [HttpGet("[action]/{page}")]
        public async Task<IActionResult> Index(int page)
        {
            int currentPage = page != 0 ? page : 1;

            var filter = new BaseFilter(currentPage);

            var productList = await _productService.GetPaginatedListAsync(filter);

            var viewModel = new IndexViewModel(productList.Data, UserClaims(ClaimType.PermissionId))
            {
                BaseLink = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.SiteManagement.PromenadeUrl),
                CurrentPage = currentPage,
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)),
                ItemCount = productList.Count,
                ItemsPerPage = filter.Take.Value
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = viewModel.LastPage ?? 1
                    });
            }

            return View(viewModel);
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

            if (!CanManage(viewmodel.Product))
            {
                return RedirectToUnauthorized();
            }

            if (viewmodel.Product == null)
            {
                AlertDanger = "Could not find that product.";
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            viewmodel.LocationMap = await _locationService.GetAllLocationProductMapsAsync(viewmodel.Product.Id);
            viewmodel.Locations = await _locationService.GetAllLocationsAsync();

            return View(viewmodel);
        }

        [HttpGet("[action]/{productId}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Permissions(int productId)
        {
            var product = await _productService.GetByIdAsync(productId);

            var permissionGroups = await _permissionGroupService.GetAllAsync();
            var permissions = await _permissionGroupService
                .GetPermissionsAsync<PermissionGroupProductManager>(product.Id);

            var viewModel = new PermissionsViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Slug = product.Slug
            };

            foreach (var permissionGroup in permissionGroups)
            {
                var permission = permissions
                    .SingleOrDefault(_ => _.PermissionGroupId == permissionGroup.Id);
                if (permission == null)
                {
                    viewModel
                        .AvailableGroups
                        .Add(permissionGroup.Id, permissionGroup.PermissionGroupName);
                }
                else
                {
                    viewModel
                        .AssignedGroups
                        .Add(permissionGroup.Id, permissionGroup.PermissionGroupName);
                }
            }

            return View(viewModel);
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

            var locations = await _locationService.GetAllLocationsAsync();

            var locationIdsWithInventories = locationInventories.Select(_ => _.LocationId).ToList();

            var excludedLocations = locations.Where(_ => !locationIdsWithInventories.Contains(_.Id))
                .ToDictionary(k => k.Id, v => v.Name);

            var productPermission = UserClaims(ClaimType.PermissionId)?
                .Any(_ => product.PermissionGroupIds.Contains(_)) == true;

            var viewModel = new ProductViewModel
            {
                ExcludedLocations = excludedLocations,
                IsProductManager = IsSiteManager() || productPermission,
                Product = product,
                LocationInventories = locationInventories
            };

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveMapping(string productSlug,
            int locationMapId)
        {
            var product = await _productService.GetBySlugAsync(productSlug);
            if (product == null)
            {
                return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            if (!CanManage(product))
            {
                return RedirectToUnauthorized();
            }

            await _locationService.DeleteMappingAsync(locationMapId);

            return RedirectToAction(nameof(Mapping), new { productSlug });
        }

        [HttpPost("[action]/{productId}/{permissionGroupId}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> RemovePermissionGroup(int productId, int permissionGroupId)
        {
            try
            {
                await _permissionGroupService
                    .RemoveFromPermissionGroupAsync<PermissionGroupProductManager>(productId,
                    permissionGroupId);
                AlertInfo = "Product management permission removed.";
            }
            catch (OcudaException oex)
            {
                AlertDanger = $"Problem removing permission: {oex.Message}";
            }

            return RedirectToAction(nameof(Permissions), new { productId });
        }

        [HttpPost]
        [Route("[action]/{productSlug}")]
        public async Task<IActionResult> RemoveSegment(string productSlug)
        {
            if (string.IsNullOrEmpty(productSlug))
            {
                ShowAlertDanger("Invalid remove segment request: no product specified.");
                return RedirectToAction(nameof(Index));
            }

            var product = await _productService.GetBySlugAsync(productSlug);

            if (product == null)
            {
                ShowAlertDanger($"Product not found for slug {productSlug}.");
                return RedirectToAction(nameof(Index));
            }

            if (!CanManage(product))
            {
                return RedirectToUnauthorized();
            }

            if (!product.SegmentId.HasValue)
            {
                ShowAlertDanger($"Segment not linked to product {product.Name}.");
                return RedirectToAction(nameof(Details), new { productSlug });
            }

            try
            {
                await _productService.UnlinkSegment(product.Id);
            }
            catch (OcudaException oex)
            {
                ShowAlertWarning($"Unable to unlink segment from product: {oex.Message}");
                return RedirectToAction(nameof(Details), new { productSlug });
            }

            try
            {
                await _segmentService.DeleteAsync(product.SegmentId.Value);
                ShowAlertSuccess("Segment removed and deleted.");
            }
            catch (OcudaException oex)
            {
                string message = oex.Message;
                if (oex.Data[OcudaExceptionData.SegmentInUseBy] is ICollection<string> inUseList)
                {
                    message = $"in use by {inUseList.Count} other locations";
                }
                ShowAlertWarning($"Segment removed from this product but not deleted: {message}");
            }

            return RedirectToAction(nameof(Details), new { productSlug });
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
                    verifyViewModel.Adjustments = await _productService.ParseInventoryAsync(product.Id, tempFile);
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

        private bool CanManage(Product product)
        {
            return IsSiteManager() || UserClaims(ClaimType.PermissionId)?
                .Any(_ => product.PermissionGroupIds.Contains(_)) == true;
        }
    }
}
