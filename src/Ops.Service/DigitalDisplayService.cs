using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Service
{
    public class DigitalDisplayService
        : BaseService<DigitalDisplayService>, IDigitalDisplayService
    {
        private const string BaseFilePath = "digitaldisplay";

        private readonly IDistributedCache _cache;
        private readonly IDigitalDisplayAssetRepository _digitalDisplayAssetRepository;
        private readonly IDigitalDisplayAssetSetRepository _digitalDisplayAssetSetRepository;
        private readonly IDigitalDisplayDisplaySetRepository _digitalDisplayDisplaySetRepository;
        private readonly IDigitalDisplayItemRepository _digitalDisplayItemRepository;
        private readonly IDigitalDisplayRepository _digitalDisplayRepository;
        private readonly IDigitalDisplaySetRepository _digitalDisplaySetRepository;
        private readonly IPathResolverService _pathResolver;
        private readonly IUserRepository _userRepository;

        public DigitalDisplayService(ILogger<DigitalDisplayService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDistributedCache cache,
            IDigitalDisplayAssetRepository digitalDisplayAssetRepository,
            IDigitalDisplayAssetSetRepository digitalDisplayAssetSetRepository,
            IDigitalDisplayItemRepository digitalDisplayItemRepository,
            IDigitalDisplayDisplaySetRepository digitalDisplayDisplaySetRepository,
            IDigitalDisplayRepository digitalDisplayRepository,
            IDigitalDisplaySetRepository digitalDisplaySetRepository,
            IPathResolverService pathResolver,
            IUserRepository userRepository)
            : base(logger, httpContextAccessor)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _digitalDisplayAssetRepository = digitalDisplayAssetRepository
                ?? throw new ArgumentNullException(nameof(digitalDisplayAssetRepository));
            _digitalDisplayAssetSetRepository = digitalDisplayAssetSetRepository
                ?? throw new ArgumentNullException(nameof(digitalDisplayAssetSetRepository));
            _digitalDisplayDisplaySetRepository = digitalDisplayDisplaySetRepository
                ?? throw new ArgumentNullException(nameof(digitalDisplayDisplaySetRepository));
            _digitalDisplayItemRepository = digitalDisplayItemRepository
                ?? throw new ArgumentNullException(nameof(digitalDisplayItemRepository));
            _digitalDisplayRepository = digitalDisplayRepository
                ?? throw new ArgumentNullException(nameof(digitalDisplayRepository));
            _digitalDisplaySetRepository = digitalDisplaySetRepository
                ?? throw new ArgumentNullException(nameof(digitalDisplaySetRepository));
            _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public Task<DigitalDisplayAsset> AddAssetAsync(DigitalDisplayAsset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }
            return AddAssetInternalAsync(asset);
        }

        public Task AddDisplayAsync(DigitalDisplay display)
        {
            if (display == null)
            {
                throw new ArgumentNullException(nameof(display));
            }
            return AddDisplayInternalAsync(display);
        }

        public Task AddSetAsync(DigitalDisplaySet displaySet)
        {
            if (displaySet == null)
            {
                throw new ArgumentNullException(nameof(displaySet));
            }
            return AddSetInternalAsync(displaySet);
        }

        public async Task
            AddUpdateDigitalDisplayAssetSetAsync(DigitalDisplayAssetSet assetSet)
        {
            if (assetSet == null)
            {
                throw new OcudaException("Cannot update empty asset set request.");
            }

            var exists = await _digitalDisplayAssetSetRepository
                .ExistsAsync(assetSet.DigitalDisplayAssetId,
                    assetSet.DigitalDisplaySetId);

            if (exists)
            {
                _digitalDisplayAssetSetRepository.Update(assetSet);
            }
            else
            {
                await _digitalDisplayAssetSetRepository.AddAsync(assetSet);
            }

            var set = await _digitalDisplaySetRepository
                .FindAsync(assetSet.DigitalDisplaySetId);
            set.LastContentUpdate = DateTime.Now;
            _digitalDisplaySetRepository.Update(set);

            await _digitalDisplayAssetSetRepository.SaveAsync();
        }

        public async Task<IEnumerable<int>> AssignSetAsync(int displayId, int setId)
        {
            var sets = await _digitalDisplayDisplaySetRepository
                .GetByDisplayIdsAsync(new[] { displayId });
            var hasValue = sets.SingleOrDefault(_ => _.DigitalDisplaySetId == setId);
            if (hasValue != null)
            {
                _digitalDisplayDisplaySetRepository.Remove(hasValue);
            }
            else
            {
                await _digitalDisplayDisplaySetRepository.AddAsync(new DigitalDisplayDisplaySet
                {
                    DigitalDisplayId = displayId,
                    DigitalDisplaySetId = setId
                });
            }
            await _digitalDisplayDisplaySetRepository.SaveAsync();

            sets = await _digitalDisplayDisplaySetRepository
                .GetByDisplayIdsAsync(new[] { displayId });

            return sets.Select(_ => _.DigitalDisplaySetId);
        }

        public async Task DeleteAssetAsync(int digitalDisplayAssetId)
        {
            var asset = await _digitalDisplayAssetRepository.FindAsync(digitalDisplayAssetId);

            if (asset == null)
            {
                throw new OcudaException($"Unable to find asset id {digitalDisplayAssetId} to delete it");
            }

            _digitalDisplayItemRepository.RemoveByAssetId(digitalDisplayAssetId);
            await _digitalDisplayItemRepository.SaveAsync();
            try
            {
                System.IO.File.Delete(GetAssetPath(asset.Path));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unable to delete asset id {AssetId} file {FilePath}: {ErrorMessage}",
                    asset.Id,
                    asset.Path,
                    ex.Message);
                throw new OcudaException("Unable to delete asset file.", ex);
            }

            _digitalDisplayAssetRepository.Remove(asset);
            await _digitalDisplayAssetRepository.SaveAsync();
        }

        public async Task DeleteDisplayAsync(int displayId)
        {
            var display = await _digitalDisplayRepository.FindAsync(displayId);
            _digitalDisplayRepository.Remove(display);
            await _digitalDisplayRepository.SaveAsync();
        }

        public async Task DeleteSetAsync(int setId)
        {
            var set = await _digitalDisplaySetRepository.FindAsync(setId);
            _digitalDisplaySetRepository.Remove(set);
            await _digitalDisplaySetRepository.SaveAsync();
        }

        public async Task<DigitalDisplayAsset> FindAssetByChecksumAsync(byte[] checksum)
        {
            return await _digitalDisplayAssetRepository.FindByChecksumAsync(checksum);
        }

        public async Task<DigitalDisplayAsset> GetAssetAsync(int digitalDisplayAssetId)
        {
            var asset = await _digitalDisplayAssetRepository.FindAsync(digitalDisplayAssetId);
            asset.CreatedByUser = await _userRepository.FindAsync(asset.CreatedBy);
            return asset;
        }

        public string GetAssetPath(string fileName)
        {
            return _pathResolver.GetPublicContentFilePath(fileName, BaseFilePath);
        }

        public Task<ICollection<DigitalDisplayAssetSet>> GetAssetSetsAsync(int assetId)
        {
            return _digitalDisplayAssetSetRepository.GetByAssetIdAsync(assetId);
        }

        public async Task<IDictionary<int, int>> GetAssetSetsCountAsync(IEnumerable<int> assetIds)
        {
            var setCount = new Dictionary<int, int>();
            if (assetIds != null)
            {
                foreach (int assetId in assetIds)
                {
                    setCount.Add(assetId, await _digitalDisplayAssetSetRepository
                        .GetSetCountContainingAsset(assetId));
                }
            }
            return setCount;
        }

        public IDictionary<int, string> GetAssetUrlPaths(IEnumerable<DigitalDisplayAsset> assets)
        {
            if (assets == null)
            {
                throw new ArgumentNullException(nameof(assets));
            }

            return assets
                .ToDictionary(k => k.Id,
                    v => _pathResolver.GetPublicContentLink(BaseFilePath, v.Path));
        }

        public string GetAssetWebPath(DigitalDisplayAsset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }
            return _pathResolver.GetPublicContentLink(BaseFilePath, asset.Path);
        }

        public async Task<DigitalDisplay> GetDisplayAsync(int displayId)
        {
            var display = await _digitalDisplayRepository.FindAsync(displayId);
            display.Status = await GetDisplayStatusAsync(displayId);
            return display;
        }

        public async Task<ICollection<DigitalDisplay>> GetDisplaysAsync()
        {
            var displays = await _digitalDisplayRepository.GetAllAsync();
            foreach (var display in displays)
            {
                display.Status = await GetDisplayStatusAsync(display.Id);
            }
            return displays;
        }

        public async Task<ICollection<DigitalDisplayDisplaySet>>
            GetDisplaysSetsAsync(IEnumerable<int> displayIds)
        {
            return await _digitalDisplayDisplaySetRepository.GetByDisplayIdsAsync(displayIds);
        }

        public async Task<(DateTime AsOf, string Message)> GetDisplayStatusAsync(int displayId)
        {
            string statusKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.OpsDigitalDisplayStatus,
                displayId);
            string statusAtKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.OpsDigitalDisplayStatusAt,
                displayId);

            string message = await _cache.GetStringAsync(statusKey);

            var asOfTicks = await _cache.GetAsync(statusAtKey);
            var asOf = asOfTicks != null
                ? DateTime.FromBinary(BitConverter.ToInt64(asOfTicks, 0))
                : default;

            return (asOf, message);
        }

        public Task<DataWithCount<ICollection<DigitalDisplayAsset>>>
            GetPaginatedAssetsAsync(BaseFilter filter)
        {
            return _digitalDisplayAssetRepository.GetPaginatedListAsync(filter);
        }

        public async Task<DigitalDisplaySet> GetSetAsync(int setId)
        {
            return await _digitalDisplaySetRepository.FindAsync(setId);
        }

        public async Task<DigitalDisplaySet> GetSetAsync(string setName)
        {
            return await _digitalDisplaySetRepository.GetByName(setName);
        }

        public async Task<IDictionary<int, int>> GetSetsAssetCountsAsync()
        {
            return await _digitalDisplayAssetSetRepository.GetSetsAssetCountsAsync();
        }

        public async Task<ICollection<DigitalDisplaySet>> GetSetsAsync()
        {
            return await _digitalDisplaySetRepository.GetAllAsync();
        }

        public async Task<IDictionary<int, int>> GetSetsDisplaysCountsAsync()
        {
            return await _digitalDisplayDisplaySetRepository.GetSetsDisplaysCountsAsync();
        }

        public async Task RemoveDigitalDisplayAssetSetAsync(int assetId, int setId)
        {
            var sets = await _digitalDisplayAssetSetRepository.GetByAssetIdAsync(assetId);
            var assetSet = sets.SingleOrDefault(_ => _.DigitalDisplaySetId == setId);
            if (assetSet != null)
            {
                _digitalDisplayAssetSetRepository.Remove(assetSet);
                await _digitalDisplayAssetSetRepository.SaveAsync();
            }
            else
            {
                throw new OcudaException($"Unable to find asset id {assetId} for set {setId}");
            }
        }

        public async Task SetDisplayStatusAsync(int displayId, string status)
        {
            string statusKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.OpsDigitalDisplayStatus,
                displayId);
            string statusAtKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.OpsDigitalDisplayStatusAt,
                displayId);

            if (string.IsNullOrEmpty(status))
            {
                await _cache.RemoveAsync(statusKey);
                await _cache.RemoveAsync(statusAtKey);
            }
            else
            {
                await _cache.SetStringAsync(statusKey, status);
                await _cache.SetAsync(statusAtKey, BitConverter.GetBytes(DateTime.Now.Ticks));
            }
        }

        public async Task UpdateDisplayAsync(DigitalDisplay display)
        {
            var updateDisplay = display ?? throw new ArgumentNullException(nameof(display));

            var databaseDisplay = await _digitalDisplayRepository.FindAsync(display.Id);
            if (databaseDisplay == null)
            {
                throw new OcudaException($"Unable to find digital display id {display.Id}");
            }

            // these are system variables and are not editable
            updateDisplay.LastAttempt = databaseDisplay.LastAttempt;
            updateDisplay.LastCommunication = databaseDisplay.LastCommunication;
            updateDisplay.LastContentVerification = databaseDisplay.LastContentVerification;
            updateDisplay.SlideCount = databaseDisplay.SlideCount;
            updateDisplay.CreatedBy = databaseDisplay.CreatedBy;
            updateDisplay.CreatedAt = databaseDisplay.CreatedAt;
            updateDisplay.UpdatedBy = GetCurrentUserId();
            updateDisplay.UpdatedAt = DateTime.Now;

            _digitalDisplayRepository.Update(updateDisplay);
            await _digitalDisplayRepository.SaveAsync();
        }

        public async Task UpdateSetAsync(DigitalDisplaySet displaySet)
        {
            var updateSet = displaySet ?? throw new ArgumentNullException(nameof(displaySet));

            var databaseSet = await _digitalDisplaySetRepository.FindAsync(displaySet.Id);
            if (databaseSet == null)
            {
                throw new OcudaException($"Unable to find digital display set id {displaySet.Id}");
            }

            databaseSet.Name = updateSet.Name;
            databaseSet.UpdatedAt = DateTime.Now;
            databaseSet.UpdatedBy = GetCurrentUserId();

            _digitalDisplaySetRepository.Update(databaseSet);
            await _digitalDisplayRepository.SaveAsync();
        }

        private async Task<DigitalDisplayAsset> AddAssetInternalAsync(DigitalDisplayAsset asset)
        {
            asset.CreatedBy = GetCurrentUserId();
            asset.CreatedAt = DateTime.Now;

            await _digitalDisplayAssetRepository.AddAsync(asset);
            await _digitalDisplayAssetRepository.SaveAsync();

            return asset;
        }

        private async Task AddDisplayInternalAsync(DigitalDisplay display)
        {
            display.CreatedBy = GetCurrentUserId();
            display.CreatedAt = DateTime.Now;

            await _digitalDisplayRepository.AddAsync(display);
            await _digitalDisplayRepository.SaveAsync();
            await SetDisplayStatusAsync(display.Id, "Provisioned, awaiting initial sync");
        }

        private async Task AddSetInternalAsync(DigitalDisplaySet digitalDisplaySet)
        {
            digitalDisplaySet.CreatedBy = GetCurrentUserId();
            digitalDisplaySet.CreatedAt = DateTime.Now;

            await _digitalDisplaySetRepository.AddAsync(digitalDisplaySet);
            await _digitalDisplaySetRepository.SaveAsync();
        }
    }
}