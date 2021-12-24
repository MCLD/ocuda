using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class ProductService : BaseService<ProductService>, IProductService
    {
        private const string LocationNameHeading = "Location of test pickup?";
        private const string NumberOfItemsHeading = "Number of test kits distributed:";

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILocationService _locationService;
        private readonly IProductLocationInventoryRepository _productLocationInventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserService _userService;

        public ProductService(ILogger<ProductService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeProvider dateTimeProvider,
            ILocationService locationService,
            IProductLocationInventoryRepository productLocationInventoryRepository,
            IProductRepository productRepository,
            IUserService userService)
            : base(logger, httpContextAccessor)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _productLocationInventoryRepository = productLocationInventoryRepository
                ?? throw new ArgumentNullException(nameof(productLocationInventoryRepository));
            _productRepository = productRepository
                ?? throw new ArgumentNullException(nameof(productRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<ICollection<string>> BulkInventoryStatusUpdateAsync(int productId,
            bool addValues,
            IDictionary<int, int> adjustments)
        {
            if (adjustments == null)
            {
                throw new ArgumentNullException(nameof(adjustments));
            }

            var issues = new List<string>();
            var now = DateTime.Now;

            if (adjustments.Count > 0)
            {
                foreach (var adjustment in adjustments)
                {
                    if (adjustment.Value != 0)
                    {
                        var inventory = await _productLocationInventoryRepository
                            .GetByProductAndLocationAsync(productId, adjustment.Key);

                        int currentValue = inventory.ItemCount ?? 0;

                        if (addValues)
                        {
                            inventory.ItemCount = currentValue + adjustment.Value;
                        }
                        else
                        {
                            if (currentValue < adjustment.Value)
                            {
                                issues.Add($"Location {inventory.Location.Name}: count would have been less than 0, using 0");
                                inventory.ItemCount = 0;
                            }
                            else
                            {
                                inventory.ItemCount = currentValue - adjustment.Value;
                            }
                        }

                        inventory.UpdatedAt = now;
                        inventory.UpdatedBy = GetCurrentUserId();

                        _productLocationInventoryRepository.Update(inventory);
                    }
                }

                try
                {
                    await _productLocationInventoryRepository.SaveAsync();
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        issues.Add(ex.Message + "-" + ex.InnerException.Message);
                    }
                    else
                    {
                        issues.Add(ex.Message);
                    }
                }
            }
            else
            {
                issues.Add("There were no adjustments to be made.");
            }

            return issues;
        }

        public async Task<ICollection<Product>> GetBySegmentIdAsync(int segmentId)
        {
            return await _productRepository.GetBySegmentIdAsync(segmentId);
        }

        public async Task<Product> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                throw new ArgumentNullException(nameof(slug));
            }

            var formattedSlug = slug
                .Trim()
                .ToLower(System.Globalization.CultureInfo.CurrentCulture);

            return await _productRepository.GetActiveBySlugAsync(formattedSlug);
        }

        public async Task<ProductLocationInventory> GetInventoryByProductAndLocationAsync(int productId,
            int locationId)
        {
            var inventory = await _productLocationInventoryRepository.GetByProductAndLocationAsync(
                productId, locationId);

            if (inventory.UpdatedBy.HasValue)
            {
                (inventory.UpdatedByName, inventory.UpdatedByUsername) =
                    await _userService.GetNameUsernameAsync(inventory.UpdatedBy.Value);
            }

            if (inventory.ThreshholdUpdatedBy.HasValue)
            {
                (inventory.ThreshholdUpdatedByName, inventory.ThreshholdUpdatedByUsername) =
                    await _userService.GetNameUsernameAsync(inventory.ThreshholdUpdatedBy.Value);
            }

            return inventory;
        }

        public async Task<ICollection<ProductLocationInventory>>
                    GetLocationInventoriesForProductAsync(int productId)
        {
            var inventories = await _productLocationInventoryRepository
                .GetForProductAsync(productId);

            foreach (var inventory in inventories)
            {
                if (inventory.UpdatedBy.HasValue)
                {
                    (inventory.UpdatedByName, inventory.UpdatedByUsername) =
                        await _userService.GetNameUsernameAsync(inventory.UpdatedBy.Value);
                }
            }

            return inventories;
        }

        public async Task<ICollectionWithCount<Product>> GetPaginatedListAsync(BaseFilter filter)
        {
            return await _productRepository.GetPaginatedListAsync(filter);
        }

        public async Task LinkSegment(int productId, int segmentId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new OcudaException($"Unable to find product id {productId}");
            }
            product.SegmentId = segmentId;
            _productRepository.Update(product);
            await _productRepository.SaveAsync();
        }

        public async Task<IDictionary<int, int>> ParseInventoryAsync(int productId, string filename)
        {
            var inventory = new Dictionary<int, int>();

            var locations = (await _locationService.GetAllLocationsAsync())
                .ToDictionary(k => k.Name, v => v.Id);

            var locationMap = await _locationService.GetLocationProductMapAsync(productId);

            var locationIssues = new Dictionary<string, int>();
            var issues = new Dictionary<string, string>();

            using (var stream = new System.IO.FileStream(filename, System.IO.FileMode.Open))
            {
                int locationNameColId = 0;
                int numberOfItemsColId = 0;

                int rows = 0;

                using var excelReader = ExcelReaderFactory.CreateReader(stream);
                while (excelReader.Read())
                {
                    rows++;
                    if (rows == 1)
                    {
                        try
                        {
                            for (int col = 0; col < excelReader.FieldCount; col++)
                            {
                                switch (excelReader.GetString(col).Trim() ?? $"Column{col}")
                                {
                                    case LocationNameHeading:
                                        locationNameColId = col;
                                        break;

                                    case NumberOfItemsHeading:
                                        numberOfItemsColId = col;
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new OcudaException($"Unable to find column: {ex.Message}", ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            var count = excelReader.GetDouble(numberOfItemsColId);
                            var location = excelReader.GetString(locationNameColId);

                            if (string.IsNullOrEmpty(location))
                            {
                                issues.Add($"Empty location on row {rows}", null);
                                continue;
                            }

                            int? locationId = null;

                            if (locations.ContainsKey(location.Trim()))
                            {
                                locationId = locations[location.Trim()];
                            }
                            else if (locationMap.ContainsKey(location.Trim()))
                            {
                                locationId = locationMap[location.Trim()];
                            }

                            if (!locationId.HasValue)
                            {
                                if (locationIssues.ContainsKey(location))
                                {
                                    locationIssues[location]++;
                                }
                                else
                                {
                                    locationIssues.Add(location, 1);
                                }
                                continue;
                            }

                            if (inventory.ContainsKey(locationId.Value))
                            {
                                inventory[locationId.Value] += Convert.ToInt32(count);
                            }
                            else
                            {
                                inventory.Add(locationId.Value, Convert.ToInt32(count));
                            }
                        }
                        catch (Exception ex)
                        {
                            issues.Add($"Unable to import row {rows}: {ex.Message}", null);
                        }
                    }
                }
            }

            foreach (var locationIssue in locationIssues)
            {
                issues.Add($"Location '{locationIssue.Key}' could not be mapped on {locationIssue.Value} rows",
                    locationIssue.Key);
            }

            if (issues.Count > 0)
            {
                var ex = new OcudaException("One or more errors were found during the import");
                ex.Data.Add("Issues", issues);
                ex.Data.Add("Inventory", inventory);
                throw ex;
            }

            return inventory;
        }

        public async Task UnlinkSegment(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new OcudaException($"Unable to find product id {productId}");
            }
            product.SegmentId = null;
            _productRepository.Update(product);
            await _productRepository.SaveAsync();
        }

        public async Task UpdateInventoryStatusAsync(int productId, int locationId, int itemCount)
        {
            var currentStatus = await _productLocationInventoryRepository
                .GetByProductAndLocationAsync(productId, locationId);

            currentStatus.ItemCount = itemCount;
            currentStatus.UpdatedAt = _dateTimeProvider.Now;
            currentStatus.UpdatedBy = GetCurrentUserId();

            _productLocationInventoryRepository.Update(currentStatus);
            await _productLocationInventoryRepository.SaveAsync();
        }

        public async Task UpdateThreshholdAsync(int productId, int locationId, int threshholdValue)
        {
            var currentStatus = await _productLocationInventoryRepository
                .GetByProductAndLocationAsync(productId, locationId);

            currentStatus.ManyThreshhold = threshholdValue;
            currentStatus.ThreshholdUpdatedAt = _dateTimeProvider.Now;
            currentStatus.ThreshholdUpdatedBy = GetCurrentUserId();

            _productLocationInventoryRepository.Update(currentStatus);
            await _productLocationInventoryRepository.SaveAsync();
        }
    }
}
