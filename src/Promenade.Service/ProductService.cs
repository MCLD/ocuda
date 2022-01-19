using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class ProductService : BaseService<ProductService>
    {
        private readonly IOcudaCache _cache;
        private readonly IConfiguration _config;
        private readonly IProductInventoryRepository _productInventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly SegmentService _segmentService;

        public ProductService(ILogger<ProductService> logger,
            IDateTimeProvider dateTimeProvider,
            IConfiguration config,
            IOcudaCache cache,
            IProductInventoryRepository productInventoryRepository,
            IProductRepository productRepository,
            SegmentService segmentService) : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _productInventoryRepository = productInventoryRepository
                ?? throw new ArgumentNullException(nameof(productInventoryRepository));
            _productRepository = productRepository
                ?? throw new ArgumentNullException(nameof(productRepository));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));
        }

        public async Task<ICollection<ProductLocationInventory>>
            GetInventoriesAsync(int productId, int cacheInMinutes, bool forceReload)
        {
            int cacheDurationMinutes = cacheInMinutes > 0 ? cacheInMinutes : 5;

            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromProductInventory,
                productId);

            ICollection<ProductLocationInventory> inventory = null;

            if (!forceReload)
            {
                inventory = await _cache
                    .GetObjectFromCacheAsync<ICollection<ProductLocationInventory>>(cacheKey);
            }

            if (inventory == null)
            {
                inventory = await _productInventoryRepository.GetAsync(productId);
                await _cache.SaveToCacheAsync(cacheKey,
                    inventory,
                    TimeSpan.FromMinutes(cacheDurationMinutes));
            }
            return inventory;
        }

        public async Task<Product> GetProductAsync(string slug, bool forceReload)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromProduct,
                slug);

            Product product = null;

            if (!forceReload)
            {
                product = await _cache.GetObjectFromCacheAsync<Product>(cacheKey);
            }

            if (product == null)
            {
                product = await _productRepository.GetAsync(slug);

                var cachePagesInHours = GetPageCacheDuration(_config);

                await _cache.SaveToCacheAsync(cacheKey, product, cachePagesInHours);
            }

            if (product?.SegmentId.HasValue == true)
            {
                product.SegmentText = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(product.SegmentId.Value, forceReload);
            }

            return product;
        }

        public async Task<ICollection<Product>> GetProductsAsync(bool forceReload)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromProductsSlugs);

            ICollection<Product> products = null;

            if (!forceReload)
            {
                products = await _cache.GetObjectFromCacheAsync<ICollection<Product>>(cacheKey);
            }

            if (products == null)
            {
                products = await _productRepository.GetAllNamesSlugsAsync();

                var cachePagesInHours = GetPageCacheDuration(_config);

                await _cache.SaveToCacheAsync(cacheKey, products, cachePagesInHours);
            }

            return products;
        }
    }
}
