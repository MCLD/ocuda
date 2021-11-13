using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Abstract;

namespace Ocuda.Ops.Service
{
    public class ProductService : BaseService<ProductService>, IProductService
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IProductLocationInventoryRepository _productLocationInventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserService _userService;

        public ProductService(ILogger<ProductService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeProvider dateTimeProvider,
            IProductLocationInventoryRepository productLocationInventoryRepository,
            IProductRepository productRepository,
            IUserService userService)
            : base(logger, httpContextAccessor)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _productLocationInventoryRepository = productLocationInventoryRepository
                ?? throw new ArgumentNullException(nameof(productLocationInventoryRepository));
            _productRepository = productRepository
                ?? throw new ArgumentNullException(nameof(productRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<Product> GetBySlugAsnyc(string slug)
        {
            var formattedSlug = slug.Trim().ToLower();

            return await _productRepository.GetActiveBySlugAsync(formattedSlug);
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

        public async Task<ProductLocationInventory> GetInventoryByProductAndLocation(int productId,
            int locationId)
        {
            var inventory = await _productLocationInventoryRepository.GetByProductAndLocationAsync(
                productId, locationId);

            if (inventory.UpdatedBy.HasValue)
            {
                (inventory.UpdatedByName, inventory.UpdatedByUsername) =
                    await _userService.GetNameUsernameAsync(inventory.UpdatedBy.Value);
            }

            return inventory;
        }

        public async Task UpdateInventoryStatus(int productId, int locationId,
            ProductLocationInventory.Status status)
        {
            var currentStatus = await _productLocationInventoryRepository
                .GetByProductAndLocationAsync(productId, locationId);

            currentStatus.InventoryStatus = status;
            currentStatus.UpdatedAt = _dateTimeProvider.Now;
            currentStatus.UpdatedBy = GetCurrentUserId();

            _productLocationInventoryRepository.Update(currentStatus);
            await _productLocationInventoryRepository.SaveAsync();
        }
    }
}
